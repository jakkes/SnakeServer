using SnakeServer.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeServer.Models
{
    public class GameDataModel : ResponseModel
    {
        public List<SnakeModel> Snakes { get; set; }
        public List<Node> Apples { get; set; }
        public GameDataModel() { Action = "GameData"; }
    }
}
