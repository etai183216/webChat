using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using webChat.Models;
namespace webChat.ViewModels
{
    public class SingleChatModel
    {
        public MyEnum.EntryType entryTypeCode { get; set; } = 0;
        public ChatModel chatModel { get; set; } = new ChatModel();
        public string chatRoomId { get; set; } = string.Empty;
    }

  
}
