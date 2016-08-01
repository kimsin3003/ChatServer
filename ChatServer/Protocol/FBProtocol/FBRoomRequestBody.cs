
using System.Runtime.InteropServices;

namespace ChatServer
{
    struct FBRoomRequestBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] id;
        public int roomNo;
    }
}