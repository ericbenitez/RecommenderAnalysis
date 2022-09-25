namespace CommerceWebApp.Shared
{
    public class Order
    {
        public string? Name { get; set; }
        public Dictionary<int, int>? Products { get; set; }
    }
}