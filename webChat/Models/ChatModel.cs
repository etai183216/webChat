using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace webChat.Models
{
    public class ChatModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ChatId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("chatTime")]
        public DateTime ChatTime { get; set; } = DateTime.Now;

        [BsonElement("sender")]
        public string Sender { get; set; } = "";

        [BsonElement("content")]
        public string Content { get; set; } = "";
    }
}
