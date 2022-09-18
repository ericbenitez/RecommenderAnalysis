using CommerceWebApp.Shared;
using Newtonsoft.Json;

namespace CommerceWebApp.Server.Services
{
    public class ProductsService
    {
        public IEnumerable<Product> Products;

        public ProductsService()
        {
            string json = System.IO.File.ReadAllText("Data/products.json");
            this.Products = JsonConvert.DeserializeObject<IEnumerable<Product>>(json)!;
        }
    }
}
