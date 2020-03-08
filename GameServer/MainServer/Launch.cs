using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainServer
{
    class Launch
    {
        static void Main(string[] args)
        {
            GameServer server = new GameServer();
            server.Start();
            while (!server.isShutdown)
            {
                server.Update();
            }
            server.Shutdown();
        }
    }
}
