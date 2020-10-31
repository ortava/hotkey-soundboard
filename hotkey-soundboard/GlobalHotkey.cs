using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace hotkey_soundboard
{
    /// <summary>
    /// Provides a model for managing hotkey data (key and key modifier data) that allows for
    /// global registration of the hotkey with an action tied to it. When the hotkey is pressed
    /// the assigned action will execute.
    /// 
    /// Dispose methods are included to unregister and dispose of unwanted global hotkeys.
    /// </summary>

    public class GlobalHotkey : IDisposable
    {
        private static Dictionary<int, GlobalHotkey> _dictHotKeyToCalBackProc;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int keyId, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int keyId);

        public const int WmHotKey = 0x0312;

        private bool _disposed = false;

        public int Id { get; set; }
        public Key Key { get; set; }
        public KeyModifier Key_Modifiers { get; set; }
        public Action<GlobalHotkey> Action { get; set; }
        public int KeyId { get; private set; }

        /// integer values for identifying key and key modifiers ///
        public int Virtual_Key_Code { get; private set; }
        public int Modifiers { get; private set; }

        // converts integer identifiers of key and key modifiers into Key and KeyModifier objects
        public void ConvertToKey(int virtualKeyCode, int modifier)
        {
                Key = KeyInterop.KeyFromVirtualKey(virtualKeyCode);
                Key_Modifiers = (KeyModifier)modifier;
        }

        // registers a unique global hotkey to perform the stored action
        public void Register(bool IsInactive)
        {
            if (IsInactive)
                return;

            Virtual_Key_Code = KeyInterop.VirtualKeyFromKey(Key);
            Modifiers = (int)Key_Modifiers;
            KeyId = Virtual_Key_Code + (Modifiers * 0x10000);
            bool result = RegisterHotKey(IntPtr.Zero, KeyId, (UInt32)Key_Modifiers, (UInt32)Virtual_Key_Code);

            if (_dictHotKeyToCalBackProc == null)
            {
                _dictHotKeyToCalBackProc = new Dictionary<int, GlobalHotkey>();
                ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
            }

            _dictHotKeyToCalBackProc.Add(KeyId, this);

            return;
        }

        // unregisters the global hotkey
        public void Unregister()
        {
            GlobalHotkey hotkey;
            if (_dictHotKeyToCalBackProc.TryGetValue(KeyId, out hotkey))
            {
                UnregisterHotKey(IntPtr.Zero, KeyId);
                _dictHotKeyToCalBackProc.Remove(KeyId, out hotkey);
            }
        }

        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == WmHotKey)
                {
                    GlobalHotkey hotkey;

                    if (_dictHotKeyToCalBackProc.TryGetValue((int)msg.wParam, out hotkey))
                    {
                        if (hotkey.Action != null)
                        {
                            hotkey.Action.Invoke(hotkey);
                        }
                        handled = true;
                    }
                }
            }
        }

        // implement IDisposable.
        public void Dispose()
        {
            Dispose(true);              // this object will be cleaned up by the Dispose method


            GC.SuppressFinalize(this);  // call GC.SupressFinalize to take this object off the finalization queue
                                        // ...and prevent finalization code for this object from executing a second time.
        }

        // if disposing equals true, the method has been called directly or indirectly by a user's code
        // so managed and unmanaged resources can be _disposed.

        // if disposing equals false, the method has been called by the runtime from inside the finalizer
        // so only unmanaged resources can be _disposed.
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)        // check to see if Dispose has already been called
            {
                
                if (disposing)          // if disposing equals true, dispose all managed and unmanaged resources
                {
                    Unregister();       // dispose managed resources.
                }
      
                _disposed = true;       // note disposing has been done
            }
        }
    }

    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
    }

}