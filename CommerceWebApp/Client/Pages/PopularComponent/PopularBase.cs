using CommerceWebApp.Shared;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace CommerceWebApp.Client.Pages.PopularComponent
{
    public class PopularBase : ComponentBase
    {
        [Inject]
        protected HttpClient? httpClient { get; set; }
        
        protected IEnumerable<Page>? PopularPages { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var result = await httpClient!.GetAsync("api/crawler/popular");
            this.PopularPages = JsonConvert.DeserializeObject<IEnumerable<Page>>(await result.Content.ReadAsStringAsync());
        }
    }
}