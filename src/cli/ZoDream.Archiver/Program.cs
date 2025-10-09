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
                DefaultValueFactory = _ => "Android"
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
            rootCommand.Add(fileArg);
            rootCommand.Add(skipArg);
            rootCommand.Add(outputArg);
            rootCommand.SetAction((argv, token) => {
                var rootFolder = argv.GetRequiredValue(fileArg).FullName;
                var options = new BundleOptions()
                {
                    Platform = argv.GetValue(platformArg),
                    Engine = argv.GetValue(engineArg),
                    Package = argv.GetValue(packageArg),
                    Producer = argv.GetValue(producerArg),
                    Version = argv.GetValue(versionArg),
                    FileMode = ArchiveExtractMode.Overwrite,
                    OutputFolder = argv.GetValue(outputArg)!.FullName,
                    Entrance = Path.Combine(rootFolder, "resources"),
                    ModelFormat = "gltf",
                    MaxBatchCount = argv.GetValue(batchArg),
                };
                if (argv.GetValue(dependencyArg))
                {
                    options.DependencySource = Path.Combine(rootFolder, "dependencies.bin");
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
