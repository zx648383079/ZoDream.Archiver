using System.CommandLine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.Archiver
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("资源提取");
            var fileArg = new Argument<DirectoryInfo>("file")
            {
                Description = "目标文件夹"
            };
            var skipArg = new Option<int>("skip", "-s")
            {
                Description = "跳过文件数量"
            };
            var outputArg = new Option<DirectoryInfo>("output", "-o")
            {
                Description = "输出到文件夹",
                DefaultValueFactory = argv => argv.GetRequiredValue(fileArg).CreateSubdirectory("output")
            };
            var batchArg = new Option<int>("batch", "-b")
            {
                Description = "最大批处理数量",
                DefaultValueFactory = _ => 100
            };
            var packageArg = new Option<string>("package", "-p")
            {
                Description = "包名"
            };
            var keyArg = new Option<string>("key", "-k")
            {
                Description = "密钥Key"
            };
            var versionArg = new Option<string>("version", "-v")
            {
                Description = "自定义版本号"
            };
            var dependencyArg = new Option<bool>("dependency", "-d")
            {
                Description = "使用依赖文件"
            };
            var platformArg = new Option<string>("platform", "-a")
            {
                Description = "平台",
            };
            var producerArg = new Option<string>("producer", "-c")
            {
                Description = "创作者",
            };
            var engineArg = new Option<string>("engine", "-e")
            {
                Description = "游戏引擎",
                DefaultValueFactory = _ => "Unity"
            };
            var typeTreeArg = new Option<FileInfo>("type", "-t")
            {
                Description = "TypeNodeTree文件"
            };
            var modeArg = new Option<ArchiveExtractMode>("mode", "-m")
            {
                Description = "重复文件处理方式",
                DefaultValueFactory = _ => ArchiveExtractMode.Overwrite
            };
            rootCommand.Add(fileArg);
            rootCommand.Add(skipArg);
            rootCommand.Add(outputArg);
            rootCommand.Add(batchArg);
            rootCommand.Add(packageArg);
            rootCommand.Add(keyArg);
            rootCommand.Add(versionArg);
            rootCommand.Add(dependencyArg);
            rootCommand.Add(platformArg);
            rootCommand.Add(producerArg);
            rootCommand.Add(engineArg);
            rootCommand.Add(typeTreeArg);
            rootCommand.Add(modeArg);
            rootCommand.SetAction((argv, token) => {
                var rootFolder = argv.GetRequiredValue(fileArg).FullName;
                var resFolder = Path.Combine(rootFolder, "resources");
                var options = new BundleOptions()
                {
                    Password = argv.GetValue(keyArg),
                    Platform = argv.GetValue(platformArg),
                    Engine = argv.GetValue(engineArg),
                    Package = argv.GetValue(packageArg),
                    Producer = argv.GetValue(producerArg),
                    Version = argv.GetValue(versionArg),
                    FileMode = argv.GetValue(modeArg),
                    OutputFolder = argv.GetValue(outputArg)!.FullName,
                    Entrance = Directory.Exists(resFolder) ? resFolder : rootFolder,
                    ModelFormat = "gltf",
                    MaxBatchCount = argv.GetValue(batchArg),
                    TypeTree = argv.GetValue(typeTreeArg)?.FullName ?? string.Empty,
                };
                if (argv.GetValue(dependencyArg))
                {
                    options.DependencySource = Path.Combine(rootFolder == options.Entrance ? 
                        Path.GetDirectoryName(rootFolder) : rootFolder, "dependencies.bin");
                    options.OnlyDependencyTask = !File.Exists(options.DependencySource);
                }
                var runtime = new BundleRuntime(rootFolder, options, argv.GetValue(skipArg));
                return runtime.RunAsync(token);
            });
            
            // runtime = new TransformRuntime(Path.Combine(rootFolder, "output"));
            return await rootCommand.Parse(args).InvokeAsync();
        }

 
    }
}
