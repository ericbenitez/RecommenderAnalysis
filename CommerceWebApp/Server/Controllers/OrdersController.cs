using CommerceWebApp.Shared;
using CommerceWebApp.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MongoDB.Driver;

namespace CommerceWebApp.Server.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly ProductsService productsService;
        private readonly OrdersService ordersService;

        public OrdersController(
            ProductsService productsService,
            OrdersService ordersService
        ) {
            this.productsService = productsService;
            this.ordersService = ordersService;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetOrders()
        {
            IEnumerable<Order> orders = await Task.Run(() => {
                return this.ordersService.OrdersCollection.AsQueryable();
            });

            return StatusCode(200, JsonConvert.SerializeObject(orders));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            Order? order = await Task.Run(() => {
                IEnumerable<Order> orders = this.ordersService.OrdersCollection.AsQueryable();
                if (orders.Count() < id)
                    return null;

                return orders.ToArray()[id];
            });

            return order != null ?
                StatusCode(200, JsonConvert.SerializeObject(order)):
                StatusCode(404, "Could not find order");
        }

        [HttpPost("")]
        public async Task<IActionResult> PostOrder([FromBody] Order order)
        {
            if (order.Name != null && order.Products != null)
            {
                IEnumerable<Product> products = await Task.Run(() => {
                   return this.productsService.ProductsCollection.AsQueryable(); 
                });

                foreach (var (productId, quantity) in order.Products)
                {
                    Product? product = products.FirstOrDefault(product => product.Id == productId);
                    if (product == null)
                        return StatusCode(409, $"There is no product with Id: {productId}");
                    
                    if (quantity < 0)
                        return StatusCode(409, $"Invalid quantity for product Id: {productId}");
                    
                    if (product.Stock < quantity)
                        return StatusCode(409, $"Not enough in stock for product Id: {productId}");
                
                    product.Stock -= quantity;
                    var result = this.productsService.ProductsCollection.ReplaceOne(
                        someProduct => someProduct.Id == product.Id,
                        product,
                        new ReplaceOptions {IsUpsert = true}
                    );

                    if (!result.IsAcknowledged)
                        return StatusCode(404, $"Error with database when updating quantity of product Id: {productId}");
                }

                this.ordersService.OrdersCollection.InsertOne(order);
                return StatusCode(201, "Order created");
            }

            return StatusCode(409, "Name was null or Products was null");
        }
    }
}