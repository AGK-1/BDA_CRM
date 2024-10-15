using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BDA.EmailSender
{
    public class IEmailSender_s
    {
        private readonly RequestDelegate _next;
        private readonly List<string> _allowedIPs;

        public IEmailSender_s(RequestDelegate next, List<string> allowedIPs)
        {
            _next = next;
            _allowedIPs = allowedIPs;
        }

        public async Task Invoke(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();

            if (!_allowedIPs.Contains(remoteIp))
            {
                context.Response.StatusCode = 403; // Доступ запрещён
                await context.Response.WriteAsync("ERROR INVALID HANDLE \n 0x0007217272_72 " +
                    "\n ERROR PATH NOT FOUND \n 訪問被拒絕消失 茂木走開。\n" +
                    "Cút đi Motegi. What is a failure code?");
                return;
            }

            await _next(context);
        }
    }
}

