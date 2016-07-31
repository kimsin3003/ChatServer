using System.Runtime.InteropServices;

namespace ChatServer
{
    struct CFLoginRequestBody
    {
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 12)]
        public char[] id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] password;
    }
}