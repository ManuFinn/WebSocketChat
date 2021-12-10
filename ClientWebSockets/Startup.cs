using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientWebSockets
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            var wsOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(120) };
            app.UseWebSockets(wsOptions);
            app.Run(async (context) =>
            {
                if (context.Request.Path == "/Send")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using WebSocket web = await context.WebSockets.AcceptWebSocketAsync();
                        await Send(context, web);
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
            });
        }

        private async Task Send(HttpContext context, WebSocket webSock)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSock.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
            if (result != null)
            {
                while (!result.CloseStatus.HasValue)
                {
                    string msg = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, result.Count));
                    Console.WriteLine($"Client says:{msg}");
                    await webSock.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Server says:{DateTime.UtcNow:f} ")),
                        result.MessageType,
                        result.EndOfMessage,
                        System.Threading.CancellationToken.None);
                    result = await webSock.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
                    //Console.WriteLine(result);
                }
            }
            await webSock.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, System.Threading.CancellationToken.None);
        }
    }
}
