using System;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity
{
    public static class FileNameHelper
    {

        public static bool IsMatch(IFilePath filePath, IFileName name, StringComparison comparison)
        {
            if (name is FilePathName)
            {
                return filePath is not IEntryPath && filePath.Name.Equals(name.Name, comparison);
            }
            if (name is EntryName)
            {
                return filePath is IEntryPath && filePath.Name.Equals(name.Name, comparison);
            }
            return filePath.Name.Equals(name.Name, comparison);
        }

        public static IFileName ParseIdentifier(string filePath)
        {
            if (filePath.StartsWith(LibraryFolder, StringComparison.OrdinalIgnoreCase))
            {
                return new FilePathName(filePath[LibraryFolder.Length..]);
            }
            if (filePath.StartsWith(ResourcesFolder, StringComparison.OrdinalIgnoreCase))
            {
                return new FilePathName(filePath[ResourcesFolder.Length..]);
            }
            if (filePath.StartsWith(ArchivePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return new EntryName(Path.GetFileName(filePath));
            }
            return new UnknownName(filePath);
        }

        /// <summary>
        /// 创建新的路径
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Create(IFilePath source, string name)
        {
            if (string.IsNullOrWhiteSpace(name) && source is IEntryPath e)
            {
                name = e.Name;
            }
            var filePath = FilePath.GetFilePath(source);
            if (string.IsNullOrWhiteSpace(name))
            {
                return filePath;
            }
            return CombineIf(Path.GetDirectoryName(filePath), name);
        }

        public static string CombineIf(string? folder, string name)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                return name;
            }
            return Path.Combine(folder, name);
        }


        public static string GetExtension(string fileName)
        {
            var j = fileName.LastIndexOf('#');
            var i = fileName.LastIndexOf('.');
            if (i < 0 || i <= j)
            {
                return string.Empty;
            }
            return fileName[(i + 1)..].ToLower();
        }
        /// <summary>
        /// 判断是否是普通文件类型
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool IsCommonFile(string filename)
        {
            return GetExtension(filename) switch
            {
                "dex" or "json" or "txt" or "so" or "dll" or "properties" or "xml" or "html" or "htm" => true,
                _ => false,
            };
        }

        public static bool IsEngineResource(string? fileName)
        {
            return IsDefaultResource(fileName) || IsEditorResource(fileName);
        }

        public static bool IsDefaultResource(string? fileName)
        {
            return fileName is DefaultResourceName1 or DefaultResourceName2;
        }

        public static bool IsEditorResource(string? fileName)
        {
            return fileName is EditorResourceName;
        }

        public static bool IsBuiltinExtra(string? fileName)
        {
            return fileName is BuiltinExtraName1 or BuiltinExtraName2;
        }

        public static bool IsDefaultResourceOrBuiltinExtra(string? fileName)
        {
            return IsDefaultResource(fileName) || IsBuiltinExtra(fileName);
        }

        public static bool IsEngineGeneratedF(string? fileName)
        {
            return fileName is EngineGeneratedF;
        }


        /// <summary>
        /// Remove .dll extension from the assembly name and add the "Assembly - " prefix if appropriate.
        /// </summary>
        /// <param name="assembly">An assembly name.</param>
        /// <returns>A new string if changed, otherwise the original string.</returns>
        public static string FixAssemblyName(string assembly)
        {
            return RemoveAssemblyFileExtension(IsAssemblyIdentifier(assembly) ? $"Assembly - {assembly}" : assembly);
        }

        /// <summary>
        /// Remove .dll extension from the assembly name.
        /// </summary>
        /// <param name="assembly">An assembly name.</param>
        /// <returns>A new string if changed, otherwise the original string.</returns>
        public static string RemoveAssemblyFileExtension(string assembly)
        {
            return assembly.EndsWith(AssemblyExtension, StringComparison.Ordinal) ? assembly[..^AssemblyExtension.Length] : assembly;
        }

        public static string AddAssemblyFileExtension(string assembly)
        {
            return assembly.EndsWith(AssemblyExtension, StringComparison.Ordinal) ? assembly : assembly + AssemblyExtension;
        }

        public static bool IsProjectAssembly(string assembly)
        {
            const string PrefixName = "Assembly";
            return assembly.StartsWith($"{PrefixName} - ", StringComparison.Ordinal) || assembly.StartsWith($"{PrefixName}-", StringComparison.Ordinal);
        }

        private static bool IsAssemblyIdentifier(string assembly)
        {
            return assembly
                is "Boo"
                or "Boo - first pass"
                or "CSharp"
                or "CSharp - first pass"
                or "UnityScript"
                or "UnityScript - first pass";
        }

       

        public const string LibraryFolder = "library/";
        public const string ResourcesFolder = "resources/";
        public const string ArchivePrefix = "archive:/";
        public const string DefaultResourceName1 = "unity default resources";
        public const string DefaultResourceName2 = "unity_default_resources";
        public const string EditorResourceName = "unity editor resources";
        public const string BuiltinExtraName1 = "unity builtin extra";
        public const string BuiltinExtraName2 = "unity_builtin_extra";
        public const string EngineGeneratedF = "0000000000000000f000000000000000";
        public const string AssemblyExtension = ".dll";
    }
}
