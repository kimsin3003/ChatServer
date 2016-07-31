// chat content -> byte[] not using struct 
using System.Runtime.InteropServices;

namespace ChatServer
{
    struct CFChatBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public char[] id;
        public byte[] data;// data can be : chat Room List, FE IP-PORT, chat room no 
    }
}