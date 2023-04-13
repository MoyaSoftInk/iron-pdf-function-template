using System.Net;
using System.Text.Json;
using IronPdfExample.Model;
using IronPdfExample.Query.GetTemplate;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;

namespace IronPdfExample;

public class TemplateFunction
{
    private readonly GetTemplateService _getTemplateService;

    public TemplateFunction(GetTemplateService getTemplateService)
    {
        _getTemplateService = getTemplateService;
    }

    [OpenApiOperation(operationId: "GetTemplate", tags: new[] { "TEMPLATE" }, Summary = "GetTemplate", Description = "Get the template.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(GetTemplateRequest), Deprecated = false, Description = "body request for a template and data", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PdfResponse), Summary = "The response", Description = "This returns the response")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorOutputModel), Summary = "The response", Description = "This returns the response")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorOutputModel), Summary = "The response", Description = "This returns the response")]
    [Function("GetTemplate")]
    public async Task<PdfResponse> GetTemplate([HttpTrigger(AuthorizationLevel.Function, "post", Route = "GetTemplate")] HttpRequestData req)
    {
        if (req is null)
            throw new Exception("The request can't be null");
        string requestBody;
        using (StreamReader streamReader = new StreamReader(stream: req.Body))
        {
            requestBody = await streamReader.ReadToEndAsync();
        }

        GetTemplateRequest? request = JsonSerializer.Deserialize<GetTemplateRequest>(requestBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        PdfResponse response = await _getTemplateService.Handle(request!, CancellationToken.None);
        return response;
    }
}