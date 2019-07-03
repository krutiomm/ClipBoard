using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace clipboard
{
    public class ClipBoardMonitor
    {
        public ClipBoardMonitor(Action handle, Action writeHandle)
        {
            ClipboardChangedHandle = handle;
            ClipWriteHandle = writeHandle;
        }

        public  Action ClipboardChangedHandle;
        public Action ClipWriteHandle;

        public virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case ClipUnit.WM_CLIPBOARDUPDATE:
                    // HTodo  ：触发剪贴板变化事件 
                    ClipboardChangedHandle?.Invoke();
                    break;
                case ClipUnit.WM_QUERYENDSESSION:
                    ClipWriteHandle?.Invoke();
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
