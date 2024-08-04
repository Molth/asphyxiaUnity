using System;

namespace ENet
{
    internal static class ArrayPool
    {
        [ThreadStatic] private static byte[] byteBuffer;
        [ThreadStatic] private static IntPtr[] pointerBuffer;

        public static byte[] GetByteBuffer()
        {
            if (ArrayPool.byteBuffer == null)
                ArrayPool.byteBuffer = new byte[64];
            return ArrayPool.byteBuffer;
        }

        public static IntPtr[] GetPointerBuffer()
        {
            if (ArrayPool.pointerBuffer == null)
                ArrayPool.pointerBuffer = new IntPtr[4095];
            return ArrayPool.pointerBuffer;
        }
    }
}