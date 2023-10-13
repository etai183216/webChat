using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace webChat.Models
{
    public class ChatRoomModels
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("member")]
        public List<string> Member { get; set; } = new List<string>();

        [BsonElement("updateTime")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        [BsonElement("chat")]
        public List<ChatModel> chat { get; set; } = new List<ChatModel>();

        [BsonElement("chatRoomName")]
        public string? chatRoomName { get; set; } = "";
    }
}
