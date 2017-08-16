using System.Linq;
using static System.Console;

namespace GitSharpApi.Cmd
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string workingDir = "C:\\Users\\U403598\\Desktop\\temp\\tempgit\\inner";
            var gitApi = new GitApi(new GitProcess(workingDir));

            var data = gitApi.GetCommits().Result;
            //WriteLine(string.Join("\r\n", data));
            //foreach (var varx in data)
            //{
            //    WriteLine(varx);
            //    var data2 = gitApi.CatFileAsync(varx).Result;
            //    WriteLine(data2);
            //}

            WriteLine("Press any key to exit...");
            ReadKey();
        }

        //private static async Task ExecuteProcessAndPrint(StringResultProcess process, string args, int order)
        //{
        //    //for (var i = 0; i < 200; i++)
        //    //{
        //    //    WriteLine($"Started process {i + 1}: ");
        //    //    var _ = ExecuteProcessAndPrint(process, argsx, i + 1);
        //    //}
        //    var data = await process.ExecuteCommandAsync(args);
        //    WriteLine($"{order}: {data.ExitCode}");
        //}
    }
}