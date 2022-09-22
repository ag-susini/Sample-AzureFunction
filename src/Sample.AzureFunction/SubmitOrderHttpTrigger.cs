using System;
using System.Net;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Sample.AzureFunction;

public  class SubmitOrderHttpTrigger
{
    private readonly IPublishEndpoint _publishEndpoint;
    public SubmitOrderHttpTrigger(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
    
    [Function("SubmitOrderHttpTrigger")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function,  "post")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("SubmitOrderHttpTrigger");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        await _publishEndpoint.Publish<SubmitOrder>(new { OrderId = Guid.NewGuid(), OrderNumber = "ORD00001" });

        return response;
    }
}