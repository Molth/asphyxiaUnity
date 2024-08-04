using System;
using System.Text;

namespace ENet
{
    public struct Peer
    {
        private IntPtr nativePeer;
        private uint nativeID;

        internal IntPtr NativeData
        {
            get => this.nativePeer;
            set => this.nativePeer = value;
        }

        internal Peer(IntPtr peer)
        {
            this.nativePeer = peer;
            this.nativeID = this.nativePeer != IntPtr.Zero ? Native.enet_peer_get_id(this.nativePeer) : 0U;
        }

        public bool IsSet => this.nativePeer != IntPtr.Zero;

        public uint ID => this.nativeID;

        public string IP
        {
            get
            {
                this.ThrowIfNotCreated();
                byte[] byteBuffer = ArrayPool.GetByteBuffer();
                return Native.enet_peer_get_ip(this.nativePeer, byteBuffer, (IntPtr)byteBuffer.Length) == 0 ? Encoding.ASCII.GetString(byteBuffer, 0, byteBuffer.StringLength()) : string.Empty;
            }
        }

        public ushort Port
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_port(this.nativePeer);
            }
        }

        public uint MTU
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_mtu(this.nativePeer);
            }
        }

        public PeerState State => !(this.nativePeer == IntPtr.Zero) ? Native.enet_peer_get_state(this.nativePeer) : PeerState.Uninitialized;

        public uint RoundTripTime
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_rtt(this.nativePeer);
            }
        }

        public uint LastRoundTripTime
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_last_rtt(this.nativePeer);
            }
        }

        public uint LastSendTime
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_lastsendtime(this.nativePeer);
            }
        }

        public uint LastReceiveTime
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_lastreceivetime(this.nativePeer);
            }
        }

        public ulong PacketsSent
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_packets_sent(this.nativePeer);
            }
        }

        public ulong PacketsLost
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_packets_lost(this.nativePeer);
            }
        }

        public float PacketsThrottle
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_packets_throttle(this.nativePeer);
            }
        }

        public ulong BytesSent
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_bytes_sent(this.nativePeer);
            }
        }

        public ulong BytesReceived
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_bytes_received(this.nativePeer);
            }
        }

        public IntPtr Data
        {
            get
            {
                this.ThrowIfNotCreated();
                return Native.enet_peer_get_data(this.nativePeer);
            }
            set
            {
                this.ThrowIfNotCreated();
                Native.enet_peer_set_data(this.nativePeer, value);
            }
        }

        internal void ThrowIfNotCreated()
        {
            if (this.nativePeer == IntPtr.Zero)
                throw new InvalidOperationException("Peer not created");
        }

        public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration, uint threshold)
        {
            this.ThrowIfNotCreated();
            Native.enet_peer_throttle_configure(this.nativePeer, interval, acceleration, deceleration, threshold);
        }

        public bool Send(byte channelID, ref Packet packet)
        {
            this.ThrowIfNotCreated();
            packet.ThrowIfNotCreated();
            return Native.enet_peer_send(this.nativePeer, channelID, packet.NativeData) == 0;
        }

        public bool Receive(out byte channelID, out Packet packet)
        {
            this.ThrowIfNotCreated();
            IntPtr packet1 = Native.enet_peer_receive(this.nativePeer, out channelID);
            if (packet1 != IntPtr.Zero)
            {
                packet = new Packet(packet1);
                return true;
            }

            packet = new Packet();
            return false;
        }

        public void Ping()
        {
            this.ThrowIfNotCreated();
            Native.enet_peer_ping(this.nativePeer);
        }

        public void PingInterval(uint interval)
        {
            this.ThrowIfNotCreated();
            Native.enet_peer_ping_interval(this.nativePeer, interval);
        }

        public void Timeout(uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum)
        {
            this.ThrowIfNotCreated();
            Native.enet_peer_timeout(this.nativePeer, timeoutLimit, timeoutMinimum, timeoutMaximum);
        }

        public void Disconnect(uint data)
        {
            this.ThrowIfNotCreated();
            Native.enet_peer_disconnect(this.nativePeer, data);
        }

        public void DisconnectNow(uint data)
        {
            this.ThrowIfNotCreated();
            Native.enet_peer_disconnect_now(this.nativePeer, data);
        }

        public void DisconnectLater(uint data)
        {
            this.ThrowIfNotCreated();
            Native.enet_peer_disconnect_later(this.nativePeer, data);
        }

        public void Reset()
        {
            this.ThrowIfNotCreated();
            Native.enet_peer_reset(this.nativePeer);
        }
    }
}