using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeServer
{
    public static class Config
    {
        public static Constants Constants;
        public static int PlayerCount { get { return Constants.PlayerCount; } }
        public static double Width { get { return Constants.Width; } }
        public static double Height { get { return Constants.Height; } }
        public static double SnakeMovementLength { get { return Constants.SnakeMovementLength; } }
        public static double SnakeRadius { get { return Constants.SnakeRadius; } }
        public static int SnakeStartLength { get { return 60; } }
        public static double SnakeTurnRate { get { return 60; } }
        public static double SnakeTurnLength { get { return 0.05; } }
        public static int SnakeMovementRate { get { return Constants.SnakeMovementRate; } }
        public static int AppleDespawnTime { get { return 30; } }
        public static int AppleGrowLength { get { return 30; } }
        public static int AppleSpawnTime { get { return 10; } }
        public static double AppleRadius { get { return Constants.AppleRadius; } }
        static Config()
        {
            Constants = new Constants();
        }
    }
    public class Constants
    {
        public double Width = 1280;
        public double Height;
        public double SnakeMovementLength = 3;
        public int SnakeMovementRate = 60;
        public double SnakeRadius = 1;
        public int PlayerCount = 4;
        public double AppleRadius = 5;

        public Constants() { Height = Width * 9 / 16; }
    }
}
