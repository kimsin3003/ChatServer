using System.Runtime.InteropServices;

namespace ChatServer
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct FBHeader
    {
        public FBMessageType type;
        public FBMessageState state;
        public int length;
        public int sessionId;
    }

    enum FBMessageType : short
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

        Chat_Count = 410,

        Health_Check = 510,

        Connection_Info = 610
    };

    enum FBMessageState : short
    {
        Request = 100,
        Success = 200,
        Fail = 400
    }

}
