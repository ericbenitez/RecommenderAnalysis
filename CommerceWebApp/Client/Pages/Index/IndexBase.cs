using CommerceWebApp.Shared;
using Microsoft.AspNetCore.Components;

namespace CommerceWebApp.Client.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject]
        protected HttpClient? httpClient { get; set; }

        // protected override async Task OnInitializedAsync()
        // {
        //     // this.Products = await httpClient!.GetFromJsonAsync<List<Product>>("api/products");
        // }
    }
}