using SnakeServer.GameObjects;

namespace SnakeServer.Models
{
    public class AppleSpawnedResponseModel : ResponseModel
    {
        public Apple Apple { get; set; }
        public AppleSpawnedResponseModel(Apple apple)
        {
            Apple = apple;
            Action = "AppleSpawned";
        }
    }
}
