using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZoomAssistant
{
    static class ProcessesService
    {
        //Source: http://www.blackwasp.co.uk/GetActiveProcess.aspx
        //Thanks

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static uint? GetActiveProcessID()
        {
            var currentProcess = GetActiveProcess();
            if (currentProcess != null)
                return Convert.ToUInt32(currentProcess.Id);
            return null;
        }

        private static Process GetActiveProcess()
        {
            var hwnd = GetForegroundWindow();
            return GetProcessByHandle(hwnd);
        }

        private static Process GetProcessByHandle(IntPtr hwnd)
        {
            try
            {
                uint processID;
                GetWindowThreadProcessId(hwnd, out processID);
                return Process.GetProcessById((int)processID);
            }
            catch { return null; }
        }
    }
}
