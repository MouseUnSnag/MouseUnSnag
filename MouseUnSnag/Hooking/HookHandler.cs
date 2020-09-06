using System;
using System.Diagnostics;
using MouseUnSnag.Win32Interop;

namespace MouseUnSnag.Hooking
{
    public class HookHandler
    {
        private IntPtr _moduleHandle = IntPtr.Zero;

        public IntPtr SetHook(int HookNum, NativeMethods.HookProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule ?? throw new NullReferenceException("Main Module not found");
            if (_moduleHandle == IntPtr.Zero)
                _moduleHandle = NativeMethods.GetModuleHandle(curModule.ModuleName);
            return NativeMethods.SetWindowsHookEx(HookNum, proc, _moduleHandle, 0);
        }

        public static void UnsetHook(ref IntPtr hookHand)
        {
            if (hookHand == IntPtr.Zero)
                return;

            NativeMethods.UnhookWindowsHookEx(hookHand);
            hookHand = IntPtr.Zero;
        }
    }
}
