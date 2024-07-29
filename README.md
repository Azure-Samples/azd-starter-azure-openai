# Demystifying Retrieval Augmented Generation with .NET

Focuses on building a simple console-based .NET chat application from the ground up, with minimal dependencies and minimal fuss. For more details, please refer to the original RAG blog: https://devblogs.microsoft.com/dotnet/demystifying-retrieval-augmented-generation-with-dotnet/.

## Prerequisites

- [Azure Developer CLI](https://aka.ms/azd-install)
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

## Authenticate with Azure

Make sure AZD CLI can access Azure resources. You can use the following command to log in to Azure:

```bash
azd auth login
```

Make sure AZ CLI can access Azure resources (in hooks). You can use the following command to log in to Azure:

```bash
az login
```

## Initialize the template

Then, execute the `azd init` command to initialize the environment (You do not need to run this command if you already have the code or have opened this in a Codespace or DevContainer).

```bash
azd init -t Azure-Samples/demystifying-rag-dotnet
```

Enter an environment name.

## Deploy the sample

Run `azd up` to provision all the resources to Azure.

```bash
azd up 
```

Select your desired `subscription` and `location`. Wait a moment for the resource deployment to complete.

**Note**: Make sure to pick a region where all services are available like, for example, *East US 2*

## How to run samples.
Select any sample you want to run. Such as `01_ChatSimple`:

```bash
cd src/samples/01_ChatSimple

dotnet run
```
Typing out questions and getting answers back from the service.

**Notes:** 
- For sample `07_ChatLogging` and `08_ChatLoggingSqlLite`, since the default sku of `OpenAI` is `S0`, only the first five paragraphs are loaded. If you want to load all paragraphs, please create a higher level sku OpenAI.

```csharp
// for (int i = 0; i < paragraphs.Count; i++)
for (int i = 0; i < 5; i++)
{
    await memory.SaveInformationAsync(collectionName, paragraphs[i], $"paragraph{i}");
}
```