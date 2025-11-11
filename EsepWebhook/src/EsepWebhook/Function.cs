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
    dynamic deserialize = JsonConvert.DeserializeObject(input.Body);

    string payload = JsonConvert.SerializeObject(new { text = $"Issue Created: {deserialize.issue.html_url}" });

    var client = new HttpClient();

    var environmentVariable = Environment.GetEnvironmentVariable("SLACK_URL");
    var content = new StringContent(payload, Encoding.UTF8, "application/json");

    var response = await client.PostAsync(environmentVariable, content);

    var responseBody = await response.Content.ReadAsStringAsync();

    context.Logger.LogLine($"Slack Response: {responseBody}");

    return new APIGatewayProxyResponse
    {
        StatusCode = (int)response.StatusCode,
        Body = responseBody,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    };
}

}