using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace DemoClient
{
    class Client
    {
        private IPHostEntry m_ipHost;
        private IPAddress m_ipAddr;
        private IPEndPoint m_ipEndPoint;
        private Socket m_sock;

        public Client(string serverHostName, int port)
        {
            m_ipHost = Dns.GetHostEntry(serverHostName);
            m_ipAddr = m_ipHost.AddressList[1];     //In index 0, there's ipv6 address.
            foreach(var el in m_ipHost.AddressList)
                Console.WriteLine(el.ToString());
            m_ipEndPoint = new IPEndPoint(m_ipAddr, port);
        }

        /**make me XML**/
        public bool Connect()
        {
            m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                m_sock.Connect(m_ipEndPoint);
            }
            catch (SocketException)
            {
                Console.WriteLine("Server is Off");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            Console.WriteLine("Connected to server");
            return true;
        }

        

        private bool Send()
        {
            string bodyData = "Fuck";
            byte[] body = Serializer.StringToBytes(bodyData);

            Header tmp = new Header();
            tmp.type = MessageType._CHAT_MSG;
            tmp.length = body.Length;
            
            byte[] header = Serializer.StructureToByte(tmp);
            try
            {
                m_sock.Send(header);
                m_sock.Send(body);
            }
            catch (SocketException)
            {
                Console.WriteLine("Connection is closed");
                return false;
            }
            return true;
        }

        

        public void Start()
        {
            while(true)
            {
                if (!Connect())
                {
                    Thread.Sleep(1000);
                    continue;
                }

                while (true)
                {
                    Thread.Sleep(1000);
                    if (!Send())
                        break;
                }
            }
        }

        public void ShutDown()
        {
            m_sock.Shutdown(SocketShutdown.Both);
            m_sock.Close();
        }

    }
}
