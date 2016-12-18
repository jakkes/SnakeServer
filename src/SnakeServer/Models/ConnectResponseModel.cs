namespace SnakeServer.Models
{
    public class ConnectResponseModel : ResponseModel
    {
        public ConnectResponseModel() { Action = "ID"; }
        public string Id { get; set; }
    }
}
