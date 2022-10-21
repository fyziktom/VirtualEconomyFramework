namespace VEBlazor.Editable.Demo.Models
{
    public class Piano
    {
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int NumberOfKeys { get; set; } = 88;
        public double Volume { get; set; } = 0.5;
        public bool IsOn { get; set; } = true;
    }
}
