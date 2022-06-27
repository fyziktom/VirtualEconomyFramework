namespace VEFramework.Demo.PublishingDisplay.Models
{
    public class Profile
    {
        public string Utxo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
    }
}
