using CommerceWebApp.Shared;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace CommerceWebApp.Client.Pages.Products
{
    public class ProductsComponent : ComponentBase
    {
        [Inject]
        protected HttpClient? httpClient { get; set; }
        
        protected IEnumerable<Product>? products;
        protected string productName = "";
        protected bool inStockOnly = false;

        protected override async Task OnInitializedAsync()
        {
            products = await httpClient!.GetFromJsonAsync<List<Product>>("api/products");
        }

        protected void setProductName(ChangeEventArgs input)
        {
            productName = input.Value!.ToString()!;
        }

        protected void toggleInStockOnly()
        {
            this.inStockOnly = !this.inStockOnly;
        }

        protected void viewProduct(int productId)
        {
            Console.WriteLine("attempting to view product " + productId);
        }

        protected IEnumerable<Product> getProducts()
        {
            IEnumerable<Product> filteredProducts = this.products ?? new List<Product>();

            filteredProducts = filteredProducts.Where(product =>
            {
                bool productNameFilter = (this.productName == "")
                    ? true
                    : product.name!.ToLower().StartsWith(this.productName.ToLower())
                        || product.price.ToString().StartsWith(this.productName.ToLower())
                        || product.stock.ToString().StartsWith(this.productName.ToLower());

                bool inStockFilter = (this.inStockOnly)
                    ? (product.stock > 0)
                        ? true
                        : false
                    : true;

                return productNameFilter && inStockFilter;
            });


            return filteredProducts;
        }
    }
}