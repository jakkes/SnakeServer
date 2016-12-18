using SnakeServer.GameObjects;

namespace SnakeServer.Models
{
    public class SnakeModel : ResponseModel
    {

        public Node[] Nodes { get; set; }
        public double Heading { get; set; }
        public SnakeModel() { Action = "SnakeData"; }
    }
}
