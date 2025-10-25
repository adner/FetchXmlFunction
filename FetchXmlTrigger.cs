using Azure.Core.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Nullpointer.Functions;

public class FetchXmlTrigger
{
    private readonly ILogger<FetchXmlTrigger> _logger;
    private IOrganizationService _serviceClient;

    public FetchXmlTrigger(ILogger<FetchXmlTrigger> logger, IOrganizationService serviceClient)
    {
        _logger = logger;
        _serviceClient = serviceClient;
    }

    [Function("FetchXmlTrigger")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string body = "";
        if (req.Method == "POST")
        {
            using (StreamReader reader = new StreamReader(req.Body))
            {
                body = await reader.ReadToEndAsync();
            }
        }

        if (!string.IsNullOrEmpty(body))
        {
            try
            {
                var result = _serviceClient.RetrieveMultiple(new Microsoft.Xrm.Sdk.Query.FetchExpression(body));

                // Process the results and create KnowledgeBaseResult objects
                var knowledgeBaseResults = new List<KnowledgeBaseResult>();
                
                foreach (var entity in result.Entities)
                {
                    var knowledgeResult = new KnowledgeBaseResult();
                    
                    // Concatenate all attribute values for Content
                    var contentValues = new List<string>();
                    foreach (var attribute in entity.Attributes)
                    {
                        if (attribute.Value != null)
                        {
                            string objectString = string.Empty;
                            if (attribute.Value is AliasedValue)
                            {
                                AliasedValue aliasedValue = (AliasedValue)attribute.Value;

                                if (aliasedValue.Value is Money)
                                {
                                    objectString = attribute.Key + ":" + ((Money)aliasedValue.Value).Value;
                                }
                                else { 
                                    objectString = attribute.Key + ":" + ((AliasedValue)attribute.Value).Value;
                                }                          
                            }
                            else
                            {
                                objectString = attribute.Key + ":" + attribute.Value;
                            }                             

                            contentValues.Add(objectString ?? string.Empty);
                        }
                    }
                    knowledgeResult.Content = string.Join(", ", contentValues);
                    
                    // Build ContentLocation URL
                    var logicalName = entity.LogicalName;
                    var id = entity.Id.ToString();

                    // Update this to reflect your environment.
                    knowledgeResult.ContentLocation = $"https://org41df0750.crm4.dynamics.com/main.aspx?appid=6605cbc2-a674-f011-b4cc-000d3ab25cc7&forceUCI=1&pagetype=entityrecord&etn={logicalName}&id={id}";
                    
                    knowledgeBaseResults.Add(knowledgeResult);
                }

                return new OkObjectResult(knowledgeBaseResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing FetchXML query");
                return new BadRequestObjectResult($"Error executing query: {ex.Message}");
            }
        }

        return new BadRequestObjectResult("FetchXML query body is required");
    }

    public class KnowledgeBaseResult 
    {
        [JsonPropertyName("Content")]
        public string Content { get; set; } = string.Empty;
        
        [JsonPropertyName("ContentLocation")]
        public string ContentLocation { get; set; } = string.Empty;
    }
}