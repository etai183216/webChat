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
using webChat.ViewModels;

namespace webChat.Controllers;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class ChatRoomController : ControllerBase
{
    private readonly ChatRoomService _chatRoomService;
    public ChatRoomController(ChatRoomService chatRoomService) => _chatRoomService = chatRoomService;

    [HttpPost("/CreateChatRoom")]
    public async Task<ApiReturnModel> CreateChatRoom(CreateChatRoomEntry _createChatRoomEntry)
    {
        
        if (_createChatRoomEntry == null) return new ApiReturnModel();
        if ((_createChatRoomEntry.members.Count == 0) || _createChatRoomEntry.ChatRoomName == "") return new ApiReturnModel();

        return await _chatRoomService.CreateChatRoomAsync(_createChatRoomEntry);
    }

}
