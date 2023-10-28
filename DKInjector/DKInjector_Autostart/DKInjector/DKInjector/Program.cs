using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Injector
{ // BY Diktor
    class Program
    {
        static async void ACmd(string line)
        {
            await Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {line}",
                    WindowStyle = ProcessWindowStyle.Hidden,
                });
            });
        }
        static void Cmd(string line)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c {line}",
                WindowStyle = ProcessWindowStyle.Hidden,
            }).WaitForExit();
        }

        static async void AStartProc(string fpath)
        {
            /*await Task.Run(() =>
            {
                Process.Start(fpath);
            });*/
            //ACmd($"{fpath[0]}: & cd \"{Path.GetDirectoryName(fpath)}\" & start {Path.GetFileName(fpath)}");
            ACmd($"cd \"{Path.GetDirectoryName(fpath)}\" & start {Path.GetFileName(fpath)}");
            //ACmd($"cd \"{Path.GetDirectoryName(fpath)}\" & start revLoader.exe");
        }
        static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }
        static Process GetProcess(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.FirstOrDefault();
        }

        static void Main(string[] args)
        {
            // cfg
            //proc:hl2.exe\r\ndll:C:\desk\r\ninj_delay:10
            Injector inj = new Injector();

            Console.ForegroundColor = ConsoleColor.Green;
            string cfg = "cfg.ini";
            //if (!File.Exists(cfg)) { File.WriteAllText(cfg, "proc:hl2.exe\r\ndll_paths:C:\\fullpath, C:\\fullpath2\r\ninj_delay_milis:2000\r\nproc_freeze:true"); Console.WriteLine("CFG MAKE!"); return; }
            if (!File.Exists(cfg)) { return; }
            string[] strs = File.ReadAllLines(cfg);

            //----------------------------------
            Console.ForegroundColor = ConsoleColor.Red;
            if (strs.Length != 3) { Console.WriteLine("CFG ERROR!"); Console.ReadKey(); return; }

            string path = strs[0].Replace("path:", "").Replace("\"", "").Replace("'", "");
            if (!File.Exists(path)) { Console.WriteLine($"path \"{path}\" NOT EXISTS!"); return; }
            List<string> dlls = strs[1].Replace("dll_paths:", "").Replace(", ", ",").Split(',').Select(x => x.Replace("\"", "").Replace("'", "")).ToList();
            int delay = int.Parse(strs[2].Replace("inj_delay_milis:", ""));
            //bool freeze = (strs[3].ToLower().Replace("proc_freeze:", "") == "true");

            //----------------------------ERRRS
            foreach (var dll in dlls) { if (!File.Exists(dll)) { Console.WriteLine($"DLL \"{dll}\" NOT FOUND!"); return; } }

            Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine($"Waiting proc \"{proc}\"...");
            //(bool, UInt32) p_id = (false, 0);
            //while (true) { p_id = inj.GetGamePID(proc); if (p_id.Item1) { break; } else { System.Threading.Thread.Sleep(10); } }
            ////if (!p_id.Item1) { Console.WriteLine($"PROC \"{proc}\" NOT FOUND!"); return; }
            //---------------------------------


            //---------START--PROC
            /*ProcessStartInfo startInfo = new ProcessStartInfo(path);
            startInfo.UseShellExecute = false;
            Process proc = Process.Start(startInfo);*/
            AStartProc(path);
            string proc_name = Path.GetFileNameWithoutExtension(path);
            while (true) { var p_id = inj.GetGamePID(proc_name); if (p_id.Item1) { break; } else { System.Threading.Thread.Sleep(10); } }
            Process proc = GetProcess(proc_name);

            //Console.WriteLine("Find! Waiting delay..");
            System.Threading.Thread.Sleep(delay);
            //inj.Inject(dll, pid: (int)p_id.Item2);

            //if (freeze) { } // freeze proc
            foreach (var dll in dlls)
            {
                inj.Inject(dll, proc.ProcessName);
                System.Threading.Thread.Sleep(1 * 1000);
                Console.WriteLine($"Injected \"{dll}\"");
            }
            //if (freeze) { } // unfreeze proc
            //Console.WriteLine("OK!");

            Console.WriteLine("DLL injected successfully.");
        }
    }
}