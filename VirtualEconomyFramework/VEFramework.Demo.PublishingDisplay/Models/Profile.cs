namespace VEFramework.Demo.PublishingDisplay.Models
{
    public class Profile
    {
        public string Utxo { get; set; } = "No Utxo";
        public string Name { get; set; } = "No Title";
        public string Position { get; set; } = "No Title";
        public string Image { get; set; } = "No Image";
        public string Text { get; set; } = "No Text";
        public List<string> Tags { get; set; } = new List<string>();
    }
}
