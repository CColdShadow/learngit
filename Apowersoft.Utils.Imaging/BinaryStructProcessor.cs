using System;
using System.Runtime.InteropServices;

namespace Apowersoft.Utils.Imaging {
    public static class BinaryStructProcessor {
        public static T FromByteArray<T>(byte[] bytes) where T : struct {
            IntPtr ptr = IntPtr.Zero;
            try {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                return FromIntPtr<T>(ptr);
            }
            finally {
                if (ptr != IntPtr.Zero) {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        public static T FromIntPtr<T>(IntPtr intPtr) where T : struct {
            object obj = Marshal.PtrToStructure(intPtr, typeof(T));
            return (T)obj;
        }

        public static byte[] ToByteArray<T>(T obj) where T : struct {
            IntPtr ptr = IntPtr.Zero;
            try {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                return FromPtrToByteArray<T>(ptr);
            }
            finally {
                if (ptr != IntPtr.Zero) {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        public static byte[] FromPtrToByteArray<T>(IntPtr ptr) where T : struct {
            int size = Marshal.SizeOf(typeof(T));
            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            return bytes;
        }
    }
}