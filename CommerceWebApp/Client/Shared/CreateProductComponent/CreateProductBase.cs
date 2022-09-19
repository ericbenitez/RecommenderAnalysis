using CommerceWebApp.Shared;
using Microsoft.AspNetCore.Components;
using System.Text;
using Newtonsoft.Json;

namespace CommerceWebApp.Client.Shared
{
    public class CreateProductBase : ComponentBase
    {
        [Parameter]
        public EventCallback<ProductPage> SetPage { get; set; }

        [Inject]
        protected HttpClient? httpClient { get; set; }

        protected string JSONText = "";

        protected void SetJSONText(ChangeEventArgs input)
        {
            JSONText = input.Value!.ToString()!;
        }

        protected async Task SubmitJSON()
        {
            Product? product = JsonConvert.DeserializeObject<Product>(this.JSONText);
            StringContent content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var ok = await httpClient!.PostAsync("api/products/createProduct", content);
            await this.SetPage.InvokeAsync(ProductPage.Products);
            this.JSONText = "";
        }
    }
}