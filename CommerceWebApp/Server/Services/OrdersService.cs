using CommerceWebApp.Shared;
using Newtonsoft.Json;
using MongoDB.Driver;

namespace CommerceWebApp.Server.Services
{
    public class OrdersService
    {
        public IMongoCollection<Order> OrdersCollection;

        public OrdersService()
        {
            IMongoDatabase database = new MongoClient("mongodb://localhost:27017").GetDatabase("CommerceWebApp");
            this.OrdersCollection = database.GetCollection<Order>("Orders");

            database.DropCollection("Orders");
            this.OrdersCollection = database.GetCollection<Order>("Orders");
        }
    }
}