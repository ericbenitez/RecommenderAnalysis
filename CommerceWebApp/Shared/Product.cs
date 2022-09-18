namespace CommerceWebApp.Shared
{
    public class Product
    {
        public string? name {get; set; }
        public int price { get; set; }
        public int stock { get; set; }
        public int id { get; set; }
        public List<string> reviews = new();
        public Dimensions? dimensions {get; set; }
    }
}