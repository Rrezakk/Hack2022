using Hack2022.Services;

namespace Hack2022
{
    public class UserKeyValidatorsMiddleware
    {
        private readonly RequestDelegate _next;
        public UserKeyValidatorsMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context,UserAccountsService service)
        {
            await _next.Invoke(context);
            return;
            if (context.Request.Path.ToString().Contains("/auth/") )//access to everybody for authorization
            {
                await _next.Invoke(context);
                return;
            }
            

            if (!context.Request.Headers.Keys.Contains("user-key"))
            {
                context.Response.StatusCode = 400; //Bad Request                
                await context.Response.WriteAsync("User Key is missing");
                return;
            }
            else
            {
                var token = context.Request.Headers["user-key"];
                if (service.TryGetUserByToken(token, out var user))
                {
                    if (user.CheckAccess("moderator"))
                    {
                        //ok - additional features
                    }
                    await _next.Invoke(context);
                    return;
                }
                else
                {
                    context.Response.StatusCode = 401; //UnAuthorized
                    await context.Response.WriteAsync("Invalid User Key");
                    return;
                }
            }
            await _next.Invoke(context);
        }
    }
    public static class UserKeyValidatorsExtension
    {
        public static IApplicationBuilder ApplyUserKeyValidation(this IApplicationBuilder app)
        {
            app.UseMiddleware<UserKeyValidatorsMiddleware>();
            return app;
        }
    }
}
