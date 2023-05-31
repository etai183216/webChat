using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;
using webChat.Models;
using webChat.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<MongoDBSetting>(builder.Configuration.GetSection("chatDB"));
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<UserServices>();
// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options => 
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:KEY"]))
        };
    })  
    ;

builder.Services.AddMvc(options => { options.Filters.Add(new AuthorizeFilter()); });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();

//註冊webSocket
var webSocketOptions = new WebSocketOptions
{
    //Proxy 保持連線開啟的頻率。 預設為兩分鐘
    KeepAliveInterval = TimeSpan.FromMinutes(2)
    //WebSocket 要求之允許 Origin 標頭值的清單
    //AllowedOrigins 
};
app.UseWebSockets(webSocketOptions);
app.UseCors();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
