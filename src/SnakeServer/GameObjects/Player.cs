using Jakkes.WebSockets.Server;
using System.Collections.Generic;
using System;
using System.Threading;

namespace SnakeServer.GameObjects
{
    public class Player
    {
        private const int _moveRate = 60;
        private const int _moveLength = 10;

        private Connection _conn;
        private List<Node> _nodes;
        private Timer _moveTimer;
        private int _length = 6;

        public EventHandler Died;
        public string ID { get { return _conn.ID; } }
        public double Heading { get; private set; }
        public Node Head { get { return _nodes[0]; } }

        public Player(Connection conn)
        {
            _conn = conn;
            _conn.MessageReceived += _conn_MessageReceived;
            _conn.Closed += _conn_Closed;
        }
        public void Start(Node start, double heading)
        {
            Heading = heading;
            _nodes = new List<Node>();
            _nodes.Add(start);
            _moveTimer = new Timer(new TimerCallback(Move), null, 0, 1000 / 60);
        }
        private void Move(object state)
        {
            _nodes.Insert(0, new Node(Head.X + _moveLength * Math.Cos(Heading), Head.Y + _moveLength * Math.Sin(Heading)));
            if (_nodes.Count > _length)
                _nodes.RemoveRange(_length - 1, _nodes.Count - _length);
        }
        private void _conn_Closed(Connection source)
        {
            Died?.Invoke(this, null);
        }
        private void _conn_MessageReceived(Connection source, string data)
        {
            throw new System.NotImplementedException();
        }
    }
}
