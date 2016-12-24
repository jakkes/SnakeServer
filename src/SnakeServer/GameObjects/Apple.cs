namespace SnakeServer.GameObjects
{
    public class Apple : DespawningNode
    {
        public Apple(Node node) : base(node, Config.AppleDespawnTime * 1000) { }
        public Apple(double x, double y) : base(x, y, Config.AppleDespawnTime * 1000) { }
    }
}
