using CommerceWebApp.Shared;
using Newtonsoft.Json;
using MongoDB.Driver;

namespace CommerceWebApp.Server.Services
{
    public class ProductsService
    {
        public IMongoCollection<Product> ProductsCollection;

        public ProductsService()
        {
            IMongoDatabase database = new MongoClient("mongodb://localhost:27017").GetDatabase("CommerceWebApp");
            this.ProductsCollection = database.GetCollection<Product>("Products");

            // database.DropCollection("Products");
            // this.ProductsCollection = database.GetCollection<Product>("Products");
            // string json = System.IO.File.ReadAllText("Data/products.json");
            // List<Product> products = JsonConvert.DeserializeObject<List<Product>>(json)!;
            // foreach(Product product in products)
            // {
            //     this.ProductsCollection.InsertOne(product);
            // }
        }

        public bool AddProduct(Product product)
        {
            bool isDataValid = product.Name != null && product.Dimensions != null;
            bool notExists = this.ProductsCollection.Find(someProduct => someProduct.Id == product.Id).FirstOrDefault() == null;

            if (isDataValid && notExists)
            {
                this.ProductsCollection.InsertOne(product);
                return true;
            }
            
            return false;
        }

        public bool SubmitReview(int id, string review)
        {
            Product? product = this.ProductsCollection.Find(product => product.Id == id).FirstOrDefault();
            if (product != null && review != "")
            {
                product.Reviews.Add(review);
                var result = this.ProductsCollection.ReplaceOne(
                    someProduct => someProduct.Id == product.Id, 
                    product, 
                    new ReplaceOptions {IsUpsert = true}
                );

                return result.IsAcknowledged;
            }

            return false;
        }
    }
}
