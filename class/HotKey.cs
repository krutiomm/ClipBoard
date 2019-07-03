using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace clipboard
{
    internal class HotKeyWinApi

    {
        public const int WmHotKey = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]

        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, Keys vk);



        [DllImport("user32.dll", SetLastError = true)]

        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    }

    public sealed class HotKey : IDisposable

    {
        public event Action<HotKey> HotKeyPressed;
        
        private readonly int _id;

        private bool _isKeyRegistered;

        readonly IntPtr _handle;



        public HotKey(ModifierKeys modifierKeys, Keys key, Window window)
            : this(modifierKeys, key, new WindowInteropHelper(window))
        {
            Contract.Requires(window != null);
        }

        public HotKey(ModifierKeys modifierKeys, Keys key, WindowInteropHelper window)
            : this(modifierKeys, key, window.Handle)
        {
            Contract.Requires(window != null);
        }
        
        public HotKey(ModifierKeys modifierKeys, Keys key, IntPtr windowHandle)
        {
            Contract.Requires(modifierKeys != ModifierKeys.None || key != Keys.None);
            Contract.Requires(windowHandle != IntPtr.Zero);
            
            Key = key;
            KeyModifier = modifierKeys;
            _id = GetHashCode();
            _handle = windowHandle;
            RegisterHotKey();
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        ~HotKey()
        {
            Dispose();
        }
        
        public Keys Key { get; private set; }
        
        public ModifierKeys KeyModifier { get; private set; }
        public void RegisterHotKey()
        {
            if (Key == Keys.None)
                return;
            if (_isKeyRegistered)
                UnregisterHotKey();
            _isKeyRegistered = HotKeyWinApi.RegisterHotKey(_handle, _id, KeyModifier, Key);
            if (!_isKeyRegistered)
                throw new ApplicationException(string.Format("快捷键{0}|{1}已被占用", KeyModifier, Key));
        }


        public void UnregisterHotKey()
        {
            _isKeyRegistered = !HotKeyWinApi.UnregisterHotKey(_handle, _id);
        }


        public void Dispose()
        {
            try
            {
                ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
                UnregisterHotKey();
            }
            catch (Exception ex)
            {

            }
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == HotKeyWinApi.WmHotKey
                    && (int)(msg.wParam) == _id)
                {
                    OnHotKeyPressed();
                    handled = true;
                }
            }
        }
        

        private void OnHotKeyPressed()
        {
            if (HotKeyPressed != null)
                HotKeyPressed(this);
        }

    }

    public static class HotKeyHelper
    {
        public static ModifierKeys ConvertMKey(this string keyName)
        {
            if (keyName.ToUpper() == ModifierKeys.Alt.ToString().ToUpper())
            {
                return ModifierKeys.Alt;
            }
            if (keyName.ToUpper() == ModifierKeys.Control.ToString().ToUpper() || keyName.ToUpper() == "CTRL")
            {
                return ModifierKeys.Control;
            }
            if (keyName.ToUpper() == ModifierKeys.Shift.ToString().ToUpper())
            {
                return ModifierKeys.Shift;
            }
            if (keyName.ToUpper() == ModifierKeys.Windows.ToString().ToUpper())
            {
                return ModifierKeys.Windows;
            }
            return ModifierKeys.None;
        }

        public static Keys ConvertKey(this string keyName)
        {
            if (keyName.ToUpper() == Keys.A.ToString().ToUpper())
            {
                return Keys.A;
            }
            if (keyName.ToUpper() == Keys.Add.ToString().ToUpper())
            {
                return Keys.Add;
            }
            if (keyName.ToUpper() == Keys.B.ToString().ToUpper())
            {
                return Keys.B;
            }
            if (keyName.ToUpper() == Keys.Back.ToString().ToUpper())
            {
                return Keys.Back;
            }
            if (keyName.ToUpper() == Keys.C.ToString().ToUpper())
            {
                return Keys.C;
            }
            if (keyName.ToUpper() == Keys.Cancel.ToString().ToUpper())
            {
                return Keys.Cancel;
            }
            if (keyName.ToUpper() == Keys.Clear.ToString().ToUpper())
            {
                return Keys.Clear;
            }
            if (keyName.ToUpper() == Keys.D.ToString().ToUpper())
            {
                return Keys.D;
            }
            if (keyName.ToUpper() == Keys.D0.ToString().ToUpper())
            {
                return Keys.D0;
            }
            if (keyName.ToUpper() == Keys.D1.ToString().ToUpper())
            {
                return Keys.D1;
            }
            if (keyName.ToUpper() == Keys.D2.ToString().ToUpper())
            {
                return Keys.D2;
            }
            if (keyName.ToUpper() == Keys.D3.ToString().ToUpper())
            {
                return Keys.D3;
            }
            if (keyName.ToUpper() == Keys.D4.ToString().ToUpper())
            {
                return Keys.D4;
            }
            if (keyName.ToUpper() == Keys.D5.ToString().ToUpper())
            {
                return Keys.D5;
            }
            if (keyName.ToUpper() == Keys.D6.ToString().ToUpper())
            {
                return Keys.D6;
            }
            if (keyName.ToUpper() == Keys.D7.ToString().ToUpper())
            {
                return Keys.D7;
            }
            if (keyName.ToUpper() == Keys.D8.ToString().ToUpper())
            {
                return Keys.D8;
            }
            if (keyName.ToUpper() == Keys.D9.ToString().ToUpper())
            {
                return Keys.D9;
            }
            if (keyName.ToUpper() == Keys.Decimal.ToString().ToUpper())
            {
                return Keys.Decimal;
            }
            if (keyName.ToUpper() == Keys.Delete.ToString().ToUpper())
            {
                return Keys.Delete;
            }
            if (keyName.ToUpper() == Keys.Divide.ToString().ToUpper())
            {
                return Keys.Divide;
            }
            if (keyName.ToUpper() == Keys.Down.ToString().ToUpper())
            {
                return Keys.Down;
            }
            if (keyName.ToUpper() == Keys.E.ToString().ToUpper())
            {
                return Keys.E;
            }
            if (keyName.ToUpper() == Keys.End.ToString().ToUpper())
            {
                return Keys.End;
            }
            if (keyName.ToUpper() == Keys.Enter.ToString().ToUpper())
            {
                return Keys.Enter;
            }
            if (keyName.ToUpper() == Keys.Escape.ToString().ToUpper())
            {
                return Keys.Escape;
            }
            if (keyName.ToUpper() == Keys.Execute.ToString().ToUpper())
            {
                return Keys.Execute;
            }
            if (keyName.ToUpper() == Keys.F.ToString().ToUpper())
            {
                return Keys.F;
            }
            if (keyName.ToUpper() == Keys.F1.ToString().ToUpper())
            {
                return Keys.F1;
            }
            if (keyName.ToUpper() == Keys.F2.ToString().ToUpper())
            {
                return Keys.F2;
            }
            if (keyName.ToUpper() == Keys.F3.ToString().ToUpper())
            {
                return Keys.F3;
            }
            if (keyName.ToUpper() == Keys.F4.ToString().ToUpper())
            {
                return Keys.F4;
            }
            if (keyName.ToUpper() == Keys.F5.ToString().ToUpper())
            {
                return Keys.F5;
            }
            if (keyName.ToUpper() == Keys.F6.ToString().ToUpper())
            {
                return Keys.F6;
            }
            if (keyName.ToUpper() == Keys.F7.ToString().ToUpper())
            {
                return Keys.F7;
            }
            if (keyName.ToUpper() == Keys.F8.ToString().ToUpper())
            {
                return Keys.F8;
            }
            if (keyName.ToUpper() == Keys.F9.ToString().ToUpper())
            {
                return Keys.F9;
            }
            if (keyName.ToUpper() == Keys.F10.ToString().ToUpper())
            {
                return Keys.F10;
            }
            if (keyName.ToUpper() == Keys.F11.ToString().ToUpper())
            {
                return Keys.F11;
            }
            if (keyName.ToUpper() == Keys.F12.ToString().ToUpper())
            {
                return Keys.F12;
            }
            if (keyName.ToUpper() == Keys.F13.ToString().ToUpper())
            {
                return Keys.F13;
            }
            if (keyName.ToUpper() == Keys.F14.ToString().ToUpper())
            {
                return Keys.F14;
            }
            if (keyName.ToUpper() == Keys.F15.ToString().ToUpper())
            {
                return Keys.F15;
            }
            if (keyName.ToUpper() == Keys.G.ToString().ToUpper())
            {
                return Keys.G;
            }
            if (keyName.ToUpper() == Keys.H.ToString().ToUpper())
            {
                return Keys.H;
            }
            if (keyName.ToUpper() == Keys.Home.ToString().ToUpper())
            {
                return Keys.Home;
            }
            if (keyName.ToUpper() == Keys.I.ToString().ToUpper())
            {
                return Keys.I;
            }
            if (keyName.ToUpper() == Keys.Insert.ToString().ToUpper())
            {
                return Keys.Insert;
            }
            if (keyName.ToUpper() == Keys.J.ToString().ToUpper())
            {
                return Keys.J;
            }
            if (keyName.ToUpper() == Keys.K.ToString().ToUpper())
            {
                return Keys.K;
            }
            if (keyName.ToUpper() == Keys.L.ToString().ToUpper())
            {
                return Keys.L;
            }
            if (keyName.ToUpper() == Keys.LButton.ToString().ToUpper())
            {
                return Keys.LButton;
            }
            if (keyName.ToUpper() == Keys.Left.ToString().ToUpper())
            {
                return Keys.Left;
            }
            if (keyName.ToUpper() == Keys.LineFeed.ToString().ToUpper())
            {
                return Keys.LineFeed;
            }
            if (keyName.ToUpper() == Keys.M.ToString().ToUpper())
            {
                return Keys.M;
            }
            if (keyName.ToUpper() == Keys.MButton.ToString().ToUpper())
            {
                return Keys.MButton;
            }
            if (keyName.ToUpper() == Keys.Multiply.ToString().ToUpper())
            {
                return Keys.Multiply;
            }
            if (keyName.ToUpper() == Keys.N.ToString().ToUpper())
            {
                return Keys.N;
            }
            if (keyName.ToUpper() == Keys.Next.ToString().ToUpper())
            {
                return Keys.Next;
            }
            if (keyName.ToUpper() == Keys.NumLock.ToString().ToUpper())
            {
                return Keys.NumLock;
            }
            if (keyName.ToUpper() == Keys.NumPad0.ToString().ToUpper())
            {
                return Keys.NumPad0;
            }
            if (keyName.ToUpper() == Keys.NumPad1.ToString().ToUpper())
            {
                return Keys.NumPad1;
            }
            if (keyName.ToUpper() == Keys.NumPad2.ToString().ToUpper())
            {
                return Keys.NumPad2;
            }
            if (keyName.ToUpper() == Keys.NumPad3.ToString().ToUpper())
            {
                return Keys.NumPad3;
            }
            if (keyName.ToUpper() == Keys.NumPad4.ToString().ToUpper())
            {
                return Keys.NumPad4;
            }
            if (keyName.ToUpper() == Keys.NumPad5.ToString().ToUpper())
            {
                return Keys.NumPad5;
            }
            if (keyName.ToUpper() == Keys.NumPad6.ToString().ToUpper())
            {
                return Keys.NumPad6;
            }
            if (keyName.ToUpper() == Keys.NumPad7.ToString().ToUpper())
            {
                return Keys.NumPad7;
            }
            if (keyName.ToUpper() == Keys.NumPad8.ToString().ToUpper())
            {
                return Keys.NumPad8;
            }
            if (keyName.ToUpper() == Keys.NumPad9.ToString().ToUpper())
            {
                return Keys.NumPad9;
            }
            if (keyName.ToUpper() == Keys.O.ToString().ToUpper())
            {
                return Keys.O;
            }
            if (keyName.ToUpper() == Keys.P.ToString().ToUpper())
            {
                return Keys.P;
            }
            if (keyName.ToUpper() == Keys.PageDown.ToString().ToUpper())
            {
                return Keys.PageDown;
            }
            if (keyName.ToUpper() == Keys.PageUp.ToString().ToUpper())
            {
                return Keys.PageUp;
            }
            if (keyName.ToUpper() == Keys.Pause.ToString().ToUpper())
            {
                return Keys.Pause;
            }
            if (keyName.ToUpper() == Keys.Print.ToString().ToUpper())
            {
                return Keys.Print;
            }
            if (keyName.ToUpper() == Keys.PrintScreen.ToString().ToUpper())
            {
                return Keys.PrintScreen;
            }
            if (keyName.ToUpper() == Keys.Q.ToString().ToUpper())
            {
                return Keys.Q;
            }
            if (keyName.ToUpper() == Keys.R.ToString().ToUpper())
            {
                return Keys.R;
            }
            if (keyName.ToUpper() == Keys.RButton.ToString().ToUpper())
            {
                return Keys.RButton;
            }
            if (keyName.ToUpper() == Keys.S.ToString().ToUpper())
            {
                return Keys.S;
            }
            if (keyName.ToUpper() == Keys.Scroll.ToString().ToUpper())
            {
                return Keys.Scroll;
            }
            if (keyName.ToUpper() == Keys.Space.ToString().ToUpper())
            {
                return Keys.Space;
            }
            if (keyName.ToUpper() == Keys.T.ToString().ToUpper())
            {
                return Keys.T;
            }
            if (keyName.ToUpper() == Keys.Tab.ToString().ToUpper())
            {
                return Keys.Tab;
            }
            if (keyName.ToUpper() == Keys.U.ToString().ToUpper())
            {
                return Keys.U;
            }
            if (keyName.ToUpper() == Keys.Up.ToString().ToUpper())
            {
                return Keys.Up;
            }
            if (keyName.ToUpper() == Keys.V.ToString().ToUpper())
            {
                return Keys.V;
            }
            if (keyName.ToUpper() == Keys.W.ToString().ToUpper())
            {
                return Keys.W;
            }
            if (keyName.ToUpper() == Keys.X.ToString().ToUpper())
            {
                return Keys.X;
            }
            if (keyName.ToUpper() == Keys.Y.ToString().ToUpper())
            {
                return Keys.Y;
            }
            if (keyName.ToUpper() == Keys.Z.ToString().ToUpper())
            {
                return Keys.Z;
            }
            return Keys.None;
        }
    }
}
