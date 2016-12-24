using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeServer.GameObjects
{
    public abstract class DespawningNode : Node
    {
        public event EventHandler Despawned;

        private Timer _despawnTimer;

        public DespawningNode(Node node, int despawnTime) : this(node.X, node.Y, despawnTime) { }
        public DespawningNode(double x, double y, int despawnTime)
            : base(x,y)
        {
            _despawnTimer = new Timer(new TimerCallback(_despawn), null, despawnTime, Timeout.Infinite);
        }
        private void _despawn(object state)
        {
            Despawn();
        }
        public void Despawn()
        {
            _despawnTimer?.Dispose();
            Despawned?.Invoke(this, null);
        }
    }
}
