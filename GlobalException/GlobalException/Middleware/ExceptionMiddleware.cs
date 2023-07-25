using System.IO;
using System.Net;
using System.Text.Json;

namespace GlobalException.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, ex.Message);
                await HandleExceptionAsync(httpContext, ex, _logger);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger _logger)
        {
            var serverError = HttpStatusCode.InternalServerError;
            var output = JsonSerializer.Serialize(new { error = "The Error occured" });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)serverError;

            string pathName = "GlobalException\\log.txt";// Put your file location here 
            var date = DateTime.Now;
            if (!File.Exists(pathName))
            {
                File.Create(pathName);
                TextWriter tw = File.AppendText(pathName);
                tw.WriteLine(date.ToString() + ' ' + ex.Message);
                tw.Close();
            }
            else if (File.Exists(pathName))
            {
                TextWriter tw = File.AppendText(pathName);
                tw.WriteLine(date.ToString() + ' ' + ex.Message);
                tw.Close();
            }

            return context.Response.WriteAsync(output);
        }
    }
}
