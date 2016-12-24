using System;

namespace SnakeServer.GameObjects
{
    public class Node
    {
        public double X { get; set; }
        public double Y { get; set; }
        
        public Node(int x, int y)
        {
            X = x; Y = y;
        }
        public Node(double x, double y)
        {
            X = x; Y = y;
        }
        public double DistanceTo(Node node)
        {
            return Math.Sqrt(Math.Pow(X - node.X, 2) + Math.Pow(Y - node.Y, 2));
        }
    }
}
