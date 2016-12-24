using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeServer.Models
{
    public class SnakeDataModel : ResponseModel
    {
        public List<SnakeModel> Snakes { get; set; }
        public SnakeDataModel() { Action = "SnakeData"; }
    }
}
