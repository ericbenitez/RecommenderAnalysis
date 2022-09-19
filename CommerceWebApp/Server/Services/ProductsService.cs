using CommerceWebApp.Shared;
using Newtonsoft.Json;

namespace CommerceWebApp.Server.Services
{
    public class ProductsService
    {
        public List<Product> Products;

        public ProductsService()
        {
            string json = System.IO.File.ReadAllText("Data/products.json");
            this.Products = JsonConvert.DeserializeObject<List<Product>>(json)!;
        }

        public Product? AddProduct(Product product)
        {
            bool isDataValid = product.Name != null && product.Dimensions != null;
            bool notExists = !this.Products.Exists(someProduct => someProduct.Id == product.Id);

            if (isDataValid && notExists)
            {
                Console.WriteLine("added it");
                this.Products.Add(product);
                return product;
            }
            Console.WriteLine("someting went wrong");
            return null;
        }
    }
}
