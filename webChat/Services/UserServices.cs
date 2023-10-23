using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using webChat.Models;

namespace webChat.Services
{
    public class UserServices
    {
        private readonly IMongoCollection<UserModel> _userCollection;
        private readonly IConfiguration _configuration;

        //伺服器與mongoDB連線，並生成_chatCollection備用
        public UserServices(IOptions<MongoDBSetting> mongoDBSetting,IConfiguration configuration)
        {
            var mongoClient = new MongoClient(mongoDBSetting.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSetting.Value.DatabaseName);
            _userCollection = mongoDatabase.GetCollection<UserModel>(mongoDBSetting.Value.UserCollectionName);
            _configuration = configuration;
        }

        //登入時Controller會呼叫這個function進行驗證
        public async Task<string> UserLoginGetToken(string _account,string _pw) 
        {
            //用帳號去資料表搜尋有沒有這個會員，如果有的話就撈出來
            var data = await _userCollection.Find(x => x.Account==_account).ToListAsync();

            //如果data沒有先確認有沒有內容就去拿[0]會報錯。
            if (!data.Any()) return String.Empty;
            else
            {
                if (data[0].Password != _pw) return String.Empty;//字串
            }

            var claims = new List<Claim>
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name, _account),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:KEY"]));

            var jwt = new JwtSecurityToken
            (
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires:DateTime.Now.AddMinutes(30),
                signingCredentials:new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256)
            ) ;

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return token;
        }
    }
}
