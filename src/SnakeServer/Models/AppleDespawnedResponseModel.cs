namespace SnakeServer.Models
{
    public class AppleDespawnedResponseModel : ResponseModel
    {
        public string ID { get; set; }
        public AppleDespawnedResponseModel(string ID)
        {
            Action = "AppleDespawned";
            this.ID = ID;
        }
    }
}
