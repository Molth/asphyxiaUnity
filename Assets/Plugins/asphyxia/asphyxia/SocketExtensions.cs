//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if NET8_0_OR_GREATER
using System.Net;
using System.Net.Sockets;

namespace asphyxia
{
    /// <summary>
    ///     Socket extensions
    /// </summary>
    public static class SocketExtensions
    {
        /// <summary>
        ///     Sent to
        /// </summary>
        /// <param name="socket">Socket</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="size">Size</param>
        /// <param name="socketFlags">SocketFlags</param>
        /// <param name="socketAddress">SocketAddress</param>
        /// <returns>Sent bytes</returns>
        public static int SendTo(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, SocketAddress socketAddress) => socket.SendTo(new ReadOnlySpan<byte>(buffer, offset, size), socketFlags, socketAddress);

        /// <summary>
        ///     Receive from
        /// </summary>
        /// <param name="socket">Socket</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="socketFlags">SocketFlags</param>
        /// <param name="socketAddress">SocketAddress</param>
        /// <returns>Received bytes</returns>
        public static int ReceiveFrom(this Socket socket, Span<byte> buffer, SocketFlags socketFlags, ref SocketAddress socketAddress) => socket.ReceiveFrom(buffer, socketFlags, socketAddress);
    }
}
#endif