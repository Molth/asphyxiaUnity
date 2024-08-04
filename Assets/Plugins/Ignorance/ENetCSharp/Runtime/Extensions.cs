using System;

namespace ENet
{
    public static class Extensions
    {
        public static int StringLength(this byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            int index = 0;
            while (index < data.Length && data[index] != (byte)0)
                ++index;
            return index;
        }
    }
}