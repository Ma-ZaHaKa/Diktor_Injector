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

namespace Injector
{ // BY Diktor
    class Program
    {
        static void Main(string[] args)
        {
            // cfg
            //proc:hl2.exe\r\ndll:C:\desk\r\ninj_delay:10
            Injector inj = new Injector();

            Console.ForegroundColor = ConsoleColor.Green;
            string cfg = "cfg.ini";
            if (!File.Exists(cfg)) { File.WriteAllText(cfg, "proc:hl2.exe\r\ndll_paths:C:\\fullpath, C:\\fullpath2\r\ninj_delay_milis:2000\r\nproc_freeze:true"); Console.WriteLine("CFG MAKE!"); return; }
            string[] strs = File.ReadAllLines(cfg);

            //----------------------------------
            Console.ForegroundColor = ConsoleColor.Red;
            if (strs.Length != 4) { Console.WriteLine("CFG ERROR!"); return; }
            string proc = strs[0].Replace("proc:", "").Replace(".exe", "").Replace(".EXE", "");
            List<string> dlls = strs[1].Replace("dll_paths:", "").Replace(", ", ",").Split(',').ToList();
            int delay = int.Parse(strs[2].Replace("inj_delay_milis:", ""));
            bool freeze = (strs[3].ToLower().Replace("proc_freeze:", "") == "true");

            //----------------------------ERRRS
            foreach (var dll in dlls) { if (!File.Exists(dll)) { Console.WriteLine($"DLL \"{dll}\" NOT FOUND!"); return; } }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Waiting proc \"{proc}\"...");
            (bool, UInt32) p_id = (false, 0);
            while (true) { p_id = inj.GetGamePID(proc); if (p_id.Item1) { break; } else { System.Threading.Thread.Sleep(10); } }
            //if (!p_id.Item1) { Console.WriteLine($"PROC \"{proc}\" NOT FOUND!"); return; }
            //---------------------------------

            Console.WriteLine("Find! Waiting delay..");
            System.Threading.Thread.Sleep(delay);
            //inj.Inject(dll, pid: (int)p_id.Item2);

            if (freeze) { } // freeze proc
            foreach (var dll in dlls)
            {
                inj.Inject(dll, proc);
                System.Threading.Thread.Sleep(1 * 1000);
                Console.WriteLine($"Injected \"{dll}\"");
            }
            if (freeze) { } // unfreeze proc
            //Console.WriteLine("OK!");

            Console.WriteLine("DLL injected successfully.");
        }
    }
}