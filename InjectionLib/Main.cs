using EasyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InjectionLib
{
    public class Main : IEntryPoint
    {
        public LocalHook MessageBoxWHook = null;
        public LocalHook MessageBoxAHook = null;

        public Main(RemoteHooking.IContext context, String channelName, HookParameter parameter)
        {
            MessageBox.Show(parameter.Msg, "Hooked");
        }

        public void Run(RemoteHooking.IContext context, String channelName, HookParameter parameter)
        {
            var id = NativeAPI.GetCurrentProcessId();
            var libHandle = NativeAPI.GetCurrentThreadId();

            try
            {
                MessageBoxWHook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "MessageBoxW"),
                    new DMessageBoxW(MessageBoxW_Hooked),
                    this);
                MessageBoxWHook.ThreadACL.SetExclusiveACL(new Int32[1]);

                MessageBoxAHook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "MessageBoxA"),
                    new DMessageBoxW(MessageBoxA_Hooked),
                    this);
                MessageBoxAHook.ThreadACL.SetExclusiveACL(new Int32[1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {
                while (true)
                {
                    Thread.Sleep(10);
                }
            }
            catch
            {

            }
        }

        #region MessageBoxW

        [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
        public static extern IntPtr MessageBoxW(int hWnd, string text, string caption, uint type);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        delegate IntPtr DMessageBoxW(int hWnd, string text, string caption, uint type);

        static IntPtr MessageBoxW_Hooked(int hWnd, string text, string caption, uint type)
        {
            return MessageBoxW(hWnd, "Hooked - " + text, "Hooked - " + caption, type);
        }

        #endregion

        #region MessageBoxA

        [DllImport("user32.dll", EntryPoint = "MessageBoxA", CharSet = CharSet.Ansi)]
        public static extern IntPtr MessageBoxA(int hWnd, string text, string caption, uint type);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate IntPtr DMessageBoxA(int hWnd, string text, string caption, uint type);

        static IntPtr MessageBoxA_Hooked(int hWnd, string text, string caption, uint type)
        {
            return MessageBoxA(hWnd, "Hooked - " + text, "Hooked - " + caption, type);
        }

        #endregion
    }
}
