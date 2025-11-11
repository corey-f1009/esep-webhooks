using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
   public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
{
    context.Logger.LogLine($"Incoming payload: {input.Body}");

    dynamic deserialize = JsonConvert.DeserializeObject(input.Body);

    string issueUrl = deserialize?.issue?.html_url;
    context.Logger.LogLine($"Issue URL: {issueUrl}");

    string payload = JsonConvert.SerializeObject(new { text = $"Issue Created: {issueUrl}" });

    var environmentVariable = Environment.GetEnvironmentVariable("SLACK_URL");
    context.Logger.LogLine($"SLACK_URL: {environmentVariable}");

    using var client = new HttpClient();
    var content = new StringContent(payload, Encoding.UTF8, "application/json");

    var response = await client.PostAsync(environmentVariable, content);
    var responseBody = await response.Content.ReadAsStringAsync();

    context.Logger.LogLine($"Slack responded: {response.StatusCode} - {responseBody}");

    return new APIGatewayProxyResponse
    {
        StatusCode = (int)response.StatusCode,
        Body = responseBody,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    };
}


}