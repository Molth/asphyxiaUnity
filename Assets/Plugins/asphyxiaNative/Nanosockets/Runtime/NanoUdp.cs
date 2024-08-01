//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

#pragma warning disable CA1401
#pragma warning disable CA2101
#pragma warning disable SYSLIB1054

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace NanoSockets
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class NanoUdp
    {
#if __IOS__ || UNITY_IOS && !UNITY_EDITOR
        private const string NativeLibrary = "__Internal";
#else
        private const string NativeLibrary = "nanosockets";
#endif

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Initialize();

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_deinitialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Deinitialize();

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_create", CallingConvention = CallingConvention.Cdecl)]
        public static extern long Create(int sendBufferSize, int receiveBufferSize);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Destroy(ref long socket);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_bind", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Bind(long socket, IntPtr address);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_bind", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Bind(long socket, ref NanoIPEndPoint address);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_connect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Connect(long socket, ref NanoIPEndPoint address);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_set_option", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetOption(long socket, SocketOptionLevel level, SocketOptionName optionName, ref int optionValue, int optionLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_get_option", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetOption(long socket, SocketOptionLevel level, SocketOptionName optionName, ref int optionValue, ref int optionLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_set_nonblocking", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetNonBlocking(long socket, byte nonBlocking);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_set_dontfragment", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetDontFragment(long socket);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(long socket, IntPtr address, IntPtr buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(long socket, IntPtr address, byte[] buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(long socket, ref NanoIPEndPoint address, IntPtr buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(long socket, ref NanoIPEndPoint address, byte[] buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(long socket, ref NanoIPEndPoint address, byte* buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(long socket, NanoIPEndPoint* address, byte[] buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(long socket, NanoIPEndPoint* address, byte* buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(long socket, IntPtr address, IntPtr buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(long socket, IntPtr address, byte[] buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(long socket, ref NanoIPEndPoint address, IntPtr buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(long socket, ref NanoIPEndPoint address, byte[] buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(long socket, ref NanoIPEndPoint address, byte* buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(long socket, NanoIPEndPoint* address, byte[] buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(long socket, NanoIPEndPoint* address, byte* buffer, int bufferLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_poll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Poll(long socket, long timeout);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_get", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetAddress(long socket, ref NanoIPEndPoint address);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_is_equal", CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsEqual(ref NanoIPEndPoint left, ref NanoIPEndPoint right);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_set_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetIP(ref NanoIPEndPoint address, IntPtr ip);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_set_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetIP(ref NanoIPEndPoint address, string ip);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_set_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetIP(ref NanoIPEndPoint address, char* ip);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_get_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetIP(ref NanoIPEndPoint address, IntPtr stringBuffer, int ipLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_get_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetIP(ref NanoIPEndPoint address, byte* stringBuffer, int ipLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_get_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetIP(ref NanoIPEndPoint address, StringBuilder stringBuilder, int ipLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_set_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetHostName(ref NanoIPEndPoint address, IntPtr name);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_set_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetHostName(ref NanoIPEndPoint address, string name);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_get_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetHostName(ref NanoIPEndPoint address, IntPtr name, int nameLength);

        [DllImport(NativeLibrary, EntryPoint = "nanosockets_address_get_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetHostName(ref NanoIPEndPoint address, StringBuilder name, int nameLength);
    }
}