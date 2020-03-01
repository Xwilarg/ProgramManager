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
            autoRestart = true;
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
                stdout += e.Data + "\n";
            };
            process.ErrorDataReceived += (sender, e) => {
                stdout += e.Data + "\n";
            };
            thread = new Thread(new ThreadStart(KeepAlive));
            thread.Start();
        }

        private void KeepAlive()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                if (autoRestart)
                {
                    stdout = "";
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
            }
        }

        public void Stop()
        {
            autoRestart = false;
            if (!process.HasExited)
            {
                process.Kill();
            }
        }

        public void Start()
        {
            autoRestart = true;
        }

        public void Restart()
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }

        public string GetStdout()
            => stdout;

        public bool IsStopped()
            => process.HasExited;

        private Process process;
        private readonly Thread thread;
        private string stdout;
        private bool autoRestart;
    }
}
