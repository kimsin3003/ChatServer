 using System.Runtime.InteropServices;


namespace ChatServer
{
    struct CFRoomRequestBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] id;
        public int roomNo;
    }
}