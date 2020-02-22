using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ProgramManager
{
    public class LocalProcess
    {
        public LocalProcess(string path)
        {
            stdout = "";
            process = new Process();
            process.OutputDataReceived += (sender, e) => {
                System.Console.WriteLine(e.Data);
                stdout += e.Data + "\n";
            };
            process.ErrorDataReceived += (sender, e) => {
                System.Console.WriteLine(e.Data);
                stdout += e.Data + "\n";
            };
            thread = new Thread(new ThreadStart(() => { KeepAlive(path); }));
            thread.Start();
        }

        public void KeepAlive(string path)
        {
            while (Thread.CurrentThread.IsAlive)
            {
                FileInfo fi = new FileInfo(path);
                System.Console.WriteLine(path);
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = path,
                    WorkingDirectory = fi.Directory.ToString(),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true
                };
                stdout = "";
                process = Process.Start(startInfo);
                process.WaitForExit();
            }
        }

        public string GetStdout()
            => stdout;

        private Process process;
        private readonly Thread thread;
        private string stdout;
    }
}
