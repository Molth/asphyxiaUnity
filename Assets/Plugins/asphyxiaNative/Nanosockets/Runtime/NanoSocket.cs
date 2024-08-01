//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif
using System.Net.Sockets;

#pragma warning disable CA1816
#pragma warning disable CS8602

// ReSharper disable PossibleNullReferenceException

namespace NanoSockets
{
    /// <summary>
    ///     Socket
    /// </summary>
    public sealed unsafe class NanoSocket : IDisposable
    {
        /// <summary>
        ///     Lock
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        ///     Handle
        /// </summary>
        private long _handle;

        /// <summary>
        ///     Blocking
        /// </summary>
        private bool _willBlock = true;

        /// <summary>
        ///     Handle
        /// </summary>
        public long Handle => _handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsSet => _handle > 0L;

        /// <summary>
        ///     Blocking
        /// </summary>
        public bool Blocking
        {
            get => _willBlock;
            set
            {
                if (NanoUdp.SetNonBlocking(_handle, value ? (byte)1 : (byte)0) != 0)
                    return;
                _willBlock = value;
            }
        }

        /// <summary>
        ///     DontFragment
        /// </summary>
        public bool DontFragment
        {
            get
            {
                var dontFragment = 0;
                var length = 4;
                _ = NanoUdp.GetOption(_handle, SocketOptionLevel.IP, SocketOptionName.DontFragment, ref dontFragment, ref length);
                return dontFragment != 0;
            }
            set
            {
                var dontFragment = value ? 1 : 0;
                _ = NanoUdp.SetOption(_handle, SocketOptionLevel.IP, SocketOptionName.DontFragment, ref dontFragment, 1);
            }
        }

        /// <summary>
        ///     Send buffer size
        /// </summary>
        public int SendBufferSize
        {
            get
            {
                var sendBufferSize = 0;
                var length = 4;
                _ = NanoUdp.GetOption(_handle, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, ref sendBufferSize, ref length);
                return sendBufferSize;
            }
            set
            {
                var sendBufferSize = value;
                _ = NanoUdp.SetOption(_handle, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, ref sendBufferSize, 4);
            }
        }

        /// <summary>
        ///     Receive buffer size
        /// </summary>
        public int ReceiveBufferSize
        {
            get
            {
                var receiveBufferSize = 0;
                var length = 4;
                _ = NanoUdp.GetOption(_handle, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, ref receiveBufferSize, ref length);
                return receiveBufferSize;
            }
            set
            {
                var receiveBufferSize = value;
                _ = NanoUdp.SetOption(_handle, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, ref receiveBufferSize, 4);
            }
        }

