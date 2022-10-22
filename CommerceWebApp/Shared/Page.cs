using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CommerceWebApp.Shared
{
    public class Page
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Url { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public List<string> OutgoingLinks = new();
        public int OutgoingLinksCount { get; set; }
        public List<string> IncomingLinks = new();
        public int IncomingLinksCount { get; set; }
        public double SearchScore { get; set; }
    }
}
