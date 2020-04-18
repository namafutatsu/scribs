using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Scribs.Core.Entities {

    public class Text: Entity {
        [BsonElement]
        public string UserId { get; set; }
        [BsonElement]
        public string ProjectId { get; set; }
        [BsonElement]
        public string Content { get; set; }

        public Text(string userId, string projectId, string content) {
            UserId = userId;
            ProjectId = projectId;
            Content = content;
        }
    }
}
