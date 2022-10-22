using CommerceWebApp.Shared;
using CommerceWebApp.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MongoDB.Driver;

namespace CommerceWebApp.Server.Controllers
{
    [ApiController]
    [Route("api/pages/")]
    public class PagesController : ControllerBase
    {
        private readonly CrawlerService crawlerService;

        public PagesController(
            CrawlerService crawlerService
        )
        {
            this.crawlerService = crawlerService;
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular()
        {
            List<Page> pages = await Task.Run(() =>
            {
                return this.crawlerService.PagesCollection.AsQueryable().ToList();
            });

            IEnumerable<Page> popularPages = pages.OrderByDescending(page => page.IncomingLinksCount).Take(10);
            return StatusCode(200, JsonConvert.SerializeObject(popularPages));
        }

        [HttpGet("{title}")]
        public async Task<IActionResult> GetPageByTitle(string title)
        {
            Page? page = await Task.Run(() =>
            {
                return this.crawlerService.PagesCollection.Find(page => page.Title == title).FirstOrDefault();
            });

            if (page != null)
                return StatusCode(200, JsonConvert.SerializeObject(page));
            else
                return StatusCode(404, "Page not found");
        }

        [HttpGet("index/{text}")]
        public async Task<IActionResult> GetIndexedPagesByText(string text)
        {
            List<Page> pages = new();
            if (this.crawlerService.index != null)
            {
                var results = this.crawlerService.index.Search(text);

                int count = 0;
                int maxResults = 10;
                await foreach (Lunr.Result result in results)
                {
                    if (count == maxResults) break;

                    Page? page = this.crawlerService.PagesCollection.Find(page => page.Id == result.DocumentReference).FirstOrDefault();
                    if (page != null)
                    {
                        page.SearchScore = result.Score;
                        pages.Add(page);
                        count++;
                    }
                }

                return StatusCode(200, JsonConvert.SerializeObject(pages));
            }

            else
                return StatusCode(404, "Index not available");
        }
    }
}
