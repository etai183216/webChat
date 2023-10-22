using webChat.ViewModels;

namespace webChat.EntryModels
{
    public class SendEntry
    {
        public EntryType Type { get; set; } = 0;
        public string ContentObject { get; set; } = "";
        public string ChatRoomId { get; set; } = "";
    }
    
}
