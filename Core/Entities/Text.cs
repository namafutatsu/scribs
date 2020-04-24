using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Scribs.Core.Services;

namespace Scribs.Core.Entities {

    public class Text: Entity {
        [BsonElement]
        public string UserId { get; set; }
        [BsonElement]
        public string ProjectId { get; set; }
        [BsonElement]
        public string Content { get; set; }

        public Text(string userId, string projectId, Document document) {
            Id = document.Id;
            Name = document.Name;
            UserId = userId;
            ProjectId = projectId;
            Content = document.Content;
        }

        public Text() {
        }
    }
}
