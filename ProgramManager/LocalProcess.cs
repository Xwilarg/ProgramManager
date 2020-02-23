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
            FileInfo fi = new FileInfo(path);
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = path,
                WorkingDirectory = fi.DirectoryName,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            process.OutputDataReceived += (sender, e) => {
                System.Console.WriteLine(e.Data);
                stdout += e.Data + "\n";
            };
            process.ErrorDataReceived += (sender, e) => {
                System.Console.WriteLine(e.Data);
                stdout += e.Data + "\n";
            };
            thread = new Thread(new ThreadStart(KeepAlive));
            thread.Start();
        }

        public void KeepAlive()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                stdout = "";
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
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
