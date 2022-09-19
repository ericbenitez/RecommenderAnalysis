using CommerceWebApp.Shared;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace CommerceWebApp.Client.Pages.Products
{
    public class ProductsBase : ComponentBase
    {
        [Inject]
        protected HttpClient? httpClient { get; set; }

        protected List<Product>? Products;
        protected string SearchText = "";
        protected bool InStockOnly = false;

        protected ProductPage CurrentPage = ProductPage.Products;
        protected Product? CurrentProduct;

        protected override async Task OnInitializedAsync()
        {
            this.Products = await httpClient!.GetFromJsonAsync<List<Product>>("api/products");
        }

        protected void SetSearchText(ChangeEventArgs input)
        {
            this.SearchText = input.Value!.ToString()!;
        }

        protected void ToggleInStockOnly()
        {
            this.InStockOnly = !this.InStockOnly;
        }

        public async void SetPage(ProductPage page)
        {
            this.SearchText = "";
            this.InStockOnly = false;

            this.CurrentPage = page;
            this.Products = await httpClient!.GetFromJsonAsync<List<Product>>("api/products");
            StateHasChanged();
        }

        protected void SetCurrentProduct(Product? product)
        {
            this.CurrentProduct = product;
        }

        protected IEnumerable<Product> GetProducts()
        {
            IEnumerable<Product> filteredProducts = this.Products ?? new List<Product>();

            filteredProducts = filteredProducts.Where(product =>
            {
                bool searchFilter = (this.SearchText == "")
                    ? true
                    : product.Name!.ToLower().StartsWith(this.SearchText.ToLower())
                        || product.Price.ToString().StartsWith(this.SearchText.ToLower())
                        || product.Stock.ToString().StartsWith(this.SearchText.ToLower());

                bool inStockFilter = (this.InStockOnly)
                    ? product.Stock > 0
                    : true;

                return searchFilter && inStockFilter;
            });


            return filteredProducts;
        }
    }
}