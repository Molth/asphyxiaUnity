using System;
using System.Runtime.InteropServices;

namespace ENet
{
    public struct Packet : IDisposable
    {
        private IntPtr nativePacket;

        internal IntPtr NativeData
        {
            get => this.nativePacket;
            set => this.nativePacket = value;
        }

        internal Packet(IntPtr packet) => this.nativePacket = packet;

        public void Dispose()
        {
            if (!(this.nativePacket != IntPtr.Zero))
                return;
            Native.enet_packet_dispose(this.nativePacket);
            this.nativePacket = IntPtr.Zero;
        }

        public bool IsSet => this.nativePacket != IntPtr.Zero;

        public IntPtr Data
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_packet_get_data(this.nativePacket);
            }
        }

        public IntPtr UserData
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_packet_get_user_data(this.nativePacket);
            }
            set
            {
                this.ThrowIfNotCreated();
                Native.enet_packet_set_user_data(this.nativePacket, value);
            }
        }

        public int Length
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_packet_get_length(this.nativePacket);
            }
        }

        public bool HasReferences
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_packet_check_references(this.nativePacket) != 0;
            }
        }

        internal void ThrowIfNotCreated()
        {
            if (this.nativePacket == IntPtr.Zero)
                throw new InvalidOperationException("Packet not created");
        }

        public void SetFreeCallback(IntPtr callback)
        {
            this.ThrowIfNotCreated();
            Native.enet_packet_set_free_callback(this.nativePacket, callback);
        }

        public void SetFreeCallback(PacketFreeCallback callback)
        {
            this.ThrowIfNotCreated();
            Native.enet_packet_set_free_callback(this.nativePacket, Marshal.GetFunctionPointerForDelegate<PacketFreeCallback>(callback));
        }

        public void Create(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            this.Create(data, data.Length);
        }

        public void Create(byte[] data, int length) => this.Create(data, length, PacketFlags.None);

        public void Create(byte[] data, PacketFlags flags) => this.Create(data, data.Length, flags);

        public void Create(byte[] data, int length, PacketFlags flags)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (length < 0 || length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
            this.nativePacket = Native.enet_packet_create(data, (IntPtr)length, flags);
        }

        public void Create(IntPtr data, int length, PacketFlags flags)
        {
            if (data == IntPtr.Zero)
                throw new ArgumentNullException(nameof(data));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            this.nativePacket = Native.enet_packet_create(data, (IntPtr)length, flags);
        }

        public void Create(byte[] data, int offset, int length, PacketFlags flags)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
            this.nativePacket = Native.enet_packet_create_offset(data, (IntPtr)length, (IntPtr)offset, flags);
        }

        public void Create(IntPtr data, int offset, int length, PacketFlags flags)
        {
            if (data == IntPtr.Zero)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            this.nativePacket = Native.enet_packet_create_offset(data, (IntPtr)length, (IntPtr)offset, flags);
        }

        public void CopyTo(byte[] destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            Marshal.Copy(this.Data, destination, 0, this.Length);
        }
    }
}