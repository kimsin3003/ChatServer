 using System.Runtime.InteropServices;


namespace ChatServer
{
    struct CFRoomRequestBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        char[] id;
        int roomNo;
    }
}