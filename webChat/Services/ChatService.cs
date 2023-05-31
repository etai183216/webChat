using webChat.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.ComponentModel;
using Amazon.Runtime.Internal;

namespace webChat.Services
{
    public class ChatService
    {
        private readonly IMongoCollection<ChatRoomModels> _chatCollection;

        //伺服器與mongoDB連線，並生成_chatCollection備用
        public ChatService(IOptions<MongoDBSetting> mongoDBSetting)
        {
            var mongoClient = new MongoClient(mongoDBSetting.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSetting.Value.DatabaseName);
            _chatCollection = mongoDatabase.GetCollection<ChatRoomModels>(mongoDBSetting.Value.ChatCollectionName);
        }

        public async Task<List<ChatRoomModels>> GetAsync(string _account)
        {
            var filter = Builders<ChatRoomModels>.Filter.AnyEq(x => x.Member, _account);
            var results = await _chatCollection.Find(filter).ToListAsync();

            return results;
        }



        //------------------------------使用者在聊天室送出訊息
        public async Task<List<ChatRoomModels>> CreateChat(string _objectContent,string _chatRoomId)
        {
            ChatModel? receivedObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatModel>(_objectContent);

            var filter = Builders<ChatRoomModels>.Filter.Eq("_id", ObjectId.Parse(_chatRoomId));
            var update = Builders<ChatRoomModels>.Update.Push("chat", receivedObject);
            await _chatCollection.UpdateOneAsync(filter, update);

            var updateTime = Builders<ChatRoomModels>.Update.Set(x => x.UpdateTime,DateTime.Now);
            await _chatCollection.UpdateOneAsync(filter, updateTime);

            return await _chatCollection.FindSync(filter).ToListAsync();
        }
    }
}
