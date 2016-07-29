// chat content -> byte[] not using struct 
namespace ChatServer
{
    struct CFChatBody
    {
        byte[] data;// data can be : chat Room List, FE IP-PORT, chat room no 
    }
}