using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoClient
{
    class ClientMain
    {
        public static void Main(string[] args)
        {

            Client client = new Client("localhost", 11000);
            client.Start();
            client.ShutDown();
        }
    }
}
