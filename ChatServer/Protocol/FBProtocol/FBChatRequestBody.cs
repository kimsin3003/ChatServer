
using System.Runtime.InteropServices;

namespace ChatServer
{
    struct FBChatRequestBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] id;
    }
}
