using Jakkes.WebSockets.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var src = new WebSocketServer(8080);
            src.Start();
            Console.ReadLine();
        }
    }
}
