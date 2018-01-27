using EasyHook;
using InjectionLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MessageBoxHook_EasyHook
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        public Form1()
        {
            InitializeComponent();
        }

        private static bool InstallHookInternal(int processId)
        {
            try
            {
                var parameter = new HookParameter
                {
                    Msg = "Целевой процесс успешно внедрен",
                    HostProcessId = RemoteHooking.GetCurrentProcessId()
                };

                RemoteHooking.Inject(
                    processId,
                    InjectionOptions.Default,
                    typeof(HookParameter).Assembly.Location,
                    typeof(HookParameter).Assembly.Location,
                    string.Empty,
                    parameter
                );
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }

            return true;
        }

        private static bool IsWin64Emulator(int processId)
        {
            var process = Process.GetProcessById(processId);
            if (process == null)
                return false;

            if ((Environment.OSVersion.Version.Major > 5)
                || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
            {
                bool retVal;

                return !(IsWow64Process(process.Handle, out retVal) && retVal);
            }

            return false; // not on 64-bit Windows Emulator
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox1.Text, out int result))
            {
                MessageBox.Show("Only nuber");
                textBox1.Clear();
                return;
            }
            var p = Process.GetProcessById(result);
            if (p == null)
            {
                MessageBox.Show("Указанный процесс не существует!");
                return;
            }
            // Эта пока не нужно.
            //if (IsWin64Emulator(p.Id) != IsWin64Emulator(Process.GetCurrentProcess().Id))
            //{
            //    var currentPlat = IsWin64Emulator(Process.GetCurrentProcess().Id) ? 64 : 32;
            //    var targetPlat = IsWin64Emulator(p.Id) ? 64 : 32;
            //    MessageBox.Show($"Текущая программа {currentPlat} Bit разрядный, Программа на кторый внедряеться {targetPlat} Bit. Так не пойдет");
            //    return;
            //}

            InstallHookInternal(p.Id);
        }
    }
}
