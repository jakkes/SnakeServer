using System;

namespace SnakeServer.GameObjects
{
    public class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        
        public Node(int x, int y)
        {
            X = x; Y = y;
        }
        public Node(double x, double y)
        {
            X = (int)x; Y = (int)y;
        }
        public int DistanceTo(Node node)
        {
            return (int)(Math.Sqrt(Math.Pow(X - node.X, 2) + Math.Pow(Y - node.Y, 2)));
        }
    }
}
