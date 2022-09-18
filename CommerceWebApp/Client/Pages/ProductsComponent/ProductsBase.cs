using CommerceWebApp.Shared;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace CommerceWebApp.Client.Pages.Products
{
    public class ProductsBase : ComponentBase
    {
        [Inject]
        protected HttpClient? HttpClient { get; set; }

        protected IEnumerable<Product>? Products;
        protected string ProductName = "";
        protected bool InStockOnly = false;
        protected Product? CurrentProduct;

        protected override async Task OnInitializedAsync()
        {
            Products = await HttpClient!.GetFromJsonAsync<List<Product>>("api/products");
        }

        protected void SetProductName(ChangeEventArgs input)
        {
            ProductName = input.Value!.ToString()!;
        }

        protected void ToggleInStockOnly()
        {
            this.InStockOnly = !this.InStockOnly;
        }

        protected static void ViewProduct(int productId)
        {
            Console.WriteLine("attempting to view product " + productId);
        }

        protected IEnumerable<Product> GetProducts()
        {
            IEnumerable<Product> filteredProducts = this.Products ?? new List<Product>();

            filteredProducts = filteredProducts.Where(product =>
            {
                bool productNameFilter = (this.ProductName == "")
                    ? true
                    : product.Name!.ToLower().StartsWith(this.ProductName.ToLower())
                        || product.Price.ToString().StartsWith(this.ProductName.ToLower())
                        || product.Stock.ToString().StartsWith(this.ProductName.ToLower());

                bool inStockFilter = (this.InStockOnly)
                    ? (product.Stock > 0)
                        ? true
                        : false
                    : true;

                return productNameFilter && inStockFilter;
            });


            return filteredProducts;
        }
    }
}