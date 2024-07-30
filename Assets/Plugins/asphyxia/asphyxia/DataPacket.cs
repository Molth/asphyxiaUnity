//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif
using static System.Runtime.InteropServices.Marshal;
using static System.Runtime.CompilerServices.Unsafe;

namespace asphyxia
{
    /// <summary>
    ///     DataPacket
    /// </summary>
    public unsafe struct DataPacket : IDisposable
    {
        /// <summary>
        ///     Data
        /// </summary>
        public byte* Data;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length;

        /// <summary>
        ///     Flags
        /// </summary>
        public PacketFlag Flags;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="length">Length</param>
        /// <param name="flags">Flags</param>
        private DataPacket(byte* data, int length, PacketFlag flags)
        {
            Data = data;
            Length = length;
            Flags = flags;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsSet => Data != null;

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(int length, PacketFlag flags)
        {
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                throw new NullReferenceException("NoAllocate must have data.");
            var data = (byte*)AllocHGlobal(length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(DataPacket src, PacketFlag flags)
        {
            var length = src.Length;
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                return new DataPacket(src.Data, length, flags);
            var data = (byte*)AllocHGlobal(length);
            CopyBlock(data, src.Data, (uint)length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="length">Length</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(DataPacket src, int length, PacketFlag flags)
        {
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                return new DataPacket(src.Data, length, flags);
            var data = (byte*)AllocHGlobal(length);
            CopyBlock(data, src.Data, (uint)length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(DataPacket src, int offset, int length, PacketFlag flags)
        {
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                return new DataPacket(src.Data + offset, length, flags);
            var data = (byte*)AllocHGlobal(length);
            CopyBlock(data, src.Data + offset, (uint)length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="length">Length</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(byte* src, int length, PacketFlag flags)
        {
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                return new DataPacket(src, length, flags);
            var data = (byte*)AllocHGlobal(length);
            CopyBlock(data, src, (uint)length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(byte* src, int offset, int length, PacketFlag flags)
        {
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                return new DataPacket(src + offset, length, flags);
            var data = (byte*)AllocHGlobal(length);
            CopyBlock(data, src + offset, (uint)length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="length">Length</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(nint src, int length, PacketFlag flags)
        {
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                return new DataPacket((byte*)src, length, flags);
            var data = (byte*)AllocHGlobal(length);
            CopyBlock(data, (byte*)src, (uint)length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(nint src, int offset, int length, PacketFlag flags)
        {
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                return new DataPacket((byte*)src + offset, length, flags);
            var data = (byte*)AllocHGlobal(length);
            CopyBlock(data, (byte*)src + offset, (uint)length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(byte[] src, PacketFlag flags)
        {
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                throw new InvalidOperationException("Data must be pinned.");
            var length = src.Length;
            var data = (byte*)AllocHGlobal(length);
            CopyBlock(ref *data, ref src[0], (uint)length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="length">Length</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(byte[] src, int length, PacketFlag flags)
        {
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                throw new InvalidOperationException("Data must be pinned.");
            var data = (byte*)AllocHGlobal(length);
            CopyBlock(ref *data, ref src[0], (uint)length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(byte[] src, int offset, int length, PacketFlag flags)
        {
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                throw new InvalidOperationException("Data must be pinned.");
            var data = (byte*)AllocHGlobal(length);
            CopyBlock(ref *data, ref src[offset], (uint)length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="flags">Flags</param>
        /// <returns>DataPacket</returns>
        public static DataPacket Create(Span<byte> src, PacketFlag flags)
        {
            if (((int)flags & (int)PacketFlag.NoAllocate) != 0)
                throw new InvalidOperationException("Data must be pinned.");
            var length = src.Length;
            var data = (byte*)AllocHGlobal(length);
            CopyBlock(ref *data, ref src[0], (uint)length);
            return new DataPacket(data, length, flags);
        }

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        public void CopyTo(DataPacket dst) => CopyBlock(dst.Data, Data, (uint)Length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="length">Length</param>
        public void CopyTo(DataPacket dst, int length) => CopyBlock(dst.Data, Data, (uint)length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        public void CopyTo(DataPacket dst, int offset, int length) => CopyBlock(dst.Data, Data + offset, (uint)length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        public void CopyTo(byte* dst) => CopyBlock(dst, Data, (uint)Length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="length">Length</param>
        public void CopyTo(byte* dst, int length) => CopyBlock(dst, Data, (uint)length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        public void CopyTo(byte* dst, int offset, int length) => CopyBlock(dst, Data + offset, (uint)length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        public void CopyTo(nint dst) => CopyBlock((byte*)dst, Data, (uint)Length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="length">Length</param>
        public void CopyTo(nint dst, int length) => CopyBlock((byte*)dst, Data, (uint)length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        public void CopyTo(nint dst, int offset, int length) => CopyBlock((byte*)dst, Data + offset, (uint)length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        public void CopyTo(byte[] dst) => CopyBlock(ref dst[0], ref *Data, (uint)Length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="length">Length</param>
        public void CopyTo(byte[] dst, int length) => CopyBlock(ref dst[0], ref *Data, (uint)length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        public void CopyTo(byte[] dst, int offset, int length) => CopyBlock(ref dst[0], ref *(Data + offset), (uint)length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        public void CopyTo(Span<byte> dst) => CopyBlock(ref dst[0], ref *Data, (uint)Length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="length">Length</param>
        public void CopyTo(Span<byte> dst, int length) => CopyBlock(ref dst[0], ref *Data, (uint)length);

        /// <summary>
        ///     CopyTo
        /// </summary>
        /// <param name="dst">Destination</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        public void CopyTo(Span<byte> dst, int offset, int length) => CopyBlock(ref dst[0], ref *(Data + offset), (uint)length);

        /// <summary>
        ///     AsSpan
        /// </summary>
        /// <returns>Span</returns>
        public Span<byte> AsSpan() => new(Data, Length);

        /// <summary>
        ///     AsSpan
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Span</returns>
        public Span<byte> AsSpan(int offset) => new(Data + offset, Length);

        /// <summary>
        ///     AsSpan
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        public Span<byte> AsSpan(int offset, int length) => new(Data + offset, length);

        /// <summary>
        ///     AsReadOnlySpan
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        public ReadOnlySpan<byte> AsReadOnlySpan() => new(Data, Length);

        /// <summary>
        ///     AsReadOnlySpan
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>ReadOnlySpan</returns>
        public ReadOnlySpan<byte> AsReadOnlySpan(int offset) => new(Data + offset, Length);

        /// <summary>
        ///     AsReadOnlySpan
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        public ReadOnlySpan<byte> AsReadOnlySpan(int offset, int length) => new(Data + offset, length);

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            if (((int)Flags & (int)PacketFlag.NoAllocate) != 0 || Data == null)
                return;
            FreeHGlobal((nint)Data);
            Data = null;
        }
    }
}