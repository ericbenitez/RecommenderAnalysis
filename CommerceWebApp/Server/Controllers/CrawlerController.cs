using CommerceWebApp.Shared;
using CommerceWebApp.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MongoDB.Driver;

namespace CommerceWebApp.Server.Controllers
{
    [ApiController]
    [Route("api/crawler/")]
    public class CrawlerController : ControllerBase
    {
        private readonly CrawlerService crawlerService;

        public CrawlerController(
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
    }
}
