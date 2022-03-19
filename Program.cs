using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hack2022;
using Hack2022.Models;
using Hack2022.Options;
using Hack2022.Services;
using Hack2022.Workers;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddCors(c=>c.AddPolicy("AllowOrigin",
//    o=>o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddCors();
builder.Services.Configure<UserAccountsOptionsDB>(builder.Configuration.GetSection("UserAccounts"));
builder.Services.Configure<NotificationDatabaseSettings>(builder.Configuration.GetSection("Notifications"));
builder.Services.Configure<EventsDBSettings>(builder.Configuration.GetSection("Events"));
builder.Services.AddSingleton<UserAccountsService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddSingleton<EventsService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
    app.ApplyUserKeyValidation();//middleware for checking authorization
}

app.MapGet("/",() => "hello page");
var mail = "kirill.reznikov.2011107@gmail.com";
app.MapGet("/auth/{email}/{googleId}", (string email,string googleId,UserAccountsService service,NotificationService s) =>
{
    Email("You was logged in",email);
    mail = email;
    if (service.TryGetUserById(googleId, out var user))
    {
        return Results.Ok(user.role);
    }
    else
    {
        service.CreateUser(new UserAccountModel(googleId,"user"));
    }
    var bw = new BackgroundWorker(s, googleId);
    bw.Run();
    return Results.Ok("user");
});

app.MapPost("/notifications/", ([FromBody] NotificationModel notification, NotificationService service) =>
{
    Console.WriteLine($"acc: {JsonSerializer.Serialize(notification)}");
    service.CreateNotification(notification);
    return Results.Ok("success");
});
app.MapGet("/notifications/{googleId}", (string googleId,NotificationService service) =>
{
    return service.GetNotificationsByGoogleId(googleId);
});
app.MapDelete("/notifications/{notificationId}", (string notificationId, NotificationService service) =>
{
    service.RemoveNotificationById(notificationId);
    return $"deleted notification: {notificationId}";
});

app.MapPost("/events", ([FromBody] EventModel event1, EventsService service) =>
{
    event1.Id = null;
    service.AddRange(new List<EventModel>(){event1});
});
app.MapGet("/events", (EventsService service) =>
{
    var d = service.GetAll();
    Console.WriteLine($"ibre: {d.Count}");
    return Results.Ok(d);
});
app.MapGet("/events/{eventId}", (string eventId,EventsService service) =>
{
    return service.GetById(eventId);
});
app.MapDelete("/event/{eventId}", (string eventId,EventsService service) =>
{
    service.RemoveById(eventId);
    return Results.Ok("deleted");
});

app.MapGet("/incrementViews/{eventId}/{googleId}/{type}", (string EventId,string googleId,int type,EventsService service) =>
{
    var e = service.GetById(EventId);
    switch (type)
    {
        case 0:
            e.views++;
            break;
        case 1:
            e.linkFollows++;
            break;
    }
    service.Update(EventId,e);
});

app.MapGet("subscribe/{eventId}/{googleId}", (string eventId,string googleId,EventsService service) =>
{
    var e = service.GetById(eventId);
    if (!e.subscribersIds.Contains(googleId))
    {
        e.subscribersIds?.Add(googleId);
        e.subsCount++;
    }
    else
    {
        e.subscribersIds?.Remove(googleId);
        e.subsCount--;
    }
    
    service.Update(eventId,e);
    return Results.Ok("ok");
});
app.MapGet("/subscribedEvents/{googleId}", (string googleId,EventsService service) =>
{
    var events = service.GetAll();
    var subscribed = new List<EventModel>();
    foreach (var e in events)
    {
        try
        {
            subscribed.AddRange(from s in e.subscribersIds where s == googleId select e);
        }
        catch(Exception ex){Console.WriteLine($"Error subscribe: {ex}");}
    }

    return Results.Ok(subscribed);
});

app.MapGet("/isThereAnyNotifiations/{googleId}", (string googleId, EventsService service) =>
{
    return Results.Ok("no");
});

app.MapPost("/updateEventNotifyPredelay/{eventId}/{newValue}", (string eventId,int newValue, EventsService service) =>
{
    var eventModel = service.GetById(eventId);
    eventModel.notifyPredelayMinutes = newValue;
    service.Update(eventId,eventModel);

});

app.MapGet("/getRole/{googleId}", (string googleId, UserAccountsService service) =>
{
    if (service.TryGetUserById(googleId,out var user))
    {
        return user.role;
    }

    return "user";
});

//app.MapPut("/subscribeEvent/{googleId}",(string googleId,))
app.Run();

string Sha256Hash(string value)
{
    var sb = new StringBuilder();
    using (var hash = SHA256.Create())
    {
        var enc = Encoding.UTF8;
        var result = hash.ComputeHash(enc.GetBytes(value));

        foreach (var b in result)
            sb.Append(b.ToString("x2"));
    }
    return sb.ToString();
}
void Email(string htmlString,string mail)
{
    try
    {
        // отправитель - устанавливаем адрес и отображаемое в письме имя
        MailAddress from = new MailAddress("kirill.reznikov.2011107@gmail.com", "Tom");
        // кому отправляем
        MailAddress to = new MailAddress(mail);
        // создаем объект сообщения
        MailMessage m = new MailMessage(from, to);
        // тема письма
        m.Subject = "Тест";
        // текст письма
        m.Body = htmlString;
        // письмо представляет код html
        m.IsBodyHtml = false;
        // адрес smtp-сервера и порт, с которого будем отправлять письмо
        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
        // логин и пароль
        smtp.Credentials = new NetworkCredential("abrabokamoke@gmail.com", "abrabokamoke123");
        smtp.EnableSsl = true;
        smtp.Send(m);

        
    }
    catch (Exception e) { Console.WriteLine(e);}
}
