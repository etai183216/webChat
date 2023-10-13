namespace webChat.EntryModels
{
    public class CreateChatRoomEntry
    {
        public string ChatRoomName { get; set; } = "";
        public List<string> members { get; set; } = new List<string>();
    }
}
