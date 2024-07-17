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
Select any sample you want to run.

```bash
cd src/samples/...

dotnet run
```
Typing out questions and getting answers back from the service.