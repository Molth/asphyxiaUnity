using System;
using System.Runtime.InteropServices;

namespace ENet
{
    public class Host : IDisposable
    {
        private IntPtr nativeHost;

        internal IntPtr NativeData
        {
            get => this.nativeHost;
            set => this.nativeHost = value;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!(this.nativeHost != IntPtr.Zero))
                return;
            Native.enet_host_destroy(this.nativeHost);
            this.nativeHost = IntPtr.Zero;
        }

        ~Host() => this.Dispose(false);

        public bool IsSet => this.nativeHost != IntPtr.Zero;

        public uint PeersCount
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_host_get_peers_count(this.nativeHost);
            }
        }

        public uint PacketsSent
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_host_get_packets_sent(this.nativeHost);
            }
        }

        public uint PacketsReceived
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_host_get_packets_received(this.nativeHost);
            }
        }

        public uint BytesSent
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_host_get_bytes_sent(this.nativeHost);
            }
        }

        public uint BytesReceived
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_host_get_bytes_received(this.nativeHost);
            }
        }

        internal void ThrowIfNotCreated()
        {
            if (this.nativeHost == IntPtr.Zero)
                throw new InvalidOperationException("Host not created");
        }

        private static void ThrowIfChannelsExceeded(int channelLimit)
        {
            if (channelLimit < 0 || channelLimit > (int)byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(channelLimit));
        }

        public void Create() => this.Create(new Address?(), 1, 0);

        public void Create(int bufferSize) => this.Create(new Address?(), 1, 0, 0U, 0U, bufferSize);

        public void Create(Address? address, int peerLimit) => this.Create(address, peerLimit, 0);

        public void Create(Address? address, int peerLimit, int channelLimit) => this.Create(address, peerLimit, channelLimit, 0U, 0U, 0);

        public void Create(int peerLimit, int channelLimit) => this.Create(new Address?(), peerLimit, channelLimit, 0U, 0U, 0);

        public void Create(int peerLimit, int channelLimit, uint incomingBandwidth, uint outgoingBandwidth)
        {
            this.Create(new Address?(), peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth, 0);
        }

        public void Create(Address? address, int peerLimit, int channelLimit, uint incomingBandwidth, uint outgoingBandwidth)
        {
            this.Create(address, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth, 0);
        }

        public void Create(Address? address, int peerLimit, int channelLimit, uint incomingBandwidth, uint outgoingBandwidth, int bufferSize)
        {
            if (this.nativeHost != IntPtr.Zero)
                throw new InvalidOperationException("Host already created");
            if (peerLimit < 0 || peerLimit > 4095)
                throw new ArgumentOutOfRangeException(nameof(peerLimit));
            Host.ThrowIfChannelsExceeded(channelLimit);
            if (address.HasValue)
            {
                ENetAddress nativeData = address.Value.NativeData;
                this.nativeHost = Native.enet_host_create(ref nativeData, (IntPtr)peerLimit, (IntPtr)channelLimit, incomingBandwidth, outgoingBandwidth, bufferSize);
            }
            else
            {
                this.nativeHost = Native.enet_host_create(IntPtr.Zero, (IntPtr)peerLimit, (IntPtr)channelLimit, incomingBandwidth, outgoingBandwidth, bufferSize);
            }

            if (this.nativeHost == IntPtr.Zero)
                throw new InvalidOperationException("Host creation call failed");
        }

        public void PreventConnections(bool state)
        {
            this.ThrowIfNotCreated();
            Native.enet_host_prevent_connections(this.nativeHost, state ? (byte)1 : (byte)0);
        }

        public void Broadcast(byte channelID, ref Packet packet)
        {
            this.ThrowIfNotCreated();
            packet.ThrowIfNotCreated();
            Native.enet_host_broadcast(this.nativeHost, channelID, packet.NativeData);
            packet.NativeData = IntPtr.Zero;
        }

        public void Broadcast(byte channelID, ref Packet packet, Peer excludedPeer)
        {
            this.ThrowIfNotCreated();
            packet.ThrowIfNotCreated();
            Native.enet_host_broadcast_exclude(this.nativeHost, channelID, packet.NativeData, excludedPeer.NativeData);
            packet.NativeData = IntPtr.Zero;
        }

        public void Broadcast(byte channelID, ref Packet packet, Peer[] peers)
        {
            if (peers == null)
                throw new ArgumentNullException(nameof(peers));
            this.ThrowIfNotCreated();
            packet.ThrowIfNotCreated();
            if (peers.Length != 0)
            {
                IntPtr[] pointerBuffer = ArrayPool.GetPointerBuffer();
                int peersLength = 0;
                for (int index = 0; index < peers.Length; ++index)
                {
                    if (peers[index].NativeData != IntPtr.Zero)
                    {
                        pointerBuffer[peersLength] = peers[index].NativeData;
                        ++peersLength;
                    }
                }

                Native.enet_host_broadcast_selective(this.nativeHost, channelID, packet.NativeData, pointerBuffer, (IntPtr)peersLength);
                packet.NativeData = IntPtr.Zero;
            }
            else
            {
                packet.Dispose();
                throw new ArgumentOutOfRangeException("Peers array can't be empty");
            }
        }

        public int CheckEvents(out Event @event)
        {
            this.ThrowIfNotCreated();
            ENetEvent event1;
            int num = Native.enet_host_check_events(this.nativeHost, out event1);
            if (num <= 0)
            {
                @event = new Event();
                return num;
            }

            @event = new Event(event1);
            return num;
        }

        public Peer Connect(Address address) => this.Connect(address, 0, 0U);

        public Peer Connect(Address address, int channelLimit) => this.Connect(address, channelLimit, 0U);

        public Peer Connect(Address address, int channelLimit, uint data)
        {
            this.ThrowIfNotCreated();
            Host.ThrowIfChannelsExceeded(channelLimit);
            ENetAddress nativeData = address.NativeData;
            Peer peer = new Peer(Native.enet_host_connect(this.nativeHost, ref nativeData, (IntPtr)channelLimit, data));
            if (peer.NativeData == IntPtr.Zero)
                throw new InvalidOperationException("Host connect call failed");
            return peer;
        }

        public int Service(int timeout, out Event @event)
        {
            if (timeout < 0)
                throw new ArgumentOutOfRangeException(nameof(timeout));
            this.ThrowIfNotCreated();
            ENetEvent event1;
            int num = Native.enet_host_service(this.nativeHost, out event1, (uint)timeout);
            if (num <= 0)
            {
                @event = new Event();
                return num;
            }

            @event = new Event(event1);
            return num;
        }

        public void SetBandwidthLimit(uint incomingBandwidth, uint outgoingBandwidth)
        {
            this.ThrowIfNotCreated();
            Native.enet_host_bandwidth_limit(this.nativeHost, incomingBandwidth, outgoingBandwidth);
        }

        public void SetChannelLimit(int channelLimit)
        {
            this.ThrowIfNotCreated();
            Host.ThrowIfChannelsExceeded(channelLimit);
            Native.enet_host_channel_limit(this.nativeHost, (IntPtr)channelLimit);
        }

        public void SetMaxDuplicatePeers(ushort number)
        {
            this.ThrowIfNotCreated();
            Native.enet_host_set_max_duplicate_peers(this.nativeHost, number);
        }

        public void SetInterceptCallback(IntPtr callback)
        {
            this.ThrowIfNotCreated();
            Native.enet_host_set_intercept_callback(this.nativeHost, callback);
        }

        public void SetInterceptCallback(InterceptCallback callback)
        {
            this.ThrowIfNotCreated();
            Native.enet_host_set_intercept_callback(this.nativeHost, Marshal.GetFunctionPointerForDelegate<InterceptCallback>(callback));
        }

        public void SetChecksumCallback(IntPtr callback)
        {
            this.ThrowIfNotCreated();
            Native.enet_host_set_checksum_callback(this.nativeHost, callback);
        }

        public void SetChecksumCallback(ChecksumCallback callback)
        {
            this.ThrowIfNotCreated();
            Native.enet_host_set_checksum_callback(this.nativeHost, Marshal.GetFunctionPointerForDelegate<ChecksumCallback>(callback));
        }

        public void Flush()
        {
            this.ThrowIfNotCreated();
            Native.enet_host_flush(this.nativeHost);
        }
    }
}