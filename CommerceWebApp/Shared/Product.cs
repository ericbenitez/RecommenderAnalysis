namespace CommerceWebApp.Shared
{
    public class Product
    {
        public string? Name { get; set; }
        public int Id { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; }
        public Dimensions? Dimensions { get; set; }
        public List<string> Reviews = new();
    }
}