using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Injector
{
    public class Injector
    {
        // DLL imports

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        // Privileges 
        private const int ProcessSuspendResume = 0x0800;
        private const int ProcessCreateThread = 0x0002;
        private const int ProcessQueryInformation = 0x0400;
        private const int ProcessVmOperation = 0x0008;
        private const int ProcessVmWrite = 0x0020;
        private const int ProcessVmRead = 0x0010;
        private const int ProcessAllAccess = ProcessCreateThread | ProcessQueryInformation | ProcessVmOperation | ProcessVmWrite | ProcessVmRead;

        // Memory Allocation

        private const uint MemoryCommit = 0x00001000;
        private const uint MemoryReserve = 0x00002000;
        private const uint PageReadWrite = 4;

        //--GET--PID
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        public (bool, UInt32) GetGamePID(string proc_name)
        {
            UInt32 ret = 0;
            Process[] proc = Process.GetProcessesByName(proc_name);

            if (proc.Length == 0)
            {
                return (false, ret);
            }

            IntPtr hwGame = proc[0].MainWindowHandle;

            if (hwGame == IntPtr.Zero)
            {
                return (false, ret);
            }

            GetWindowThreadProcessId(hwGame, out ret);

            return (true, ret);
        }

        // @return List logs str-s
        public List<string> Inject(string dllPath, string processName = "", int pid = 0)
        {
            // Get the process id
            List<string> logs = new List<string>();
            if (processName == "" && pid == 0) { logs.Add(@"Empty Proc Input"); return logs; }

            var processId = 0;
            if (processName != "") { processId = Process.GetProcessesByName(processName)[0].Id; }
            else { processId = pid; }

            // Get the address of LoadLibraryA

            var loadLibraryA = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            if (loadLibraryA == IntPtr.Zero)
            {
                var lastError = Marshal.GetLastWin32Error();
                logs.Add(@"Failed to find kernel32.dll");
            }

            else
            {
                logs.Add(@"Successfully loaded kernel32.dll");
            }

            // Get the handle for the process

            var processHandle = OpenProcess(ProcessAllAccess, false, processId);

            if (processHandle == IntPtr.Zero)
            {
                var lastError = Marshal.GetLastWin32Error();
                logs.Add(@"Failed to get process handle");
            }

            else
            {
                logs.Add(@"Successfully found process handle");
            }

            // Allocate memory in the process

            var memory = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)(dllPath.Length + 1), MemoryCommit | MemoryReserve, PageReadWrite);

            if (memory == IntPtr.Zero)
            {
                var lastError = Marshal.GetLastWin32Error();
                logs.Add(@"Failed to allocate memory");
            }

            else
            {
                logs.Add(@"Successfully allocated memory");
            }


            // Write memory in the process

            if (WriteProcessMemory(processHandle, memory, Encoding.Default.GetBytes(dllPath), (uint)(dllPath.Length + 1), 0) == 0)
            {
                var lastError = Marshal.GetLastWin32Error();
                logs.Add(@"Failed to write memory");
            }

            else
            {
                logs.Add(@"Successfully wrote memory");
            }

            // Create a thread to call LoadLibraryA in the process

            if (CreateRemoteThread(processHandle, IntPtr.Zero, 0, loadLibraryA, memory, 0, IntPtr.Zero) == IntPtr.Zero)
            {
                var lastError = Marshal.GetLastWin32Error();
                logs.Add(@"Failed to create remote thread");
            }

            else
            {
                logs.Add(@"Successfully created remote thread");
            }

            return logs;
        }


    }
}
