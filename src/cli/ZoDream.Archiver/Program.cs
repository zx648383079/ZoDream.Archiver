namespace ZoDream.Archiver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rootFolder = "D:\\zodream";
            var cancelToken = new CancellationTokenSource();
            IConsoleRuntime runtime;
            runtime = new BundleRuntime(rootFolder);
            // runtime = new TransformRuntime(Path.Combine(rootFolder, "output"));
            var task = runtime.RunAsync(cancelToken.Token);
            task.Wait();
            cancelToken.Cancel();
            Console.WriteLine();
            Console.WriteLine("Finished!");
            Console.ReadKey();
        }

 
    }
}
