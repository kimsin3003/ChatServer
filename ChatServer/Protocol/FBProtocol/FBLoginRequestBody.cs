using System.Runtime.InteropServices;

namespace ChatServer
{
    struct FBLoginRequestBody
    {
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 12)]
        public char[] id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] password;
        public bool isDummy;
    }
}