using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Text;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AzdLibrary;

#pragma warning disable SKEXP0001, SKEXP0003, SKEXP0010, SKEXP0050, SKEXP0055 // Experimental

// Read the environment variable
AzdEnvironment.LoadEnvVariables();

string OPENAI_HOST = Environment.GetEnvironmentVariable("OPENAI_HOST")!;

// Initialize the kernel
Kernel kernel;
ISemanticTextMemory memory;
if (OPENAI_HOST == "azure"){
    IKernelBuilder kb = Kernel.CreateBuilder();
    kb.AddAzureOpenAIChatCompletion(Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!);
    kb.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    kb.Services.ConfigureHttpClientDefaults(c => c.AddStandardResilienceHandler());
    kernel = kb.Build();
    
    memory = new MemoryBuilder()
        .WithLoggerFactory(kernel.LoggerFactory)
        .WithMemoryStore(new VolatileMemoryStore())
        .WithAzureOpenAITextEmbeddingGeneration(Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDING_MODEL")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!)
        .Build();
}
else{
    IKernelBuilder kb = Kernel.CreateBuilder();
    kb.AddOpenAIChatCompletion(Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!);
    kb.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    kb.Services.ConfigureHttpClientDefaults(c => c.AddStandardResilienceHandler());
    kernel = kb.Build();

    memory = new MemoryBuilder()
        .WithLoggerFactory(kernel.LoggerFactory)
        .WithMemoryStore(new VolatileMemoryStore())
        .WithOpenAITextEmbeddingGeneration(Environment.GetEnvironmentVariable("OPENAI_EMBEDDING_MODEL")!, Environment.GetEnvironmentVariable("OPENAI_API_KEY")!)
        .Build();
}

IList<string> collections = await memory.GetCollectionsAsync();
string collectionName = "net7perf";
if (collections.Contains(collectionName))
{
    Console.WriteLine("Found database");
}
else
{
    using HttpClient client = new();
    string s = await client.GetStringAsync("https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7");
    List<string> paragraphs =
        TextChunker.SplitPlainTextParagraphs(
            TextChunker.SplitPlainTextLines(
                WebUtility.HtmlDecode(Regex.Replace(s, @"<[^>]+>|&nbsp;", "")),
                128),
            1024);
    // Since the default sku of OpenAI is S0, only the first five paragraphs are loaded here.
    // If you want to load all paragraphs, please create a higher level sku OpenAI.
    // for (int i = 0; i < paragraphs.Count; i++)
    for (int i = 0; i < 5; i++)
    {
        // sleep for a bit to avoid rate limiting
        await Task.Delay(TimeSpan.FromSeconds(3)); 
        await memory.SaveInformationAsync(collectionName, paragraphs[i], $"paragraph{i}");
    }
        
    Console.WriteLine("Generated database");
}

// Create a new chat
var ai = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory chat = new("You are an AI assistant that helps people find information.");
StringBuilder builder = new();

// Q&A loop
while (true)
{   
    Console.WriteLine("Since the default sku of OpenAI is S0, only the first five paragraphs are loaded here. If you want to load all paragraphs, please create a higher level sku OpenAI.");
    Console.Write("Question: ");
    string question = Console.ReadLine()!;

    builder.Clear();
    await foreach (var result in memory.SearchAsync(collectionName, question, limit: 3))
        builder.AppendLine(result.Metadata.Text);

    int contextToRemove = -1;
    if (builder.Length != 0)
    {
        builder.Insert(0, "Here's some additional information: ");
        contextToRemove = chat.Count;
        chat.AddUserMessage(builder.ToString());
    }

    chat.AddUserMessage(question);

    builder.Clear();
    await foreach (var message in ai.GetStreamingChatMessageContentsAsync(chat))
    {
        Console.Write(message);
        builder.Append(message.Content);
    }
    Console.WriteLine();
    chat.AddAssistantMessage(builder.ToString());

    if (contextToRemove >= 0) chat.RemoveAt(contextToRemove);
    Console.WriteLine();
}