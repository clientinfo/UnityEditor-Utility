using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace unity_editor_utils.Core.Detour
{
    /// <summary>
    /// Provides functionality to detour (hook) methods at runtime by replacing their original instructions with a jump to a custom hook method.
    /// </summary>
    public class Detour : IDisposable
    {
        private const uint HookSizeX64 = 12;
        private const uint HookSizeX86 = 7;
        private byte[] _original;
        private bool _disposed;

        private static readonly object Lock = new object();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

        /// <summary>
        /// Initializes a new instance of the <see cref="Detour"/> class.
        /// </summary>
        public Detour()
        {
            _original = null;
            OriginalMethod = HookedMethod = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Detour"/> class with the specified original and hook methods.
        /// </summary>
        /// <param name="originalFunctionInfo">The original method to be detoured.</param>
        /// <param name="hookFunctionInfo">The hook method to detour to.</param>
        public Detour(MethodInfo originalFunctionInfo, MethodInfo hookFunctionInfo)
        {
            _original = null;
            Init(originalFunctionInfo, hookFunctionInfo);
        }

        /// <summary>
        /// Gets the original method to be detoured.
        /// </summary>
        public MethodInfo OriginalMethod { get; private set; }

        /// <summary>
        /// Gets the hook method to detour to.
        /// </summary>
        public MethodInfo HookedMethod { get; private set; }

        /// <summary>
        /// Initializes the detour with the specified original and hook methods.
        /// </summary>
        /// <param name="originalFunctionInfo">The original method to be detoured.</param>
        /// <param name="hookFunctionInfo">The hook method to detour to.</param>
        public void Init(MethodInfo originalFunctionInfo, MethodInfo hookFunctionInfo)
        {
            if (originalFunctionInfo == null || hookFunctionInfo == null)
            {
                throw new ArgumentException("Both original and hook methods need to be valid.");
            }

            lock (Lock)
            {
                RuntimeHelpers.PrepareMethod(originalFunctionInfo.MethodHandle);
                RuntimeHelpers.PrepareMethod(hookFunctionInfo.MethodHandle);

                var originalFunctionPointer = originalFunctionInfo.MethodHandle.GetFunctionPointer();
                var hookFunctionPointer = hookFunctionInfo.MethodHandle.GetFunctionPointer();

                if (originalFunctionPointer == IntPtr.Zero)
                {
                    throw new ArgumentException("Failed to get the function pointer of the original method.");
                }

                if (hookFunctionPointer == IntPtr.Zero)
                {
                    throw new ArgumentException("Failed to get the function pointer of the hook method.");
                }

                OriginalMethod = originalFunctionInfo;
                HookedMethod = hookFunctionInfo;
            }
        }

        /// <summary>
        /// Applies the hook, replacing the original method instructions with a jump to the hook method.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the hook is not initialized before use.</exception>
        public void ApplyHook()
        {
            if (OriginalMethod == null || HookedMethod == null)
            {
                throw new InvalidOperationException("Hook must be initialized before use.");
            }

            if (_original != null)
            {
                return;
            }

            lock (Lock)
            {
                var originalFunctionPointer = OriginalMethod.MethodHandle.GetFunctionPointer();
                var hookedFunctionPointer = HookedMethod.MethodHandle.GetFunctionPointer();
                uint oldProtection;

                if (originalFunctionPointer == IntPtr.Zero)
                {
                    throw new ArgumentException("Failed to get the function pointer of the original method.");
                }

                if (hookedFunctionPointer == IntPtr.Zero)
                {
                    throw new ArgumentException("Failed to get the function pointer of the hooked method.");
                }

                if (IntPtr.Size == 8)
                {
                    _original = new byte[HookSizeX64];
                    if (!VirtualProtect(originalFunctionPointer, HookSizeX64, 0x40, out oldProtection))
                    {
                        throw new ArgumentException("Failed to change memory protection to execute-readwrite.");
                    }

                    unsafe
                    {
                        var ptr = (byte*)originalFunctionPointer;
                        if (ptr == null)
                        {
                            throw new ArgumentException("Failed to get the pointer to the original function.");
                        }

                        Marshal.Copy(new IntPtr(ptr), _original, 0, (int)HookSizeX64);

                        // 64-bit jump: Move the address of hookedFunctionPointer into RAX, then jump to the address in RAX
                        ptr[0] = 0x48; // MOV RAX, immediate value
                        ptr[1] = 0xB8; // Part of the MOV RAX instruction
                        *(IntPtr*)(ptr + 2) = hookedFunctionPointer; // The address to move into RAX
                        ptr[10] = 0xFF; // JMP RAX
                        ptr[11] = 0xE0; // Part of the JMP RAX instruction
                    }

                    if (!VirtualProtect(originalFunctionPointer, HookSizeX64, oldProtection, out _))
                    {
                        throw new ArgumentException("Failed to restore memory protection.");
                    }
                }
                else
                {
                    _original = new byte[HookSizeX86];
                    if (!VirtualProtect(originalFunctionPointer, HookSizeX86, 0x40, out oldProtection))
                    {
                        throw new ArgumentException("Failed to change memory protection to execute-readwrite.");
                    }

                    unsafe
                    {
                        var ptr = (byte*)originalFunctionPointer;
                        if (ptr == null)
                        {
                            throw new ArgumentException("Failed to get the pointer to the original function.");
                        }

                        Marshal.Copy(new IntPtr(ptr), _original, 0, (int)HookSizeX86);

                        // 32-bit jump: Move the address of hookedFunctionPointer into EAX, then jump to the address in EAX
                        ptr[0] = 0xB8; // MOV EAX, immediate value
                        *(IntPtr*)(ptr + 1) = hookedFunctionPointer; // The address to move into EAX
                        ptr[5] = 0xFF; // JMP EAX
                        ptr[6] = 0xE0; // Part of the JMP EAX instruction
                    }

                    if (!VirtualProtect(originalFunctionPointer, HookSizeX86, oldProtection, out _))
                    {
                        throw new ArgumentException("Failed to restore memory protection.");
                    }
                }
            }
        }


        /// <summary>
        /// Restores the original method instructions, removing the hook.
        /// </summary>
        public void Unhook()
        {
            if (_original == null)
            {
                return;
            }

            lock (Lock)
            {
                var codeSize = (uint)_original.Length;
                var originalFunctionPointer = OriginalMethod.MethodHandle.GetFunctionPointer();

                if (originalFunctionPointer == IntPtr.Zero)
                {
                    throw new ArgumentException("Failed to get the function pointer of the original method.");
                }

                if (!VirtualProtect(originalFunctionPointer, codeSize, 0x40, out var oldProtection))
                {
                    throw new ArgumentException("Failed to change memory protection to execute-readwrite.");
                }

                unsafe
                {
                    var ptr = (byte*)originalFunctionPointer;
                    if (ptr == null)
                    {
                        throw new ArgumentException("Failed to get the pointer to the original function.");
                    }

                    Marshal.Copy(_original, 0, new IntPtr(ptr), (int)codeSize);
                }

                if (!VirtualProtect(originalFunctionPointer, codeSize, oldProtection, out _))
                {
                    throw new ArgumentException("Failed to restore memory protection.");
                }

                _original = null;
            }
        }
    

        /// <summary>
        /// Calls the original method temporarily by unhooking, invoking the method, and reapplying the hook.
        /// </summary>
        /// <param name="this">The instance of the class containing the original method.</param>
        /// <param name="params">The parameters to pass to the original method.</param>
        /// <exception cref="InvalidOperationException">Thrown if the original method is not set.</exception>
        public void CallOriginalFunction(object @this, object[] @params)
        {
            if (OriginalMethod == null)
            {
                throw new InvalidOperationException("Original method is not set.");
            }

            Unhook();
            try
            {
                OriginalMethod.Invoke(@this, @params);
            }
            finally
            {
                ApplyHook();
            }
        }

        /// <summary>
        /// Disposes of the detour, ensuring the original method is unhooked.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Unhook();
                _disposed = true;
            }
        }
    }
}
