 using System.Runtime.InteropServices;


namespace ChatServer
{
    struct CFRoomRequestBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public char[] id;
        public int roomNo;
    }
}