### 語言
```
這是後端伺服器，使用C#。(前端伺服器請參考chatchat)
```
### 主要功能
```
線上1對1聊天室的後端，用來練習mongoDB、Websocket、Token...等技術
```
### 資料庫結構
```
只有UserList及ChatList兩個collection，顧名思義就是用來記錄聊天內容及使用者帳號密碼而已。

```

### ChatList結構
```
[{
  "_id": {
    "$oid": "64647d084ade8042e379fe53"
  },
  "member": [
    "etai183216",
    "etai123123123"
  ],
  "chat": [
    {
      "_id": {
        "$oid": "646741bc4957ff4e883b2d95"
      },
      "chatTime": {
        "$date": "2023-05-19T09:30:33.477Z"
      },
      "sender": "etai183216",
      "content": "41414"
    }],
  "updateTime": {
    "$date": "2023-05-31T01:01:11.166Z"
  }
}]
```

### UserList結構
```
[{
  "_id": {
    "$oid": "646d70744edeae0b25165fa9"
  },
  "account": "etai183216",
  "pw": "112233"
},{
  "_id": {
    "$oid": "646d71684edeae0b25165fae"
  },
  "account": "hahaha12345",
  "pw": "112233"
}]
```
