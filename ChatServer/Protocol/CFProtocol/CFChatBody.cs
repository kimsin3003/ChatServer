// chat content -> byte[] not using struct 
using System.Runtime.InteropServices;

namespace ChatServer
{
    struct CFChatBody
    {
        public byte[] data;// data can be : chat Room List, FE IP-PORT, chat room no 
    }
}