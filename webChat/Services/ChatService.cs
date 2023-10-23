using webChat.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.ComponentModel;
using Amazon.Runtime.Internal;
using webChat.ViewModels;
using System.Linq;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization;

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


        //------------------------------使用者在聊天室送出訊息
        public async Task<(List<string>,SendModel)> InsertChatAsync(string _objectContent,string _chatRoomId)
        {
            ChatModel? receivedObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatModel>(_objectContent);

            if (receivedObject == null) return (new List<string>(),new SendModel());

            ChatModel tempChatModel = new ChatModel();
            tempChatModel.Sender = receivedObject.Sender;
            tempChatModel.ChatTime = receivedObject.ChatTime;
            tempChatModel.Content = receivedObject.Content;

            //以objectId找出聊天室 再insert 一筆對話紀錄
            FilterDefinition<ChatRoomModels> filter = Builders<ChatRoomModels>.Filter.Eq("_id", ObjectId.Parse(_chatRoomId));
            UpdateDefinition<ChatRoomModels> update = Builders<ChatRoomModels>.Update.Push("chat", tempChatModel);//插入資料列
            await _chatCollection.UpdateOneAsync(filter, update);
           
            //更新聊天室最後更新時間
            UpdateDefinition<ChatRoomModels> updateTime = Builders<ChatRoomModels>.Update.Set(x => x.UpdateTime, receivedObject.ChatTime);//更新時間
            await _chatCollection.UpdateOneAsync(filter, updateTime);

            ProjectionDefinition<ChatRoomModels> projection =  Builders<ChatRoomModels>.Projection.Include(x => x.Member);
            BsonDocument result = _chatCollection.Find(filter).Project(projection).FirstOrDefault();

            //生成return value
            SendModel res = new SendModel();
            List<string> members = result.Select(item => item.ToString()).ToList();
            res.contentObject = JsonConvert.SerializeObject(receivedObject);
            res.entryTypeCode = MyEnum.EntryType.SendingMessage;

            return (members,res);
        }
    }
}
