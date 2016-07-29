using System.Runtime.InteropServices;

namespace ChatServer
{
    struct LoginRequestBody
    {
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 12)]
        char[] id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        char[] password;
        bool isDummy;
    }
}