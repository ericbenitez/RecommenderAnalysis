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

        public bool AddProduct(Product product)
        {
            bool isDataValid = product.Name != null && product.Dimensions != null;
            bool notExists = !this.Products.Exists(someProduct => someProduct.Id == product.Id);

            if (isDataValid && notExists)
            {
                this.Products.Add(product);
                return true;
            }
            
            return false;
        }

        public bool SubmitReview(ReviewDto reviewDto)
        {
            Product? product = this.Products.Find(product => product.Id == reviewDto.Id);
            if (product != null && reviewDto.Review != null)
            {
                product.Reviews.Add(reviewDto.Review);
                return true;
            }

            return false;
        }
    }
}
