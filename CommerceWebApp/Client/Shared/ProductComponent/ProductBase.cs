using Microsoft.AspNetCore.Components;
using CommerceWebApp.Shared;
using Newtonsoft.Json;
using System.Text;

namespace CommerceWebApp.Client.Shared
{
    public class ProductBase : ComponentBase
    {
        [Parameter]
        public Product? Product { get; set; }

        [Parameter]
        public EventCallback<ProductPage> SetPage { get; set; }

        [Inject]
        protected HttpClient? httpClient { get; set; }

        protected string CurrentReview = "5";

        protected void SetCurrentReview(ChangeEventArgs input)
        {
            this.CurrentReview = input.Value!.ToString()!;
        }

        protected async Task SubmitReview()
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(this.CurrentReview), Encoding.UTF8, "application/json");
            await httpClient!.PostAsync($"api/products/{this.Product!.Id}/reviews/submit", content);
            await this.SetPage.InvokeAsync(ProductPage.Products);
            this.CurrentReview = "5";
        }
    }
}