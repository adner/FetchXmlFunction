# FetchXML Azure Function for Copilot Studio

An Azure Function that executes FetchXML queries against Microsoft Dataverse and returns results in the JSON format required by Copilot Studio's custom knowledge sources.

## Purpose

This function enables you to use Dataverse data as a custom knowledge source in Copilot Studio by:

1. Accepting FetchXML queries via HTTP POST requests
2. Executing the queries against your Dataverse environment
3. Transforming the results into the specific JSON format that Copilot Studio expects for knowledge sources

This allows your copilots to retrieve and utilize dynamic data from Dataverse tables when answering user questions, leveraging Copilot Studio's `OnKnowledgeRequested` trigger.

## How It Works

When integrated with Copilot Studio's custom knowledge source feature:

1. **Copilot Studio** determines when knowledge retrieval is needed based on user queries
2. The **OnKnowledgeRequested trigger** activates and calls this Azure Function with a FetchXML query
3. The function queries **Dataverse** using the provided FetchXML
4. Results are transformed into the required format with `Content` and `ContentLocation` fields
5. **Copilot Studio** uses up to 15 snippets from the results to generate contextually relevant responses

## JSON Format

The function returns an array of knowledge base results in the following format:

```json
[
  {
    "Content": "attribute1:value1, attribute2:value2, ...",
    "ContentLocation": "https://your-org.dynamics.com/main.aspx?...&etn=entityname&id=guid"
  }
]
```

- **Content**: A concatenated string of all attribute key-value pairs from the Dataverse entity
- **ContentLocation**: A deep link to the specific record in your Dataverse environment

## Configuration

The function uses dependency injection for the `IOrganizationService` client. Configure your Dataverse connection in `appsettings.json` or Azure Function application settings.

## Learn More

For a detailed guide on implementing custom knowledge sources in Copilot Studio, see:
[Custom Knowledge Sources in Copilot Studio](https://microsoft.github.io/mcscatblog/posts/copilot-studio-custom-knowledge-source/)

## Usage

Send a POST request to the function endpoint with a FetchXML query in the request body:

```xml
<fetch top="50">
  <entity name="account">
    <attribute name="name" />
    <attribute name="accountnumber" />
  </entity>
</fetch>
```

The function will return JSON-formatted results ready for consumption by Copilot Studio.
