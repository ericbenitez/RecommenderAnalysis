using CommerceWebApp.Shared;
using CommerceWebApp.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CommerceWebApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                return this.productsService.Products;
            });
            
            return StatusCode(200, JsonConvert.SerializeObject(products));
        }

        [HttpPost("createProduct")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            Product? createdProduct = await Task.Run(() => {
                return this.productsService.AddProduct(product);
            });

            return createdProduct != null ? 
                StatusCode(200, "Created Product successfully") :
                StatusCode(400, "Could not create Product");
        }
    }
}