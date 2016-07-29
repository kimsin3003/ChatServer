
using System.Runtime.InteropServices;

namespace ChatServer
{
    struct FBChatRequestBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        char[] id;
    }
}
