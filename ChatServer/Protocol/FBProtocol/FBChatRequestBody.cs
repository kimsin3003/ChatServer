
using System.Runtime.InteropServices;

namespace ChatServer
{
    struct FBChatRequestBody
    {
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 12)]
        public char[] id;
    }
}
