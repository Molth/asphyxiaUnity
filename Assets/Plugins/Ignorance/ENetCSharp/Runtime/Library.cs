using System;

namespace ENet
{
    public static class Library
    {
        public const uint maxChannelCount = 255;
        public const uint maxPeers = 4095;
        public const uint maxPacketSize = 33554432;
        public const uint throttleThreshold = 40;
        public const uint throttleScale = 32;
        public const uint throttleAcceleration = 2;
        public const uint throttleDeceleration = 2;
        public const uint throttleInterval = 5000;
        public const uint timeoutLimit = 32;
        public const uint timeoutMinimum = 5000;
        public const uint timeoutMaximum = 30000;
        public const uint version = 132104;

        public static uint Time => Native.enet_time_get();

        public static bool Initialize()
        {
            if (Native.enet_linked_version() != 132104U)
                throw new InvalidOperationException("Incompatatible version");
            return Native.enet_initialize() == 0;
        }

        public static bool Initialize(Callbacks callbacks)
        {
            if (callbacks == null)
                throw new ArgumentNullException(nameof(callbacks));
            if (Native.enet_linked_version() != 132104U)
                throw new InvalidOperationException("Incompatatible version");
            ENetCallbacks nativeData = callbacks.NativeData;
            return Native.enet_initialize_with_callbacks(132104U, ref nativeData) == 0;
        }

        public static void Deinitialize() => Native.enet_deinitialize();

        public static ulong CRC64(IntPtr buffers, int bufferCount) => Native.enet_crc64(buffers, bufferCount);
    }
}