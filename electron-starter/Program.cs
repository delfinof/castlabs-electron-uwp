using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace electron_starter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Process myProcess = new Process();
            try
            {
                myProcess.StartInfo.UseShellExecute = false;
                // You can start any process, HelloWorld is a do-nothing example.
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));

      
                // You can start any process, HelloWorld is a do-nothing example.
                myProcess.StartInfo.FileName = "..\\electron\\electron.exe";

                var filename = Environment.CommandLine; 
                if (filename.StartsWith("\""))
                {
                    var quote_position = filename.Substring(1).IndexOf('\"');
                    filename = filename.Substring(quote_position + 3);
                }
                else
                    filename = filename.Replace(Process.GetCurrentProcess().MainModule.FileVersionInfo.FileName, "");

                myProcess.StartInfo.Arguments = filename;
                myProcess.StartInfo.CreateNoWindow = false;
                myProcess.Start();

                myProcess.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
