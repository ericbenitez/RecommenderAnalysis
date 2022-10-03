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
        public CrawlerController()
        {
            Console.WriteLine("Controller started");
            
        }
    }
}