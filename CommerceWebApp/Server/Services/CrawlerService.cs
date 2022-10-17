using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using MongoDB.Driver;
using Newtonsoft.Json;
using CommerceWebApp.Shared;

namespace CommerceWebApp.Server.Services
{
    public class CrawlerService
    {
        public bool IsCrawlCompleted = false;
        public IMongoCollection<Page> PagesCollection;

        public CrawlerService()
        {
            IMongoDatabase database = new MongoClient("mongodb://localhost:27017").GetDatabase("CommerceWebApp");
            database.DropCollection("Pages");
            this.PagesCollection = database.GetCollection<Page>("Pages");

            Task.Run(async () =>
            {
                // await this.Crawl();
                //await this.IndexDocuments();
            });


        }

        private async Task Crawl()
        {
            var config = new CrawlConfiguration
            {
                // MaxPagesToCrawl = 10,
                // MinCrawlDelayPerDomainMilliSeconds = 10,
                MaxConcurrentThreads = 10,
            };

            PoliteWebCrawler crawler = new PoliteWebCrawler(config);
            crawler.PageCrawlCompleted += PageCrawlCompleted;
            CrawlResult crawlResult = await crawler.CrawlAsync(new Uri("https://people.scs.carleton.ca/~davidmckenney/fruitgraph/N-0.html"));
            this.IsCrawlCompleted = true;

            List<Page> pages = this.PagesCollection.AsQueryable().ToList();
            foreach (Page page in pages)
            {
                foreach (string outgoingLink in page.OutgoingLinks)
                {
                    Page? foundPage = pages.Find(somePage => somePage.Url == outgoingLink);
                    if (foundPage != null && page.Url != null)
                    {
                        foundPage.IncomingLinks.Add(page.Url);
                        foundPage.IncomingLinksCount = foundPage.IncomingLinks.Count;
                    }
                }
            }

            foreach (Page page in pages)
            {
                if (page.Url != null)
                {
                    this.PagesCollection.ReplaceOne(
                        somePage => somePage.Url == page.Url,
                        page,
                        new ReplaceOptions { IsUpsert = true }
                    );
                }
            }
        }

        private async Task IndexDocuments()
        {
            //List<Page> pages = this.PagesCollection.AsQueryable().ToList();

            //var index = await Lunr.Index.Build(async builder =>
            //{
            //    builder
            //        .AddField("title")
            //        .AddField("body");

            //    foreach (Page page in pages)
            //    {
            //        if (page.Title != null && page.Body != null && page.Id != null)
            //        {
            //            await builder.Add(new Lunr.Document
            //            {
            //                { "title", page.Title },
            //                { "body", page.Body },
            //                { "id", page.Id }
            //            });
            //        }

            //    }

            //});

            //var ok = index.Search("banana");

            //await foreach (Lunr.Result result in ok)
            //{
            //    Console.WriteLine(result.DocumentReference);
            //}
        }

        private void PageCrawlCompleted(object? sender, PageCrawlCompletedArgs args)
        {
            if (args.CrawledPage.ParsedLinks != null)
            {
                bool notDocumentExists = this.PagesCollection.Find(page => page.Url == args.CrawledPage.Uri.ToString()).FirstOrDefault() == null;
                if (notDocumentExists)
                {
                    IEnumerable<string> outgoingLinks = args.CrawledPage.ParsedLinks.Select(link => link.HrefValue.ToString());

                    this.PagesCollection.InsertOne(new Page
                    {
                        Url = args.CrawledPage.Uri.ToString(),
                        Title = args.CrawledPage.AngleSharpHtmlDocument.Title,
                        Body = args.CrawledPage.AngleSharpHtmlDocument.Body.TextContent,
                        OutgoingLinks = outgoingLinks.ToList(),
                        OutgoingLinksCount = outgoingLinks.Count()
                    });
                }
            }
        }
    }
}