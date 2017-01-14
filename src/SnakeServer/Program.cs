using System;

namespace SnakeServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var src = new Server(8080);
            src.Start();
            Console.WriteLine("Started server on port 8080");
            Console.ReadLine();
            src.Close(true);
        }
    }
}
