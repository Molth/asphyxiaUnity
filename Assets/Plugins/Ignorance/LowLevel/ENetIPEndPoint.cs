//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif
using System.Runtime.CompilerServices;
using System.Text;

#pragma warning disable CS8600
#pragma warning disable CS8604
#pragma warning disable CS9074

// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable RedundantAssignment

namespace ENet
{
    /// <summary>
    ///     EndPoint
    /// </summary>
    public unsafe struct ENetIPEndPoint
    {
        /// <summary>
        ///     Address
        /// </summary>
        public string Address;

        /// <summary>
        ///     Port
        /// </summary>
        public ushort Port;

        /// <summary>
        ///     Write bytes
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="bytesWritten">BytesWritten</param>
        public void WriteBytes(Span<byte> destination, out int bytesWritten)
        {
            var bytes = Encoding.ASCII.GetBytes(Address, destination);
            Unsafe.WriteUnaligned(ref destination[bytes], Port);
            bytesWritten = bytes + 4;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="peer">Peer</param>
        public ENetIPEndPoint(Peer peer)
        {
            Address = peer.IP;
            Port = peer.Port;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="packet">Packet</param>
        public ENetIPEndPoint(Packet packet)
        {
            var data = (byte*)packet.Data;
            var length = packet.Length;
            Address = Encoding.ASCII.GetString(data, length - 4);
            Port = Unsafe.ReadUnaligned<ushort>(data + length - 4);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="packet">Packet</param>
        /// <param name="offset">Offset</param>
        public ENetIPEndPoint(Packet packet, int offset)
        {
            var data = (byte*)packet.Data + offset;
            var length = packet.Length - offset;
            Address = Encoding.ASCII.GetString(data, length - 4);
            Port = Unsafe.ReadUnaligned<ushort>(data + length - 4);
        }

        /// <summary>
        ///     Create address
        /// </summary>
        /// <returns>Address</returns>
        public Address CreateAddress()
        {
            var address = new Address();
            address.SetHost(Address);
            address.Port = Port;
            return address;
        }

        /// <summary>
        ///     Returns the hash code for this instance
        /// </summary>
        /// <returns>A hash code for the current</returns>
        public override int GetHashCode() => Address.GetHashCode() ^ Port;

        /// <summary>
        ///     Converts the value of this instance to its equivalent string representation
        /// </summary>
        /// <returns>Represents the NetworkConnection value as a string</returns>
        public override string ToString() => Address + ":" + Port;
    }
}