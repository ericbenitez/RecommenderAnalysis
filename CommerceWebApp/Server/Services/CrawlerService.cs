using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using MongoDB.Driver;
using Newtonsoft.Json;
using CommerceWebApp.Shared;

using System;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace CommerceWebApp.Server.Services
{
    public class CrawlerService
    {
        public bool IsCrawlCompleted = false;
        public IMongoCollection<Page> PagesCollection;
        public Lunr.Index? index;

        public CrawlerService()
        {
            IMongoDatabase database = new MongoClient("mongodb://localhost:27017").GetDatabase("CommerceWebApp");
            // database.DropCollection("Pages");
            this.PagesCollection = database.GetCollection<Page>("Pages");

            Task.Run(async () =>
            {
                // await this.Crawl();
                await this.IndexDocuments();
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
            List<Page> pages = this.PagesCollection.AsQueryable().ToList();

            this.index = await Lunr.Index.Build(async builder =>
            {
               builder
                   .AddField("title")
                   .AddField("body");

               foreach (Page page in pages)
               {
                   if (page.Title != null && page.Body != null && page.Id != null)
                   {
                    // Console.WriteLine( page.Body.Replace("\n", " "));
                       await builder.Add(new Lunr.Document
                       {
                           { "title", page.Title },
                           { "body", page.Body.Replace("\n", " ") },
                           { "id", page.Id }
                       });
                   }

               }

            });

            var client = new ElasticsearchClient();
            
            pages.ForEach(async page => 
            {
                var response = await client.IndexAsync(page);
                Console.WriteLine("attempted");
                if (response.IsValid) 
                {
                    Console.WriteLine($"Index document with ID {response.Id} succeeded."); 
                }
            });
            Console.WriteLine("1");
            
            var request = new SearchRequest("body")
            {
                From = 0,
                Size = 0,
                Query = new TermQuery("body") { Value = "banana"}
            };

            var response = await client.SearchAsync<Page>(request);

            // Console.WriteLine(response.Documents.FirstOrDefault()!.Title);
            Console.WriteLine("2");
            // client.IndexAsync

            
            

            // var ok = index.Search("banana");
            // var asd = 0;
            //  await foreach (Lunr.Result result in ok)
            // {
            //    Console.WriteLine(result.DocumentReference);
               
            //    asd++;
            // }

            // Console.WriteLine(asd);
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