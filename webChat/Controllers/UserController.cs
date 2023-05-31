using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using webChat.EntryModels;
using webChat.Services;

namespace webChat.Controllers;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class UserController:ControllerBase
{
    private readonly  UserServices _userServices;
    public UserController(UserServices userServices) => _userServices = userServices;

    [HttpPost("/login")]
    public async Task<string> UserLogin(LoginEntry _loginEntry) 
    {
        if (_loginEntry == null || _loginEntry.Account == "" || _loginEntry.Password == "") return "";
        
        return await _userServices.UserLogin(_loginEntry.Account, _loginEntry.Password);
        
    }

    
}

