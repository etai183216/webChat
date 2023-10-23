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


        public async Task<(List<string>, SendModel)> GetMemberChatRoomAsync(string _account)
        {
            var filter = Builders<ChatRoomModels>.Filter.AnyEq(x => x.Member, _account);
            var results = await _chatCollection.Find(filter).ToListAsync();

            //生成return value
            SendModel res = new SendModel();
            List<string> members = new List<string>() { _account };
            res.contentObject = JsonConvert.SerializeObject(results);
            res.entryTypeCode = MyEnum.EntryType.RequiredAllMessage;

            return (members, res);

        }


        public async Task<ApiReturnModel> CreateChatRoomAsync(CreateChatRoomEntry CreateChatRoomObj)
        {
            ApiReturnModel resObj = new ApiReturnModel();
            if (CreateChatRoomObj == null || CreateChatRoomObj.userId==""|| CreateChatRoomObj.ChatRoomName == "" || CreateChatRoomObj.members.Count == 0)
            {
                resObj.status = MyEnum.ApiStatusCode.Error;
                return resObj;
            }

            ChatRoomModels tempModel = new ChatRoomModels();
            tempModel.chat = new List<ChatModel>();
            tempModel.Member = CreateChatRoomObj.members;
            tempModel.chatRoomName = CreateChatRoomObj.ChatRoomName;
            await _chatCollection.InsertOneAsync(tempModel);

            var filter = Builders<ChatRoomModels>.Filter.AnyEq(x => x.Member, CreateChatRoomObj.userId);
            var results = await _chatCollection.Find(filter).ToListAsync();
            resObj.contentObject = JsonConvert.SerializeObject(results);
            resObj.status = MyEnum.ApiStatusCode.Success;

            return resObj;
        }
    }
}
