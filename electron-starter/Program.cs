using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using Windows.Storage;


namespace electron_starter
{
    static class Program
    {

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(UInt32 dwProcessId);
        [DllImport("kernel32.dll")]
        private static extern bool GetFileInformationByHandle(SafeFileHandle hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);
        [DllImport("kernel32.dll")]
        private static extern SafeFileHandle GetStdHandle(UInt32 nStdHandle);
        [DllImport("kernel32.dll")]
        private static extern bool SetStdHandle(UInt32 nStdHandle, SafeFileHandle hHandle);
        [DllImport("kernel32.dll")]
        private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, SafeFileHandle hSourceHandle, IntPtr hTargetProcessHandle,
        out SafeFileHandle lpTargetHandle, UInt32 dwDesiredAccess, Boolean bInheritHandle, UInt32 dwOptions);

        private const UInt32 STD_OUTPUT_HANDLE = 0xFFFFFFF5;
        private const UInt32 STD_ERROR_HANDLE = 0xFFFFFFF4;
        private const UInt32 DUPLICATE_SAME_ACCESS = 2;

        struct BY_HANDLE_FILE_INFORMATION
        {
            public UInt32 FileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            public UInt32 VolumeSerialNumber;
            public UInt32 FileSizeHigh;
            public UInt32 FileSizeLow;
            public UInt32 NumberOfLinks;
            public UInt32 FileIndexHigh;
            public UInt32 FileIndexLow;
        }

        // redirect electron output to console or file. not works perfectly for starter process output
        // https://stackoverflow.com/questions/1786840/c-is-it-possible-to-have-a-single-application-behave-as-console-or-windows-app
        static void InitConsoleHandles()
        {
            SafeFileHandle hStdOut, hStdErr, hStdOutDup, hStdErrDup;
            BY_HANDLE_FILE_INFORMATION bhfi;

            hStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            hStdErr = GetStdHandle(STD_ERROR_HANDLE);

            // Get current process handle
            IntPtr hProcess = Process.GetCurrentProcess().Handle;

            // Duplicate Stdout handle to save initial value
            DuplicateHandle(hProcess, hStdOut, hProcess, out hStdOutDup,
                0, true, DUPLICATE_SAME_ACCESS);

            // Duplicate Stderr handle to save initial value
            DuplicateHandle(hProcess, hStdErr, hProcess, out hStdErrDup,
                0, true, DUPLICATE_SAME_ACCESS);

            // Attach to console window – this may modify the standard handles
            AttachConsole(ATTACH_PARENT_PROCESS);

            
            // Adjust the standard handles
            if (GetFileInformationByHandle(GetStdHandle(STD_OUTPUT_HANDLE), out bhfi))
            {
                SetStdHandle(STD_OUTPUT_HANDLE, hStdOutDup);
            }
            else
            {
                SetStdHandle(STD_OUTPUT_HANDLE, hStdOut);
            }

            if (GetFileInformationByHandle(GetStdHandle(STD_ERROR_HANDLE), out bhfi))
            {
                SetStdHandle(STD_ERROR_HANDLE, hStdErrDup);
            }
            else
            {
                SetStdHandle(STD_ERROR_HANDLE, hStdErr);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitConsoleHandles();
    //        AttachConsole(ATTACH_PARENT_PROCESS);
            Process myProcess = new Process();
            try
            {
                myProcess.StartInfo.UseShellExecute = false;
                // You can start any process, HelloWorld is a do-nothing example.
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));

                Console.WriteLine("LocalFolder: " + ApplicationData.Current.LocalFolder.Path);
                Console.WriteLine("CommandLine: " + Environment.CommandLine );
      
                // You can start any process, HelloWorld is a do-nothing example.
                myProcess.StartInfo.FileName = "..\\electron\\electron.exe";
                myProcess.StartInfo.RedirectStandardError = false;
                myProcess.StartInfo.RedirectStandardInput = false;

                var filename = Environment.CommandLine;
                if (filename.StartsWith("\""))
                {
                    Console.WriteLine("Quoted program name");
                    var quote_position = filename.Substring(1).IndexOf('\"');
                    if (quote_position + 3 < filename.Length)
                        filename = filename.Substring(quote_position + 3);
                    else
                        filename = "";
                }
                else
                {
                    Console.WriteLine("Not quoted program name");
                    var space_position = filename.IndexOf(' ');
                    if (space_position >= 0 && space_position < filename.Length)
                        filename = filename.Substring(space_position + 1);
                    else
                        filename = "";
                }
                Console.WriteLine("Command line cleaned of filename: " + filename);

                myProcess.StartInfo.Arguments = "--widevine-base-dir=" + ApplicationData.Current.LocalFolder.Path + " " + filename;
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
