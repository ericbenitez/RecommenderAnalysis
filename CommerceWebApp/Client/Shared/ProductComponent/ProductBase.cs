using Microsoft.AspNetCore.Components;
using CommerceWebApp.Shared;

namespace CommerceWebApp.Client.Shared
{
    public class ProductBase : ComponentBase
    {
        [Parameter]
        public Product? Product { get; set; }

        [Parameter]
        public EventCallback<ProductPage> SetPage { get; set; }
    }
}