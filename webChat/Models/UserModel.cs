using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace webChat.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = "";

        [BsonElement("account")]
        public string Account { get; set; } = "";

        [BsonElement("pw")]
        public string Password { get; set; } = "";

        [BsonElement("nickName")]
        public string nickName { get; set; } = "";
    }
}
