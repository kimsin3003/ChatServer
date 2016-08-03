namespace ChatServer
{
    using System.Runtime.InteropServices;

    struct CFJoinFailBody
    {
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 15)]
        public char[] ip;
        public int port;
    }
}
