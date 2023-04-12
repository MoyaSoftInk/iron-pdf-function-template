using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace IronPdfExample
{
    public class TemplateFunction
    {
        private readonly ILogger _logger;

        public TemplateFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TemplateFunction>();
        }

        [Function("GetTemplate")]
        public HttpResponseData GetTemplate([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
