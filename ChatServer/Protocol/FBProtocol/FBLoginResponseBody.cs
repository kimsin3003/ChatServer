
using System.Runtime.InteropServices;

namespace ChatServer
{
    struct FBLoginResponseBody
    {
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 12)]
        public char[] id;
    }
}