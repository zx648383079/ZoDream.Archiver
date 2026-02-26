using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using ZoDream.LuaDecompiler;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Language;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Eastward
{
    public class EastwardAssetBundle(IBundleSource source, string configPath, IArchiveScheme scheme, IBundleOptions options) : IBundleSource, IBundleHandler
    {

        private string _assetLibrary = "config/asset_index";
        private string _scriptLibrary = "config/script_library";
        private string _textureLibrary = "config/texture_index";
        private readonly string _rootFolder = Path.GetDirectoryName(configPath)!;
        private readonly Dictionary<string, GReader> _archiveItems = [];
        private readonly Dictionary<string, EastwardAssetInfo> _items = [];
        private readonly Dictionary<string, string> _scriptItems = [];
        private readonly Dictionary<string, string> _textureItems = [];

        public uint Count => source.Count;

        private bool TryGetArchive(string fileName, out string entryPtah, [NotNullWhen(true)] out GReader? reader)
        {
            if (fileName.StartsWith("game/"))
            {
                fileName = fileName[5..];
            }
            var args = fileName.Split('/', 2);
            entryPtah = args.Length > 0 ? args[1] : string.Empty;
            if (_archiveItems.TryGetValue(args[0], out reader))
            {
                return true;
            }
            var target = Path.Combine(_rootFolder, args[0] + ".g");
            if (!source.Exists(target))
            {
                return false;
            }
            var fs = source.OpenRead(new FilePath(target));
            reader = new GReader(new EndianReader(fs, EndianType.LittleEndian, false));
            _archiveItems.Add(args[0], reader);
            return true;
        }

        private void ReadDocument(string fileName, Action<JsonDocument> cb)
        {
            if (!TryGetArchive(fileName, out var entryPath, out var archive))
            {
                return;
            }
            archive.ReadDocument(entryPath, cb);
        }

        private void ReadAs(string fileName, Action<Stream> cb)
        {
            if (TryGetArchive(fileName, out var entryPath, out var archive))
            {
                archive.ExtractTo(entryPath, cb);
                return;
            }
            if (fileName.StartsWith("game/"))
            {
                fileName = fileName[5..];
            }
            var target = Path.Combine(_rootFolder, fileName);
            if (File.Exists(target))
            {
                using var fs = File.OpenRead(target);
                cb.Invoke(fs);
            }
        }

        public uint Analyze(CancellationToken token = default)
        {
            ReadDocument("config/game_config", doc => {
                var root = doc.RootElement;
                if (root.TryGetProperty("asset_library", out var ele))
                {
                    _assetLibrary = ele.GetString() ?? _assetLibrary;
                }
                if (root.TryGetProperty("script_library", out ele))
                {
                    _scriptLibrary = ele.GetString() ?? _scriptLibrary;
                }
                if (root.TryGetProperty("texture_library", out ele))
                {
                    _textureLibrary = ele.GetString() ?? _textureLibrary;
                }
            });
            LoadAssetLibrary(_assetLibrary);
            LoadScriptLibrary(_scriptLibrary);
            LoadTextureIndex(_textureLibrary);
            return (uint)_items.Count;
        }

     

        public bool Exists(string filePath)
        {
            return source.Exists(filePath);
        }

        public IEnumerable<string> FindFiles(string folder, string fileName)
        {
            return source.FindFiles(folder, fileName);
        }

        public IEnumerable<string> GetDirectories(params string[] searchPatternItems)
        {
            return source.GetDirectories(searchPatternItems);
        }

        public IEnumerable<string> GetFiles(params string[] searchPatternItems)
        {
            foreach (var item in _items)
            {
                if (searchPatternItems.Length == 0)
                {
                    yield return item.Key;
                    continue;
                }
                if (BundleStorage.IsMatch(Path.GetFileName(item.Key), searchPatternItems))
                {
                    yield return item.Key;
                    continue;
                }
            }
            foreach (var item in source.GetFiles(searchPatternItems))
            {
                if (item == configPath)
                {
                    continue;
                }
                yield return item;
            }
        }

        public string GetRelativePath(string filePath)
        {
            return source.GetRelativePath(filePath);
        }

        public IEnumerable<string> Glob(params string[] searchPatternItems)
        {
            return source.Glob(searchPatternItems);
        }

        public Stream OpenRead(IFilePath filePath)
        {
            if (filePath is IEntryPath o && _archiveItems.TryGetValue(o.FilePath, out var archive))
            {
                return archive.ReadAsStream(o.EntryPath);
            }
            return source.OpenRead(filePath);
        }

        public Stream OpenWrite(IFilePath filePath)
        {
            return source.OpenWrite(filePath);
        }

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            Analyze(token);
            var bundleFolder = Path.Combine(folder, Path.GetFileName(_rootFolder));
#if DEBUG
            SaveRaw(Path.Combine(bundleFolder, _assetLibrary), _assetLibrary, mode);
            SaveRaw(Path.Combine(bundleFolder, _textureLibrary), _textureLibrary, mode);
            SaveRaw(Path.Combine(bundleFolder, _scriptLibrary), _scriptLibrary, mode);
#endif
            foreach (var item in _items)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (item.Value.Type is "msprite")
                {
                    // 不能单独导出，需要跟 texture 导出
                    continue;
                }
                ExtractTo(bundleFolder, mode, item.Value, item.Key);
            }
            foreach (var item in Directory.GetFiles(_rootFolder, "*.bank", SearchOption.AllDirectories))
            {
                using var fs = File.OpenRead(item);
                using var reader = scheme.Open(fs, item, Path.GetFileName(item));
                if (reader is null)
                {
                    continue;
                }
                var outputPath = Path.Combine(bundleFolder, Path.GetRelativePath(_rootFolder, item));
                reader?.ExtractToDirectory(Path.GetDirectoryName(outputPath)!, mode, null, token);
            }
        }

        private void ExtractTo(string folder, ArchiveExtractMode mode, EastwardAssetInfo assetInfo, string exportName)
        {
            var assetName = assetInfo.AssetName;
            var fileType = assetInfo.FileType;
            var objectFiles = assetInfo.ObjectFiles;
            var type = assetInfo.Type;

            if (objectFiles.Count == 0 && type != "lua" && type != "texture")
            {
                    return;
            }
            switch (type)
            {
                case "folder":
                case "file":
                    return;
                case "deck2d.mquad":
                case "deck2d.mtileset":
                case "deck2d.quad":
                case "deck2d.quads":
                case "deck2d.stretchpatch":
                case "deck2d.tileset":
                case "deck2d.quad_array":
                    return;
                case "texture_pack":
                case "texture_processor":
                    return;
                case "named_tileset_pack":
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["atlas"], mode);
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["def"], mode);
                    return;
                case "named_tileset":
                    return;
                case "deck_pack_raw":
                case "deck_pack":
                    SaveBatch(Path.Combine(folder, exportName), assetName, mode, i => i.EndsWith(".hmg"));
                    return;

                case "font_ttf":
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["font"], mode);
                    return;
                case "font_bmfont":
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["font"], mode);
                    return;
                case "shader_script":
                // Since we cannot compile/decompile shaders...
                case "glsl":
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["src"], mode);
                    return;
                case "texture":
                    if (objectFiles.Count == 0)
                    {
                        if (!_textureItems.TryGetValue(assetName, out var atlasPath))
                        {
                            return;
                        }
                        SaveBatch(Path.Combine(folder, exportName), atlasPath, mode, i => !i.EndsWith(".json"));
                        return;
                    }
                    if (fileType == "v" && assetName.EndsWith("_texture"))
                    {
                        var parentName = Path.GetDirectoryName(assetName)!.Replace('\\', '/');
                        if (_items.TryGetValue(parentName, out var extra))
                        {
                            ExtractTo(folder, mode, extra, exportName + ".json");
                        }

                        SaveTexture(Path.Combine(folder, exportName), objectFiles["pixmap"], mode);
                        return;
                    }
                    SaveTexture(Path.Combine(folder, exportName), objectFiles["pixmap"], mode);
                    return;
                case "fs_project":
                    return;
                case "fs_event":
                    return;
                case "fs_folder":
                    return;
                case "lua":
                    SaveLua(Path.Combine(folder, exportName), _scriptItems[assetName], mode);
                    return;
                case "data_json":
                case "data_csv":
                case "data_xls":
                case "animator_data":
                case "text":
                case "code_tileset":
                case "asset_map":
                case "multi_texture":
                case "tb_scheme":
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["data"], mode);
                    return;
                case "locale_pack":
                    SaveBatch(Path.Combine(folder, exportName), assetName, mode);
                    return;
                case "raw":
                case "movie":
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["data"], mode);
                    return;
                case "sq_script":
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["data"], mode);
                    return;
                case "msprite":
                    SaveAtlas(Path.Combine(folder, exportName), objectFiles["def"], mode);
                    return;
                case "proto":
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["def"], mode);
                    return;

                case "effect":
                case "fsm_scheme":
                case "bt_script":
                case "material":
                case "physics_body_def":
                case "physics_material":
                case "scene_portal_graph":
                case "quest_scheme":
                case "stylesheet":
                case "prefab":
                case "story_graph":
                case "render_target":
                case "ui_style":
                case "deck2d":
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["def"], mode);
                    return;

                case "scene":
                    if (fileType == "d")
                    {
                        SaveBatch(Path.Combine(folder, exportName), assetName, mode);
                        return;
                    }
                    SaveRaw(Path.Combine(folder, exportName, "default.scene_group"), objectFiles["def"], mode);
                    return;

                case "mesh":
                    SaveBatch(Path.Combine(folder, exportName), assetName, mode);
                    return;
                case "com_script":
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["script"], mode);
                    return;
                case "lut_texture":
                    SaveRaw(Path.Combine(folder, exportName), objectFiles["texture"], mode);
                    return;
            }
        }

        private void SaveRaw(string outputPath, string sourcePath, ArchiveExtractMode mode) 
        {
            ReadAs(sourcePath, fs => 
            {
                if (LocationStorage.TryCreate(outputPath, mode, out outputPath))
                {
                    fs.SaveAs(outputPath);
                }
            });
        }

        private void SaveAtlas(string outputPath, string sourcePath, ArchiveExtractMode mode)
        {
            if (options is IBundleExtractOptions o && !o.EnabledSpine)
            {
                return;
            }
            ReadAs(sourcePath, fs => {
                if (!LocationStorage.TryCreate(outputPath, ".json", mode, out outputPath))
                {
                    return;
                }
                fs.SaveAs(outputPath);
                fs.Seek(0, SeekOrigin.Begin);
                using var doc = JsonDocument.Parse(fs);
                if (doc.RootElement.TryGetProperty("modules", out var ele))
                {
                    using var writer = new CodeWriter(File.Create(outputPath[..^5] + ".atlas"));
                    writer.WriteLine(Path.GetFileNameWithoutExtension(outputPath) + ".png")
                        // .WriteLine($"size: 0,0")
                        .WriteLine("format: RGBA8888")
                        .WriteLine("filter: Linear,Linear")
                        .WriteLine("repeat: none");
                    foreach (var item in ele.EnumerateObject())
                    {
                        var rect = item.Value.GetProperty("rect").EnumerateArray().Select(i => i.TryGetInt32(out var res) ? res : (int)i.GetDouble()).ToArray();
                        writer.Write(item.Name)
                            .WriteIndentLine()
                            .WriteLine("rotate: false", true)
                            .WriteFormat("xy: {0}, {1}", rect[0], rect[1]).WriteLine(true)
                            .WriteFormat("size: {0}, {1}", rect[2], rect[3]).WriteLine(true)
                            .WriteFormat("orig: {0}, {1}", rect[2], rect[3]).WriteLine(true)
                            .WriteLine("offset: 0, 0", true)
                            .Write("index: -1")
                            .WriteOutdentLine();
                    }
                }
            });
        }

        private void SaveLua(string outputPath, string sourcePath, ArchiveExtractMode mode)
        {
            if (options is IBundleExtractOptions o && !o.EnabledLua)
            {
                return;
            }
            ReadAs(sourcePath, fs => {
                var decompressor = new LuaScheme();
                var res = decompressor.Open(fs);
                if (res is null)
                {
                    if (LocationStorage.TryCreate(outputPath, mode, out outputPath))
                    {
                        fs.SaveAs(outputPath);
                    }
                    return;
                }
                if (!LocationStorage.TryCreate(outputPath, ".lua", mode, out var fileName))
                {
                    return;
                }
                using var os = File.Create(fileName);
                decompressor.Create(os, res);
            });
        }

        private void SaveTexture(string outputPath, string sourcePath, ArchiveExtractMode mode)
        {
            if (options is IBundleExtractOptions o && !o.EnabledImage)
            {
                return;
            }
            ReadAs(sourcePath, fs => 
            {
                if (LocationStorage.TryCreate(outputPath, ".png", mode, out var fileName))
                {
                    using var output = File.Create(fileName);
                    new HmgReader(new BinaryReader(fs), sourcePath).ExtractTo(output);
                }
            });
        }

        private void SaveBatch(string folder, string sourcePath, ArchiveExtractMode mode, Func<string, bool>? hmgFn = null)
        {
            if (!TryGetArchive(sourcePath, out var entryPath, out var archive))
            {
                return;
            }
            foreach (var item in archive.Keys)
            {
                if (item.StartsWith(entryPath))
                {
                    archive.ExtractTo(entryPath, fs => {
                        var outputPath = Path.Combine(folder, entryPath.Split('/', 2).Last());
                        if (hmgFn?.Invoke(entryPath) != true)
                        {
                            if (LocationStorage.TryCreate(outputPath, mode, out outputPath))
                            {
                                fs.SaveAs(outputPath);
                            }
                            return;
                        }
                        if (LocationStorage.TryCreate(outputPath, ".png", mode, out var fileName))
                        {
                            using var output = File.Create(fileName);
                            new HmgReader(new BinaryReader(fs), item).ExtractTo(output);
                        }
                    });
                }
            }
        }

        private void LoadAssetLibrary(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }
            ReadDocument(fileName, doc => {
                var root = doc.RootElement;
                foreach (var item in root.EnumerateObject())
                {
                    var filePath = string.Empty;
                    if (item.Value.TryGetProperty("filePath", out var ele) && ele.ValueKind == JsonValueKind.String)
                    {
                        filePath = ele.GetString();
                    }
                    var fileType = string.Empty;
                    if (item.Value.TryGetProperty("fileType", out ele))
                    {
                        fileType = ele.GetString();
                    }
                    var groupType = string.Empty;
                    if (item.Value.TryGetProperty("groupType", out ele) && ele.ValueKind == JsonValueKind.String)
                    {
                        groupType = ele.GetString();
                    }
                    var objectFiles = new Dictionary<string, string>();
                    if (item.Value.TryGetProperty("objectFiles", out ele))
                    {
                        objectFiles = ele.EnumerateObject().Select(i => new KeyValuePair<string, string>(i.Name, i.Value.GetString())).ToDictionary();
                    }

                    var type = string.Empty;
                    if (item.Value.TryGetProperty("type", out ele))
                    {
                        type = ele.GetString();
                    }
                    _items.Add(item.Name, new EastwardAssetInfo(item.Name, filePath, groupType, fileType, type, objectFiles));
                }
            });
        }

        private void LoadScriptLibrary(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }
            ReadDocument(fileName, doc => {
                var root = doc.RootElement;
                if (root.TryGetProperty("export", out var ele) && root.TryGetProperty("source", out var src))
                {
                    var temp = src.EnumerateObject().Select(item => new KeyValuePair<string, string>(item.Name, item.Value.GetString())).ToDictionary();
                    foreach (var item in ele.EnumerateObject())
                    {
                        _scriptItems.Add(temp[item.Name], item.Value.GetString());
                    }
                }
            });
        }

        private void LoadTextureIndex(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }
            ReadDocument(fileName, doc => {
                var root = doc.RootElement;
                if (root.TryGetProperty("root", out var rootKey) && root.TryGetProperty("map", out var map) 
                    && map.TryGetProperty(rootKey.GetString(), out var ele)
                    && ele.TryGetProperty("body", out ele)
                    && ele.TryGetProperty("groups", out ele))
                {
                    foreach (var item in ele.EnumerateArray())
                    {
                        var childNode = map.GetProperty(item.GetString()).GetProperty("body");
                        if (childNode.TryGetProperty("atlasCachePath", out var next) && next.ValueKind == JsonValueKind.String)
                        {
                            var atlasCachePath = next.GetString();
                            if (childNode.TryGetProperty("textures", out var tEle) && tEle.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var textureNode in tEle.EnumerateArray())
                                {
                                    var childTextureNode = map.GetProperty(textureNode.GetString()).GetProperty("body");
                                    _textureItems.Add(childTextureNode.GetProperty("path").GetString(), atlasCachePath);
                                }
                            }
                        }
                    }
                }
            });
        }

        public void Dispose()
        {
            foreach (var item in _archiveItems)
            {
                item.Value.Dispose();
            }
            _items.Clear();
            _archiveItems.Clear();
            _scriptItems.Clear();
            _textureItems.Clear();
        }
    }
}
