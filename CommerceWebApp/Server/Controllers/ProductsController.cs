using CommerceWebApp.Shared;
using CommerceWebApp.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MongoDB.Driver;

namespace CommerceWebApp.Server.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsService productsService;

        public ProductsController(
            ProductsService productsService
        ) {
            this.productsService = productsService;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetProducts()
        {
            IEnumerable<Product> products = await Task.Run(() => {
                return this.productsService.ProductsCollection.AsQueryable();
            });
            
            return StatusCode(200, JsonConvert.SerializeObject(products));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            Product? product = await Task.Run(() => {
                return this.productsService.ProductsCollection.Find(product => product.Id == id).FirstOrDefault();
            });

            if (product != null)
            {
                return StatusCode(200, JsonConvert.SerializeObject(product));
            }

            else
            {
                return StatusCode(400, "Could not find product");
            }
        }

        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetProductReviews(int id)
        {
            Product? product = await Task.Run(() => {
                return this.productsService.ProductsCollection.Find(product => product.Id == id).FirstOrDefault();
            });

            if (product != null)
            {
                return StatusCode(200, JsonConvert.SerializeObject(product.Reviews));
            }

            else
            {
                return StatusCode(400, "Could not find product");
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            bool createdProduct = await Task.Run(() => {
                return this.productsService.AddProduct(product);
            });

            return createdProduct ? 
                StatusCode(200, "Created Product successfully") :
                StatusCode(400, "Could not create Product");
        }

        [HttpPost("{id}/reviews/")]
        public async Task<IActionResult> SubmitReview(int id, [FromBody] string review)
        {
            bool submittedReview = await Task.Run(() => {
                return this.productsService.SubmitReview(id, review);
            });

            return submittedReview ?
                StatusCode(200, "Submitted Review") :
                StatusCode(400, "Could not submit review");
        }
    }
}