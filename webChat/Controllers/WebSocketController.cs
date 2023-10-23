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
using System.Reflection.Metadata.Ecma335;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

namespace webChat.Controllers;

public class WebSocketController : Controller
{
    private readonly ChatService _chatService;
    private readonly ChatRoomService _chatRoomService;
    public static Dictionary<string, WebSocket> ConnectingUser { get; set; } = new Dictionary<string, WebSocket>();
    public string nowUser = "";

    public WebSocketController(ChatService chatService,ChatRoomService chatRoomService) 
    {
        _chatService = chatService;
        _chatRoomService = chatRoomService;
    }

    [HttpGet("/verification")]
    public  ApiReturnModel verification()
    {
        ApiReturnModel resObj =  new ApiReturnModel();
        resObj.status = MyEnum.ApiStatusCode.Success;
        resObj.contentObject = "true";
        return resObj;
    }

    [AllowAnonymous]
    [HttpGet("/wsConnect/{_account}")]
    public async Task wsConnectEntry(string _account)
    {
        //不是websocket的連線要求，就回應400，並且不允許建立連線。
        if (!HttpContext.WebSockets.IsWebSocketRequest || _account.Trim() == String.Empty)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        //HttpContext.WebSockets.AcceptWebSocketAsync()----->允許建立連線
        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        nowUser = _account;
        //把連線者的id跟websocket記錄在字典裡，之後要用來推播。
        if (ConnectingUser.ContainsKey(_account)) ConnectingUser.Remove(_account);

        ConnectingUser.Add(_account, webSocket);
        //進入監聽迴圈，裡面可做路由
        await wsEcho(webSocket);

    }

    private async Task wsEcho(WebSocket webSocket)
    {
        //建立緩衝區所要使用的陣列。
        var buffer = new byte[1024 * 4];
        //緩衝區就是長度限制，是Send跟Receive要用的buffer。如果設1，送出的訊息會不斷重複，所以要設置合理範圍。
        var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        //進入監聽迴圈，一直到接收到的訊息中的CloseStatus有值，這個值是主動斷開連結的那一端會給上。
        while (!receiveResult.CloseStatus.HasValue)
        {
            //將收到、放在緩衝區的資料解碼(Encoding)成字串。
            string receivedMessage = Encoding.UTF8.GetString(buffer);
            //將字串再轉成SendEntry，是一個自己定義的入口模型，屬性type是用來做路由識別的。
            SendEntry? receivedObject = Newtonsoft.Json.JsonConvert.DeserializeObject<SendEntry>(receivedMessage);
            if (receivedObject == null) continue;

             MyEnum.EntryType requiredType = receivedObject.Type;
            //進入路由，路由中會透過屬性type決定要進入哪個function，並將結果回傳出來
            (List<string> members ,SendModel preSendModel) = await wsRouter(receivedObject);

            foreach (string member in members)
            {
                //將物件轉為Json字串
                var serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(preSendModel);
                //再將Json字串轉為可被緩衝區所使用的byte[]類型
                var dataBytes = Encoding.UTF8.GetBytes(serializedData);
                //將結果送出
                if (ConnectingUser.ContainsKey(member))
                {
                    var nowWebsocket = ConnectingUser[member];
                    //用對應到的websocket物件送出訊息
                    await nowWebsocket.SendAsync(
                        new ArraySegment<byte>(dataBytes),
                        receiveResult.MessageType,
                        receiveResult.EndOfMessage,
                        CancellationToken.None);
                }
            }
            //清空陣列
            buffer = new byte[1024 * 4];
            //繼續等待下一個訊息要求
            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        //如果跳出監聽迴圈，就主動發訊息將WebSocket連結斷開
        await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
        ConnectingUser.Remove(nowUser);
    }

    private async Task<(List<string>,SendModel)> wsRouter(SendEntry? _receivedObject)
    {
        //防呆，如果進來是空值就返回
        if (_receivedObject == null) return (new List<string>(), new SendModel());
        //先將receivedObject.Type讀值存進變數routerFlag中，避免導向過程中重複讀取。
        MyEnum.EntryType routerFlag = _receivedObject.Type;
        //開始路由判斷
        //如果是SendingMessage代表客戶端送來的訊息是他發送訊息到某個對話
        if (routerFlag == MyEnum.EntryType.SendingMessage)
            return await _chatService.InsertChatAsync(_receivedObject.ContentObject, _receivedObject.ChatRoomId);
        //如果是RequiredAllMessage代表送來的訊息是要求所有訊息
        else if (routerFlag == MyEnum.EntryType.RequiredAllMessage)
            return await _chatRoomService.GetMemberChatRoomAsync(_receivedObject.ContentObject);
        else
            return (new List<string>(), new SendModel());
    }
}
