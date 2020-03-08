using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HamPig.Network;

namespace MainServer
{
    public class GameServer
    {
        public bool isShutdown { get; private set; }

        public void Start()
        {
            isShutdown = false;
        }

        public void Update()
        {
            Console.ReadKey();
            isShutdown = true;
        }

        public void Shutdown()
        {

        }
    }
}
