using System;

namespace ENet
{
    [Flags]
    public enum PacketFlags
    {
        None = 0,
        Reliable = 1,
        Unsequenced = 2,
        NoAllocate = 4,
        UnreliableFragmented = 8,
        Instant = 16,
        Unthrottled = 32,
        Sent = 256
    }
}