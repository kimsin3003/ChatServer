
using System.Runtime.InteropServices;

namespace ChatServer
{
    struct FBLoginResponseBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] id;
    }
}