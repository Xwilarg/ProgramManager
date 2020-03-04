using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
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
                stdout += CleanProcessOutput(e.Data + "\n");
            };
            process.ErrorDataReceived += (sender, e) => {
                stdout += CleanProcessOutput(e.Data + "\n");
            };
            thread = new Thread(new ThreadStart(KeepAlive));
            thread.Start();
        }

        private string CleanProcessOutput(string str)
        {
            str = CleanFirstMatch(str, "Connected to ([^\n]+)");
            str = CleanFirstMatch(str, "POST channels\\/([0-9]+)\\/messages");
            str = CleanFirstMatch(str, "Error occurred executing \"[^\"]+\" for ([^#]+#[0-9]+)");
            str = CleanFirstMatch(str, "for XXXXXXXXXX in (.+) ---> .+Exception:");
            return str;
        }

        private string CleanFirstMatch(string str, string match)
        {
            string match1 = Regex.Match(str, match).Groups[1].Value;
            return str.Replace(match1, "XXXXXXXXXX");
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
