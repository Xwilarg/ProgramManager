using System;
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
            this.path = path;
            stdout = "";
            autoRestart = true;
            thread = new Thread(new ThreadStart(KeepAlive));
            thread.Start();
        }

        private string CleanProcessOutput(string str)
        {
            string oldStr = str;
            str = "";
            foreach (char c in oldStr)
                if (c >= 32 && c < 127)
                    str += c;
            str = CleanFirstMatch(str, "Connected to ([\\s\\S]+)");
            str = CleanFirstMatch(str, "Disconnected to ([\\s\\S]+)");
            str = CleanFirstMatch(str, "POST channels\\/([0-9]+)\\/messages");
            str = CleanFirstMatch(str, "Error occurred executing \"[^\"]+\" for ([^#]+#[0-9]+)");
            str = CleanFirstMatch(str, "for XXXXXXXXXX in (.+) ---> .+Exception:");
            return str;
        }

        private string CleanFirstMatch(string str, string match)
        {
            if (str == null)
                return "";
            Match m = Regex.Match(str, match);
            if (!m.Success)
                return str;
            string match1 = m.Groups[1].Value;
            string xxx = "";
            for (int i = 0; i < match1.Length; i++)
                xxx += "X";
            return str.Replace(match1, xxx);
        }

        private void KeepAlive()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                if (autoRestart)
                {
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
                        try
                        {
                            stdout += CleanProcessOutput(e.Data) + "\n";
                        }
                        catch (Exception) { }
                    };
                    process.ErrorDataReceived += (sender, e) => {
                        try
                        {
                            stdout += CleanProcessOutput(e.Data) + "\n";
                        }
                        catch (Exception) { }
                    };
                    Console.WriteLine(DateTime.Now.ToString() + ": Starting " + path);
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
        private string path;
    }
}
