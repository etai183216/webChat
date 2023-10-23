using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using webChat.EntryModels;
using webChat.Services;
using webChat.ViewModels;

namespace webChat.Controllers;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class UserController:ControllerBase
{
    private readonly  UserServices _userServices;
    public UserController(UserServices userServices) => _userServices = userServices;

    [AllowAnonymous]
    [HttpPost("/login")]
    public async Task<ApiReturnModel> UserLoginAsync(LoginEntry _loginEntry) 
    {
        ApiReturnModel resObj =  new ApiReturnModel();
        if (_loginEntry == null || _loginEntry.Account == "" || _loginEntry.Password == "") return resObj;
        
        resObj.contentObject = await _userServices.UserLoginGetToken(_loginEntry.Account, _loginEntry.Password);

        resObj.status = resObj.contentObject == "" ? MyEnum.ApiStatusCode.Error : MyEnum.ApiStatusCode.Success;

        return resObj;
        
    }

    
}

