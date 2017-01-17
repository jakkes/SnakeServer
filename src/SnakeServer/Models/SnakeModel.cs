using SnakeServer.GameObjects;

namespace SnakeServer.Models
{
    public class SnakeModel
    {
        public Node[] Nodes { get; set; }
        public double Heading { get; set; }
        public int Length { get; set; }
        public string ID { get; set; }
    }
}
