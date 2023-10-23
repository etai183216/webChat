using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using webChat.Models;

namespace webChat.ViewModels
{
    public class MutipleChatModel
    {
        public MyEnum.EntryType entryTypeCode { get; set; } = 0;
        public List<ChatRoomModels> chatRooms { get; set; } = new List<ChatRoomModels>();
    }
}
