using System;
using System.Text;

namespace ENet
{
    public struct Address
    {
        private ENetAddress nativeAddress;

        internal ENetAddress NativeData
        {
            get => this.nativeAddress;
            set => this.nativeAddress = value;
        }

        internal Address(ENetAddress address) => this.nativeAddress = address;

        public ushort Port
        {
            get => this.nativeAddress.port;
            set => this.nativeAddress.port = value;
        }

        public string GetIP()
        {
            StringBuilder ip = new StringBuilder(1025);
            return Native.enet_address_get_ip(ref this.nativeAddress, ip, (IntPtr)ip.Capacity) != 0 ? string.Empty : ip.ToString();
        }

        public bool SetIP(string ip)
        {
            if (ip == null)
                throw new ArgumentNullException(nameof(ip));
            return Native.enet_address_set_ip(ref this.nativeAddress, ip) == 0;
        }

        public string GetHost()
        {
            StringBuilder hostName = new StringBuilder(1025);
            return Native.enet_address_get_hostname(ref this.nativeAddress, hostName, (IntPtr)hostName.Capacity) != 0 ? string.Empty : hostName.ToString();
        }

        public bool SetHost(string hostName)
        {
            if (hostName == null)
                throw new ArgumentNullException(nameof(hostName));
            return Native.enet_address_set_hostname(ref this.nativeAddress, hostName) == 0;
        }
    }
}