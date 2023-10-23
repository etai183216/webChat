

namespace webChat
{
    public class MyEnum
    {
        public enum EntryType : int
        {
            SendingMessage = 0,
            RequiredAllMessage = 1,
        }

        public enum ApiStatusCode : int
        { 
            Success = 0,
            Error = 1,
        }
    }
}
