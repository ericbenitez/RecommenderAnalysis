using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;

namespace CommerceWebApp.Server.Services
{
    public class CrawlerService
    {
        public bool IsCrawlCompleted = false;

        public CrawlerService()
        {
            Task.Run(async () => {
                await this.Crawl();
            });
        }

        private async Task Crawl()
        {
            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 10,
                MinCrawlDelayPerDomainMilliSeconds = 10,
                MaxConcurrentThreads = 10,
            };

            PoliteWebCrawler crawler = new PoliteWebCrawler(config);
            crawler.PageCrawlCompleted += PageCrawlCompleted;
            Console.WriteLine("Begin crawl");
            CrawlResult crawlResult = await crawler.CrawlAsync(new Uri("https://people.scs.carleton.ca/~davidmckenney/fruitgraph/N-0.html"));
            this.IsCrawlCompleted = true;
        }

        private void PageCrawlCompleted(object? sender, PageCrawlCompletedArgs args)
        {
            if (args.CrawledPage.ParsedLinks != null)
            {
                Console.WriteLine(args.CrawledPage.Uri.ToString());
                Console.WriteLine("Links: [");

                foreach (var link in args.CrawledPage.ParsedLinks)
                {
                    Console.Write("    ");
                    Console.WriteLine(link.HrefValue);
                }
                Console.WriteLine("]\n --------------------------------------");
            } 
        }
    }
}