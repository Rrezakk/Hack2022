using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Timers;
using Hack2022;
using Hack2022.Models;
using Hack2022.Options;
using Hack2022.Services;
using Hack2022.Workers;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Timer = System.Threading.Timer;

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
builder.Services.AddSingleton<SearchingService>();
builder.Services.AddSingleton<BackgroundWorker>();

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
    return Results.Ok("user");
});
app.MapGet("/setNotifyState/{eventId}/{state}", (string eventId, bool state, EventsService service) =>
{
    var e = service.GetById(eventId);
    e.notifyState = state;
    service.Update(eventId, e);
    return Results.Ok($"set {state} for {eventId}");
});
app.MapPost("/notifications/", ([FromBody] NotificationModel notification, NotificationService service) =>
{
    Console.WriteLine($"acc: {JsonSerializer.Serialize(notification)}");
    service.CreateNotification(notification);
    if (notification.targetTime> DateTime.Now)
    {
        var delay = notification.targetTime - DateTime.Now;
        Task.Delay(delay).ContinueWith(t => service.ExecuteNotification(notification));
    }
    else
    {
        Task.Delay(0).ContinueWith(t => service.ExecuteNotification(notification));
    }
   

    
    //later execute
    return Results.Ok("success");
});
app.MapGet("/notifications/{googleId}", (string googleId,NotificationService service) => service.GetNotificationsByGoogleId(googleId));
app.MapDelete("/notifications/{notificationId}", (string? notificationId, NotificationService service) =>
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
    return Results.Ok(d);
});
app.MapGet("/events/{eventId}", (string eventId,EventsService service) =>
{
    return service.GetById(eventId);
});
app.MapGet("/eventsByTag/{tag}", (string tag, EventsService service) =>
{
    var events = service.GetAll();
    var res = new List<EventModel>();
    res.AddRange(events.FindAll(e => e.tags.Contains(tag)));
    return res;
});
app.MapDelete("/event/{eventId}", (string eventId,EventsService service) =>
{
    service.RemoveById(eventId);
    return Results.Ok("deleted");
});
app.MapPost("/event/{eventId}/AssignRole/{role}/{googleId}", (string role,string eventId,string googleId, EventsService service) =>
{
    Console.WriteLine($"{role} {eventId} {googleId}");
    var e = service.GetById(eventId);
    bool was = false;
    for (int i = 0; i < e.subscribersWithRoles.Count; i++)
    {
        var sub  = e.subscribersWithRoles[i];
        if(sub.googleId!=googleId)continue;
        if (sub.role == role)
        {
            return Results.Ok("already exists");
        }
        else
        {
            was = true;
            Console.WriteLine("owerriting");
            sub.role = role;
        }

        e.subscribersWithRoles[i] = sub;
    }

    if (!was)
    {
        e.subscribersWithRoles.Add(new MapSubscriberEventRole(googleId,role));
    }
    service.Update(eventId,e);
    return Results.Ok("nothing happened");

});
app.MapGet("/event/{eventId}/UsersWithRoles", (string eventId, EventsService service) =>
{
    return service.GetById(eventId).subscribersWithRoles;
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
app.MapGet("/subscribeTag/{googleId}/{tagName}", (string googleId,string tagName,NotificationService service) => 
    service.SubscribeTag(googleId, tagName));
app.MapGet("/getSubscribedTags/{googleId}",
    (string googleId, NotificationService service) =>
    {
        var tags = service.GetSubscribedTags(googleId);
        return tags != null ? Results.Ok(tags) : Results.Ok(Array.Empty<string>());
    });
app.MapGet("/getAllTags", (EventsService service) =>
{
    var events = service.GetAll().ToList();
    var tags = events.Select(x => x.tags).ToList();
    var set = new SortedSet<string>();
    foreach (var t2 in tags)
    {
        foreach (var t in t2)
        {
            set.Add(t);
        }
    }

    return set;
});
app.MapGet("/checkTagSubscription/{googleId}/{tagName}",
    (string googleId, string tagName, NotificationService service) =>
    {
        return Results.Ok(service.CheckUserSubscribedTag(googleId, tagName));
    });

app.MapGet("/isThereAnyNotifycations/{googleId}", (string googleId, EventsService service) =>
{
    return Results.Ok(new List<NotificationViewModel>(){new NotificationViewModel("ћитап на хакатоне","¬ам предстоит встреча на хакатоне сегодн€")});
});

app.MapPost("/updateEventNotifyPredelay/{eventId}/{newValue}", (string eventId,int newValue, EventsService service) =>
{
    var eventModel = service.GetById(eventId);
    eventModel.notifyPredelayMinutes = newValue;
    service.Update(eventId,eventModel);

});
app.MapGet("/getRole/{googleId}", (string googleId, UserAccountsService service) => service.TryGetUserById(googleId,out var user) ? user.role : "user");

app.MapGet("/getSearchTips/{searchString}", (string searchString, EventsService eventsService,SearchingService searchingService) =>
{
    return searchingService.GetTags(eventsService.GetAll(), searchString.ToLower());
});
app.MapGet("/getEventsBySearch/{query}", (string query,EventsService eventsService, SearchingService searchingService) =>
{
    return searchingService.Search(eventsService.GetAll(), query.ToLower());
});
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
        // отправитель - устанавливаем адрес и отображаемое в письме им€
        MailAddress from = new MailAddress("kirill.reznikov.2011107@gmail.com", "Tom");
        // кому отправл€ем
        MailAddress to = new MailAddress(mail);
        // создаем объект сообщени€
        MailMessage m = new MailMessage(from, to);
        // тема письма
        m.Subject = "“ест";
        // текст письма
        m.Body = htmlString;
        // письмо представл€ет код html
        m.IsBodyHtml = false;
        // адрес smtp-сервера и порт, с которого будем отправл€ть письмо
        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
        // логин и пароль
        smtp.Credentials = new NetworkCredential("abrabokamoke@gmail.com", "abrabokamoke123");
        smtp.EnableSsl = true;
        smtp.Send(m);

        
    }
    catch (Exception e) { Console.WriteLine(e);}
}
