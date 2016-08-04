using System;
using System.Runtime.InteropServices;

namespace ChatServer
{
    struct CFChatResponseBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] id;
        public DateTime date;
        public int msgLen; //lenght of next body 
    }
}
