namespace VEFramework.Demo.MusicBandDisplay.Models
{
    public class Post
    {
        public string Utxo { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Profile AuthorProfile { get; set; } = new Profile();
        public string Image { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
    }
}
