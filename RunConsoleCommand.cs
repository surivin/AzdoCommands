
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Text;

using System.Reflection;
using System.IO;

namespace AzdoCommands
{
    public class Response
    {
        public int code { get; set; }
        public string stdout { get; set; }
        public string stderr { get; set; }
    }

    public enum Output
    {
        Hidden,
        Internal,
        External
    }

    public static class Shell
    {
        private static string GetFileName()
        {
            return "cmd.exe";
        }


        private static string CommandConstructor(string cmd, Output? output = Output.Hidden, string dir = "")
        {
            if (!String.IsNullOrEmpty(dir))
            {
                dir = $" \"{dir}\"";
            }
            if (output == Output.External)
            {
                cmd = $"{Directory.GetCurrentDirectory()}/cmd.bat \"{cmd}\"{dir}";
            }
            cmd = $"/c \"{cmd}\"";

            return cmd;
        }

        public static Response Term(string cmd, Output? output = Output.Hidden, string dir = "")
        {
            var result = new Response();
            var stderr = new StringBuilder();
            var stdout = new StringBuilder();
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = GetFileName();
                startInfo.Arguments = CommandConstructor(cmd, output, dir);
                startInfo.RedirectStandardOutput = !(output == Output.External);
                startInfo.RedirectStandardError = !(output == Output.External);
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = !(output == Output.External);
                startInfo.RedirectStandardInput = true;

                if (!String.IsNullOrEmpty(dir) && output != Output.External)
                {
                    startInfo.WorkingDirectory = dir;
                }

                using (Process process = Process.Start(startInfo))
                {
                    switch (output)
                    {
                        case Output.Internal:
                            while (!process.StandardOutput.EndOfStream)
                            {
                                try
                                {
                                    foreach (ProcessThread thread in process.Threads)
                                        if (thread.ThreadState == System.Diagnostics.ThreadState.Wait
                                            && thread.WaitReason == ThreadWaitReason.UserRequest)
                                        {
                                            var streamWriter = process.StandardInput;
                                            streamWriter.WriteLine("I'm supplying input!");
                                        }
                                }
                                catch { }
                                string line = process.StandardOutput.ReadLine();
                                stdout.AppendLine(line);
                                Console.WriteLine(line);
                            }

                            while (!process.StandardError.EndOfStream)
                            {
                                string line = process.StandardError.ReadLine();
                                stderr.AppendLine(line);
                                Console.WriteLine(line);
                            }
                            break;
                        case Output.Hidden:
                            stdout.AppendLine(process.StandardOutput.ReadToEnd());
                            stderr.AppendLine(process.StandardError.ReadToEnd());
                            break;
                    }
                    process.WaitForExit();
                    result.stdout = stdout.ToString();
                    result.stderr = stderr.ToString();
                    result.code = process.ExitCode;
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
            return result;
        }
    }
}