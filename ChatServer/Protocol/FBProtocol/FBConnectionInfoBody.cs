using System.Runtime.InteropServices;

namespace ChatServer
{
    struct FBConnectionInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ip;
        public int port;
    }
}
