using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using System.Net.WebSockets;
using System.Text;
using webChat.Models;
using webChat.Services;
using Newtonsoft.Json;
using webChat.EntryModels;
using MongoDB.Driver;
using Amazon.Runtime.Internal.Util;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;

namespace webChat.Controllers;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class ChatRoomController : ControllerBase
{
    private readonly ChatRoomService _chatRoomService;
    public ChatRoomController(ChatRoomService chatRoomService) => _chatRoomService = chatRoomService;

    [HttpPost("/CreateChatRoom")]
    public async Task<string> CreateChatRoom(CreateChatRoomEntry _createChatRoomEntry)
    {
        if (_createChatRoomEntry == null) return "0";
        if ((_createChatRoomEntry.members.Count == 0) || _createChatRoomEntry.ChatRoomName == "") return "0";
        string res = await _chatRoomService.CreateChatRoom(_createChatRoomEntry);
        return res;
    }

}
