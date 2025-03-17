using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;

namespace AtelierResleriana.Plugin.Localization
{
    public static partial class Extensions
    {
        public static unsafe byte[] ToBytes(this Il2CppStructArray<byte> il2CppBytes)
        {
            var size = il2CppBytes.Length;
            byte[] bytes = new byte[size];
            unsafe
            {
                fixed (byte* bytesPtr = bytes)
                {
                    byte* dataPtr = (byte*)IntPtr.Add(il2CppBytes.Pointer, 4 * IntPtr.Size).ToPointer();
                    Buffer.MemoryCopy(dataPtr, bytesPtr, size, size);
                }
            }

            return bytes;
        }

        public static unsafe Il2CppStructArray<byte> ToIl2CppBytes(this byte[] bytes)
        {
            Il2CppStructArray<byte> il2CppBytes = new Il2CppStructArray<byte>(bytes.Length);
            unsafe
            {
                byte* destPtr = (byte*)IntPtr.Add(il2CppBytes.Pointer, 4 * IntPtr.Size).ToPointer();
                fixed (byte* srcPtr = bytes)
                {
                    Buffer.MemoryCopy(srcPtr, destPtr, bytes.Length, bytes.Length);
                }
            }
            return il2CppBytes;
        }
    }
}
