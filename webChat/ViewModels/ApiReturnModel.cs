namespace webChat.ViewModels
{
    public class ApiReturnModel
    {
        public MyEnum.ApiStatusCode status { get; set; } = MyEnum.ApiStatusCode.Error;
        public string contentObject { get; set; } = "";
    }
}
