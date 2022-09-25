namespace CommerceWebApp.Shared
{
    public class Order
    {
        public string? Name { get; set; }
        public Dictionary<string, int>? Products { get; set; }
    }
}