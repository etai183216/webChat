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

public class WebSocketController : Controller
{
    private readonly ChatService _chatService;
    public WebSocketController(ChatService chatService) => _chatService = chatService;
    public static Dictionary<string, WebSocket> ConnectingUser { get; set; } = new Dictionary<string, WebSocket>();
    public string nowUser = "";

    [HttpGet("/verification")]
    public bool verification()
    { 
        return true;
    }

    [AllowAnonymous]
    [HttpGet("/wsConnect/{_account}")]
    public async Task wsConnectEntry(string _account)
    {
        //如果是Websorcket的連結要求，才會許可
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            //HttpContext.WebSockets.AcceptWebSocketAsync()----->允許建立連線
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            //先暫時這樣，之後要先用Api，確認登入帳號密碼是否正確，正確才建立連線。
            nowUser = _account;
            //把連線者的id跟websocket記錄在字典裡，之後要用來推播。
            if (ConnectingUser.ContainsKey(_account)) ConnectingUser.Remove(_account);

            ConnectingUser.Add(_account, webSocket);
            //進入監聽迴圈，裡面可做路由
            await wsEcho(webSocket);
        }
        else
        {
            //不是websocket的連線要求，就回應400，並且不允許建立連線。
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task wsEcho(WebSocket webSocket)
    {
        //建立緩衝區所要使用的陣列。
        var buffer = new byte[1024*4];
        //緩衝區就是長度限制，是Send跟Receive要用的buffer。如果設1，送出的訊息會不斷重複，所以要設置合理範圍。
        var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        //進入監聽迴圈，一直到接收到的訊息中的CloseStatus有值，這個值是主動斷開連結的那一端會給上。
        while(!receiveResult.CloseStatus.HasValue) 
        {   
            //將收到、放在緩衝區的資料解碼(Encoding)成字串。
            string receivedMessage = Encoding.UTF8.GetString(buffer);
            //將字串再轉成SendEntry，是一個自己定義的入口模型，屬性type是用來做路由識別的。
            SendEntry? receivedObject=new SendEntry { };
            receivedObject = Newtonsoft.Json.JsonConvert.DeserializeObject<SendEntry>(receivedMessage);

            //進入路由，路由中會透過屬性type決定要進入哪個function，並將結果回傳出來
            var members = await wsRouter(receivedObject);

            foreach (var member in members)
            {
                var temp = await _chatService.GetAsync(member);
                //將物件轉為Json字串
                var serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(temp);
                //再將Json字串轉為可被緩衝區所使用的byte[]類型
                var dataBytes = Encoding.UTF8.GetBytes(serializedData);
                //將結果送出
                if(ConnectingUser.ContainsKey(member))
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
            buffer = new byte[1024*4];
            //繼續等待下一個訊息要求
            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),CancellationToken.None);
        }

        //如果跳出監聽迴圈，就主動發訊息將WebSocket連結斷開
        await webSocket.CloseAsync(receiveResult.CloseStatus.Value,receiveResult.CloseStatusDescription, CancellationToken.None);
        ConnectingUser.Remove(nowUser);
    }

    private async Task<List<string>> wsRouter(SendEntry? _receivedObject) 
    {
        //防呆，如果進來是空值就返回
        if (_receivedObject == null) return new List<string> {};
        //先將receivedObject.Type讀值存進變數routerFlag中，避免導向過程中重複讀取。
        string routerFlag = _receivedObject.Type;
        //開始路由判斷
        //如果是sendMessage代表客戶端送來的訊息是他發送訊息到某個對話
        if (routerFlag == "sendMessage")
        {
            var result = await _chatService.CreateChat(_receivedObject.ContentObject, _receivedObject.ChatRoomId);
            return result[0].Member;
        }
        //如果是requireMessage代表送來的訊息是單純要求更新聊天室
        else 
            return new List<string> { _receivedObject.ContentObject };
     
    }
}
