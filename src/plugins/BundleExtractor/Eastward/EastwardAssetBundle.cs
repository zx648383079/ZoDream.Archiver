using FMOD;
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
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Eastward
{
    public class EastwardAssetBundle(IBundleSource source, string configPath, IArchiveScheme scheme) : IBundleSource, IBundleHandler
    {
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
            var assetLibrary = "config/asset_index";
            var scriptLibrary = "config/script_library";
            var textureLibrary = "config/texture_index";
            ReadDocument("config/game_config", doc => {
                var root = doc.RootElement;
                if (root.TryGetProperty("asset_library", out var ele))
                {
                    assetLibrary = ele.GetString() ?? assetLibrary;
                }
                if (root.TryGetProperty("script_library", out ele))
                {
                    scriptLibrary = ele.GetString() ?? scriptLibrary;
                }
                if (root.TryGetProperty("texture_library", out ele))
                {
                    textureLibrary = ele.GetString() ?? textureLibrary;
                }
            });
            LoadAssetLibrary(assetLibrary);
            LoadScriptLibrary(scriptLibrary);
            LoadTextureIndex(textureLibrary);
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
            foreach (var item in _items)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                ExtractTo(folder, mode, item.Value);
            }
        }

        private void ExtractTo(string folder, ArchiveExtractMode mode, EastwardAssetInfo assetInfo)
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
                    SaveRaw(Path.Combine(folder, objectFiles["atlas"]), objectFiles["atlas"], mode);
                    SaveRaw(Path.Combine(folder, objectFiles["def"]), objectFiles["def"], mode);
                    return;
                case "named_tileset":
                    return;
                case "deck_pack_raw":
                case "deck_pack":
                    SaveBatch(folder, objectFiles["export"], mode, i => i.EndsWith(".hmg"));
                    return;

                case "font_ttf":
                    SaveRaw(Path.Combine(folder, objectFiles["font"]), objectFiles["font"], mode);
                    return;
                case "font_bmfont":
                    SaveRaw(Path.Combine(folder, objectFiles["font"]), objectFiles["font"], mode);
                    return;
                case "shader_script":
                // Since we cannot compile/decompile shaders...
                case "glsl":
                    SaveRaw(Path.Combine(folder, objectFiles["src"]), objectFiles["src"], mode);
                    return;
                case "texture":
                    if (objectFiles.Count == 0)
                    {
                        if (!_textureItems.TryGetValue(assetName, out var atlasPath))
                        {
                            return;
                        }
                        SaveBatch(folder, atlasPath, mode, i => !i.EndsWith(".json"));
                        return;
                    }

                    SaveTexture(Path.Combine(folder, objectFiles["pixmap"]), objectFiles["pixmap"], mode);
                    return;
                case "fs_project":
                    return;
                case "fs_event":
                    return;
                case "fs_folder":
                    return;
                case "lua":
                    SaveLua(Path.Combine(folder, assetName), _scriptItems[assetName], mode);
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
                    SaveRaw(Path.Combine(folder, objectFiles["data"]), objectFiles["data"], mode);
                    return;
                case "locale_pack":
                    SaveBatch(folder, objectFiles["data"], mode);
                    return;
                case "raw":
                case "movie":
                    SaveRaw(Path.Combine(folder, objectFiles["data"]), objectFiles["data"], mode);
                    return;
                case "sq_script":
                    SaveRaw(Path.Combine(folder, objectFiles["data"]), objectFiles["data"], mode);
                    return;
                case "msprite":
                    SaveRaw(Path.Combine(folder, objectFiles["def"]), objectFiles["def"], mode);
                    return;
                case "proto":
                    SaveRaw(Path.Combine(folder, objectFiles["def"]), objectFiles["def"], mode);
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
                    SaveRaw(Path.Combine(folder, objectFiles["def"]), objectFiles["def"], mode);
                    return;

                case "scene":
                    if (fileType == "d")
                    {
                        SaveBatch(folder, objectFiles["def"], mode);
                        return;
                    }
                    SaveRaw(Path.Combine(folder, objectFiles["def"]), objectFiles["def"], mode);
                    return;

                case "mesh":
                    SaveBatch(folder, objectFiles["mesh"], mode);
                    return;
                case "com_script":
                    SaveRaw(Path.Combine(folder, objectFiles["script"]), objectFiles["script"], mode);
                    return;
                case "lut_texture":
                    SaveRaw(Path.Combine(folder, objectFiles["texture"]), objectFiles["texture"], mode);
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

        private void SaveLua(string outputPath, string sourcePath, ArchiveExtractMode mode)
        {
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
