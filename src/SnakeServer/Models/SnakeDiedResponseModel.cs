namespace SnakeServer.Models
{
    public class SnakeDiedResponseModel : ResponseModel
    {
        public string ID { get; set; }
        public SnakeDiedResponseModel(string ID)
        {
            Action = "SnakeDied";
            this.ID = ID;
        }
    }
}
