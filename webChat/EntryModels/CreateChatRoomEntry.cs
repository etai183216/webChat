namespace webChat.EntryModels
{
    public class CreateChatRoomEntry
    {
        public string userId { get; set; } = "";
        public string ChatRoomName { get; set; } = "";
        public List<string> members { get; set; } = new List<string>();
    }
}
