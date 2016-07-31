using System.Runtime.InteropServices;

namespace ChatServer
{
    struct CFLoginRequestBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public char[] id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public char[] password;
    }
}