        /// <summary>
        ///     LocalEndPoint
        /// </summary>
        public NanoIPEndPoint LocalEndPoint
        {
            get
            {
                var ipEndPoint = new NanoIPEndPoint();
                _ = NanoUdp.GetAddress(_handle, ref ipEndPoint);
                return ipEndPoint;
            }
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose() => Close();

        /// <summary>
        ///     Destructure
        /// </summary>
        ~NanoSocket() => Close();

        /// <summary>
        ///     Create
        /// </summary>
        public bool Create() => Create(1024, 1024);

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="sendBufferSize">SendBuffer size</param>
        /// <param name="receiveBufferSize">ReceiveBuffer size</param>
        public bool Create(int sendBufferSize, int receiveBufferSize)
        {
            lock (_lock)
            {
                if (IsSet || NanoUdp.Initialize() != 0)
                    return false;
                _handle = NanoUdp.Create(sendBufferSize, receiveBufferSize);
                return true;
            }
        }

        /// <summary>
        ///     Destroy
        /// </summary>
        public bool Close()
        {
            lock (_lock)
            {
                if (!IsSet)
                    return false;
                NanoUdp.Destroy(ref _handle);
                NanoUdp.Deinitialize();
                return true;
            }
        }

        /// <summary>
        ///     Bind
        /// </summary>
        /// <param name="localEndPoint"></param>
        /// <returns>Local endPoint</returns>
        public bool Bind(ref NanoIPEndPoint localEndPoint) => NanoUdp.Bind(_handle, ref localEndPoint) == 0;

        /// <summary>
        ///     Connect
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <returns>EndPoint</returns>
        public bool Connect(ref NanoIPEndPoint remoteEndPoint) => NanoUdp.Connect(_handle, ref remoteEndPoint) == 0;

        /// <summary>
        ///     Poll
        /// </summary>
        /// <returns>Polled</returns>
        public bool Poll() => NanoUdp.Poll(_handle, 0L) > 0;

        /// <summary>
        ///     Poll
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>Polled</returns>
        public bool Poll(long timeout) => NanoUdp.Poll(_handle, timeout) > 0;

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(byte[] buffer, ref NanoIPEndPoint remoteEndPoint) => NanoUdp.Send(_handle, ref remoteEndPoint, buffer, buffer.Length);

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(byte[] buffer, int size, ref NanoIPEndPoint remoteEndPoint) => NanoUdp.Send(_handle, ref remoteEndPoint, buffer, size);

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(byte[] buffer, int offset, int size, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* src = &buffer[offset])
                return NanoUdp.Send(_handle, ref remoteEndPoint, src, size);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(ReadOnlySpan<byte> buffer, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* src = &buffer[0])
                return NanoUdp.Send(_handle, ref remoteEndPoint, src, buffer.Length);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(ReadOnlyMemory<byte> buffer, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* src = &buffer.Span[0])
                return NanoUdp.Send(_handle, ref remoteEndPoint, src, buffer.Length);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(ArraySegment<byte> buffer, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* src = &buffer.Array[buffer.Offset])
                return NanoUdp.Send(_handle, ref remoteEndPoint, src, buffer.Count);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(byte* buffer, int size, ref NanoIPEndPoint remoteEndPoint) => NanoUdp.Send(_handle, ref remoteEndPoint, buffer, size);

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(byte[] buffer, out int count, ref NanoIPEndPoint remoteEndPoint)
        {
            count = NanoUdp.Receive(_handle, ref remoteEndPoint, buffer, buffer.Length);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(byte[] buffer, int size, out int count, ref NanoIPEndPoint remoteEndPoint)
        {
            count = NanoUdp.Receive(_handle, ref remoteEndPoint, buffer, size);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="size">Size</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(byte[] buffer, int offset, int size, out int count, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* dest = &buffer[offset])
                count = NanoUdp.Receive(_handle, ref remoteEndPoint, dest, size);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(Span<byte> buffer, out int count, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* dest = &buffer[0])
                count = NanoUdp.Receive(_handle, ref remoteEndPoint, dest, buffer.Length);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(Memory<byte> buffer, out int count, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* dest = &buffer.Span[0])
                count = NanoUdp.Receive(_handle, ref remoteEndPoint, dest, buffer.Length);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(ArraySegment<byte> buffer, out int count, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* dest = &buffer.Array[buffer.Offset])
                count = NanoUdp.Receive(_handle, ref remoteEndPoint, dest, buffer.Count);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(byte* buffer, int size, out int count, ref NanoIPEndPoint remoteEndPoint)
        {
            count = NanoUdp.Receive(_handle, ref remoteEndPoint, buffer, size);
            return count > 0;
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(byte[] buffer, NanoIPEndPoint* remoteEndPoint) => NanoUdp.Send(_handle, remoteEndPoint, buffer, buffer.Length);

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(byte[] buffer, int size, NanoIPEndPoint* remoteEndPoint) => NanoUdp.Send(_handle, remoteEndPoint, buffer, size);

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(byte[] buffer, int offset, int size, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* src = &buffer[offset])
                return NanoUdp.Send(_handle, remoteEndPoint, src, size);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(ReadOnlySpan<byte> buffer, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* src = &buffer[0])
                return NanoUdp.Send(_handle, remoteEndPoint, src, buffer.Length);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(ReadOnlyMemory<byte> buffer, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* src = &buffer.Span[0])
                return NanoUdp.Send(_handle, remoteEndPoint, src, buffer.Length);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(ArraySegment<byte> buffer, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* src = &buffer.Array[buffer.Offset])
                return NanoUdp.Send(_handle, remoteEndPoint, src, buffer.Count);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        public int Send(byte* buffer, int size, NanoIPEndPoint* remoteEndPoint) => NanoUdp.Send(_handle, remoteEndPoint, buffer, size);

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(byte[] buffer, out int count, NanoIPEndPoint* remoteEndPoint)
        {
            count = NanoUdp.Receive(_handle, remoteEndPoint, buffer, buffer.Length);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(byte[] buffer, int size, out int count, NanoIPEndPoint* remoteEndPoint)
        {
            count = NanoUdp.Receive(_handle, remoteEndPoint, buffer, size);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="size">Size</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(byte[] buffer, int offset, int size, out int count, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* dest = &buffer[offset])
                count = NanoUdp.Receive(_handle, remoteEndPoint, dest, size);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(Span<byte> buffer, out int count, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* dest = &buffer[0])
                count = NanoUdp.Receive(_handle, remoteEndPoint, dest, buffer.Length);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(Memory<byte> buffer, out int count, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* dest = &buffer.Span[0])
                count = NanoUdp.Receive(_handle, remoteEndPoint, dest, buffer.Length);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(ArraySegment<byte> buffer, out int count, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* dest = &buffer.Array[buffer.Offset])
                count = NanoUdp.Receive(_handle, remoteEndPoint, dest, buffer.Count);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="count">Count</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public bool Receive(byte* buffer, int size, out int count, NanoIPEndPoint* remoteEndPoint)
        {
            count = NanoUdp.Receive(_handle, remoteEndPoint, buffer, size);
            return count > 0;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(byte[] buffer, NanoIPEndPoint* remoteEndPoint) => NanoUdp.Receive(_handle, remoteEndPoint, buffer, buffer.Length);

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(byte[] buffer, int size, NanoIPEndPoint* remoteEndPoint) => NanoUdp.Receive(_handle, remoteEndPoint, buffer, size);

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(byte[] buffer, int offset, int size, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* dest = &buffer[offset])
                return NanoUdp.Receive(_handle, remoteEndPoint, dest, size);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(Span<byte> buffer, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* dest = &buffer[0])
                return NanoUdp.Receive(_handle, remoteEndPoint, dest, buffer.Length);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(Memory<byte> buffer, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* dest = &buffer.Span[0])
                return NanoUdp.Receive(_handle, remoteEndPoint, dest, buffer.Length);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(ArraySegment<byte> buffer, NanoIPEndPoint* remoteEndPoint)
        {
            fixed (byte* dest = &buffer.Array[buffer.Offset])
                return NanoUdp.Receive(_handle, remoteEndPoint, dest, buffer.Count);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(byte* buffer, int size, NanoIPEndPoint* remoteEndPoint) => NanoUdp.Receive(_handle, remoteEndPoint, buffer, size);

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(byte[] buffer, ref NanoIPEndPoint remoteEndPoint) => NanoUdp.Receive(_handle, ref remoteEndPoint, buffer, buffer.Length);

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(byte[] buffer, int size, ref NanoIPEndPoint remoteEndPoint) => NanoUdp.Receive(_handle, ref remoteEndPoint, buffer, size);

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(byte[] buffer, int offset, int size, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* dest = &buffer[offset])
                return NanoUdp.Receive(_handle, ref remoteEndPoint, dest, size);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(Span<byte> buffer, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* dest = &buffer[0])
                return NanoUdp.Receive(_handle, ref remoteEndPoint, dest, buffer.Length);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(Memory<byte> buffer, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* dest = &buffer.Span[0])
                return NanoUdp.Receive(_handle, ref remoteEndPoint, dest, buffer.Length);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(ArraySegment<byte> buffer, ref NanoIPEndPoint remoteEndPoint)
        {
            fixed (byte* dest = &buffer.Array[buffer.Offset])
                return NanoUdp.Receive(_handle, ref remoteEndPoint, dest, buffer.Count);
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size</param>
        /// <param name="remoteEndPoint">EndPoint</param>
        /// <returns>Received</returns>
        public int Receive(byte* buffer, int size, ref NanoIPEndPoint remoteEndPoint) => NanoUdp.Receive(_handle, ref remoteEndPoint, buffer, size);

        /// <summary>
        ///     Set option
        /// </summary>
        /// <param name="level">SocketOptionLevel</param>
        /// <param name="optionName">SocketOptionName</param>
        /// <param name="optionValue">OptionValue</param>
        /// <param name="optionLength">OptionLength</param>
        /// <returns>Set</returns>
        public bool SetOption(SocketOptionLevel level, SocketOptionName optionName, ref int optionValue, int optionLength) => NanoUdp.SetOption(_handle, level, optionName, ref optionValue, optionLength) == 0;

        /// <summary>
        ///     Get option
        /// </summary>
        /// <param name="level">SocketOptionLevel</param>
        /// <param name="optionName">SocketOptionName</param>
        /// <param name="optionValue">OptionValue</param>
        /// <param name="optionLength">OptionLength</param>
        /// <returns>Got</returns>
        public bool GetOption(SocketOptionLevel level, SocketOptionName optionName, ref int optionValue, ref int optionLength) => NanoUdp.GetOption(_handle, level, optionName, ref optionValue, ref optionLength) == 0;

        /// <summary>
        ///     Implicitly converts a Socket to a long value
        /// </summary>
        /// <param name="socket">The Socket to convert</param>
        /// <returns>The long value of the Socket</returns>
        public static implicit operator long(NanoSocket socket) => socket._handle;
    }
}