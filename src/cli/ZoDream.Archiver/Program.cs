using System.CommandLine;

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
            var skipArg = new Option<int>("skip", "-s", "--skip")
            {
                Description = "跳过文件数量"
            };
            var outputArg = new Option<DirectoryInfo>("output", "-o", "--output")
            {
                Description = "输出到文件夹"
            };
            rootCommand.Add(fileArg);
            rootCommand.Add(skipArg);
            rootCommand.Add(outputArg);
            rootCommand.SetAction((argv, token) => {
                IConsoleRuntime runtime;
                runtime = new BundleRuntime(argv.GetRequiredValue(fileArg).FullName, argv.GetValue(skipArg));
                return runtime.RunAsync(token);
            });
            
            // runtime = new TransformRuntime(Path.Combine(rootFolder, "output"));
            return await rootCommand.Parse(args).InvokeAsync();
        }

 
    }
}
