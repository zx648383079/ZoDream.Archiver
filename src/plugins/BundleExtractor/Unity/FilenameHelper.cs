using System;
using System.IO;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity
{
    public static class FileNameHelper
    {
        /// <summary>
        /// 创建新的路径
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Create(string fullPath, string name)
        {
            fullPath = BundleStorage.Separate(fullPath, out var entryName);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = entryName;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                return fullPath;
            }
            var folder = Path.GetDirectoryName(fullPath);
            return Path.Combine(folder!, name);
        }

        /// <summary>
        /// 根据
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="brotherName"></param>
        /// <returns></returns>
        public static string CombineBrother(string fullPath, string brotherName)
        {
            fullPath = BundleStorage.Separate(fullPath, out var entryName);
            string? folder;
            if (!string.IsNullOrEmpty(entryName))
            {
                folder = Path.GetDirectoryName(entryName);
            } else
            {
                folder = Path.GetDirectoryName(fullPath);
            }
            if (string.IsNullOrWhiteSpace(folder))
            {
                return brotherName;
            }
            return Path.Combine(folder, brotherName);
        }

        public static string GetFileName(string fullPath)
        {
            fullPath = BundleStorage.Separate(fullPath, out var entryName);
            if (string.IsNullOrEmpty(entryName))
            {
                return Path.GetFileName(fullPath);
            }
            return Path.GetFileName(entryName);
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

        public static string FixFileIdentifier(string name)
        {
            name = name.ToLowerInvariant();
            name = FixDependencyName(name);
            name = FixResourcePath(name);
            return name;
        }

        public static string FixDependencyName(string dependency)
        {
            if (dependency.StartsWith(LibraryFolder, StringComparison.Ordinal))
            {
                return dependency[LibraryFolder.Length..];
            }
            else if (dependency.StartsWith(ResourcesFolder, StringComparison.Ordinal))
            {
                return dependency[ResourcesFolder.Length..];
            }
            return dependency;
        }

        public static string FixResourcePath(string resourcePath)
        {
            const string archivePrefix = "archive:/";
            if (resourcePath.StartsWith(archivePrefix, StringComparison.Ordinal))
            {
                return Path.GetFileName(resourcePath);
            }
            else
            {
                return resourcePath;
            }
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
        public const string DefaultResourceName1 = "unity default resources";
        public const string DefaultResourceName2 = "unity_default_resources";
        public const string EditorResourceName = "unity editor resources";
        public const string BuiltinExtraName1 = "unity builtin extra";
        public const string BuiltinExtraName2 = "unity_builtin_extra";
        public const string EngineGeneratedF = "0000000000000000f000000000000000";
        public const string AssemblyExtension = ".dll";
    }
}
