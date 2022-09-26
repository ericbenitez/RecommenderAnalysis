namespace CommerceWebApp.Shared
{
    public class Order
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public Dictionary<string, int>? Products { get; set; }
    }
}