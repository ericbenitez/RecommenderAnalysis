using CommerceWebApp.Shared;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace CommerceWebApp.Client.Pages.IndexComponent
{
    public class IndexBase : ComponentBase
    {
        [Inject]
        protected HttpClient? httpClient { get; set; }

        protected IEnumerable<Page>? IndexedPages;

        protected async Task SetSearchText(ChangeEventArgs input)
        {
            
            var text = input.Value!.ToString()!.Trim();

            if (text != "")
            {
                this.IndexedPages = await this.httpClient!.GetFromJsonAsync<IEnumerable<Page>>($"api/pages/index/{text}");
            }

            else
                this.IndexedPages = null;
        }
    }
}