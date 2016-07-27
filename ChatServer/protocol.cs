using System;
using System.Runtime.InteropServices;


namespace ChatServer
{

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct Header
    {
        public MessageType type;
        public int length;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct Packet
    {
        public Header header;
        public byte[] body;

    }

    enum MessageType : short
    {
        _REQUEST_SIGNUP = 100,
        _REQUEST_CHECK_SIGNEDUP = 110,

        _CHAT_MSG = 200,

        _REQUEST_CREATE_ROOM = 310,
        _REQUEST_LEAVE_ROOM = 320,
        _REQUEST_JOIN_ROOM = 330,
        _REQUEST_LIST_ROOM = 340,

        _STATUS_SUCCESS = 200,
        _STATUS_FAIL = 400
    };


}
