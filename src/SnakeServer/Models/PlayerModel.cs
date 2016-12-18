using SnakeServer.GameObjects;

namespace SnakeServer.Models
{
    public class PlayerModel : ResponseModel
    {
        public Node[] Nodes { get; set; }
        public PlayerModel() { Action = "PlayerData"; }
    }
}
