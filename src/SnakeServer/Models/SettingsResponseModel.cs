namespace SnakeServer.Models
{
    public class SettingsResponseModel : ResponseModel
    {
        public SettingsResponseModel() { Action = "Settings"; }
        public Constants Settings { get; set; }
    }
}
