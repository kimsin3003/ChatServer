using System.Runtime.InteropServices;

namespace ChatServer
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct CFHeader
    {
        public CFMessageType type;
        public CFMessageState state;
        public int length;
    }

    enum CFMessageType : short
    {
        Id_Dup = 110,
        Signup = 120,

        Login = 210,
        LogOut = 220,

        Room_Create = 310,
        Room_Leave = 320,
        Room_Join = 330,
        Room_List = 340,
        Room_Delete = 350,

        Chat_MSG_From_Client = 410,
        Chat_MSG_Broadcast = 420,

        Health_Check = 510
    };

    enum CFMessageState : short
    {
        Request = 100,
        Success = 200,
        Fail = 400
    }
}
