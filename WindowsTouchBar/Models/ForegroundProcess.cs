using System.Diagnostics;

namespace WindowsTouchBar.Models
{
    internal class ForegroundProcess
    {
        public Process? Process { get; set; }

        public IntPtr Handle { get; set; }
    }
}
