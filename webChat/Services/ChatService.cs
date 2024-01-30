using webChat.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.ComponentModel;
using Amazon.Runtime.Internal;
using webChat.EntryModels;
using webChat.ViewModels;
using Newtonsoft.Json;
using System.Security.Principal;


namespace webChat.Services
{
    public class ChatService
    {
        private readonly IMongoCollection<ChatRoomModels> _chatRoomCollection;

        //伺服器與mongoDB連線，並生成_chatCollection備用
        public ChatService(IOptions<MongoDBSetting> mongoDBSetting)
        {
            var mongoClient = new MongoClient(mongoDBSetting.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSetting.Value.DatabaseName);
            _chatRoomCollection = mongoDatabase.GetCollection<ChatRoomModels>(mongoDBSetting.Value.ChatCollectionName);
        }


        //------------------------------使用者在聊天室送出訊息
        public async Task<(List<string>, SendModel)> InsertChatAsync(string _objectContent, string _chatRoomId)
        {
            ChatModel? receivedObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatModel>(_objectContent);

            if (receivedObject == null) return (new List<string>(), new SendModel());

            ChatModel tempChatModel = new ChatModel();
            tempChatModel.Sender = receivedObject.Sender;
            tempChatModel.ChatTime = receivedObject.ChatTime;
            tempChatModel.Content = receivedObject.Content;

            //以objectId找出聊天室 再insert 一筆對話紀錄
            FilterDefinition<ChatRoomModels> filter = Builders<ChatRoomModels>.Filter.Eq("_id", ObjectId.Parse(_chatRoomId));
            UpdateDefinition<ChatRoomModels> update = Builders<ChatRoomModels>.Update.Push("chat", tempChatModel);//插入資料列
            await _chatRoomCollection.UpdateOneAsync(filter, update);

            //更新聊天室最後更新時間
            UpdateDefinition<ChatRoomModels> updateTime = Builders<ChatRoomModels>.Update.Set(x => x.UpdateTime, receivedObject.ChatTime);//更新時間
            await _chatRoomCollection.UpdateOneAsync(filter, updateTime);

            //搜出最新一筆本人送出的聊天紀錄做回傳
            var filter2 = Builders<ChatRoomModels>.Filter.Eq(x => x.Id, _chatRoomId);
            ChatRoomModels lastSendChatRoom = (await _chatRoomCollection.Find(filter2).ToListAsync())[0];
            List<ChatModel> lastSendChat = lastSendChatRoom.chat;

            var newestMessage = lastSendChat.Where(obj => obj.Sender == receivedObject.Sender)
                .OrderByDescending(obj => obj.ChatTime).FirstOrDefault();

            List<ChatRoomModels> result = await _chatRoomCollection.Find(filter).ToListAsync();

            if (result.Count != 1) return (new List<string>(), new SendModel());


            //生成return value
            SendModel res = new SendModel();
            List<string> members = result[0].Member;
            res.contentObject = JsonConvert.SerializeObject(newestMessage);
            res.entryTypeCode = MyEnum.EntryType.SendingMessage;

            return (members, res);
        }
    }
}
