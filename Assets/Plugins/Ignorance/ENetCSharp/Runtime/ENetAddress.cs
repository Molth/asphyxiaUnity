using System.Runtime.InteropServices;

namespace ENet
{
    [StructLayout(LayoutKind.Explicit, Size = 18)]
    internal struct ENetAddress
    {
        [FieldOffset(16)] public ushort port;
    }
}