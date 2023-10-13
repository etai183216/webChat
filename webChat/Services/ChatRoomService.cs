using webChat.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.ComponentModel;
using Amazon.Runtime.Internal;
using webChat.EntryModels;

namespace webChat.Services
{
    public class ChatRoomService
    {
        private readonly IMongoCollection<ChatRoomModels> _chatCollection;

        //伺服器與mongoDB連線，並生成_chatCollection備用
        public ChatRoomService(IOptions<MongoDBSetting> mongoDBSetting)
        {
            var mongoClient = new MongoClient(mongoDBSetting.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSetting.Value.DatabaseName);
            _chatCollection = mongoDatabase.GetCollection<ChatRoomModels>(mongoDBSetting.Value.ChatCollectionName);
        }

        /// <summary>
        /// 對資料庫進行操作的funciton
        /// </summary>
        /// <param name="CreateChatRoomObj"></param>
        /// <returns></returns>
        public async Task<string> CreateChatRoom(CreateChatRoomEntry CreateChatRoomObj)
        {
            if (CreateChatRoomObj == null) return "0"; 
            if ((CreateChatRoomObj.ChatRoomName=="")||(CreateChatRoomObj.members.Count==0)) return "0";

            try
            {
                ChatRoomModels tempModel = new ChatRoomModels();
                tempModel.chat = new List<ChatModel>();
                tempModel.Member = CreateChatRoomObj.members;
                tempModel.chatRoomName = CreateChatRoomObj.ChatRoomName;
                await _chatCollection.InsertOneAsync(tempModel);
            }
            catch (Exception ex) 
            {
                return ex.Message;
            }
            
            return "1";
        }
    }
}
