using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using LoadEnvVariables;

// Read the environment variable
var env = new AzureEnvManager();
env.LoadEnvVariables();

string OPENAI_HOST = Environment.GetEnvironmentVariable("OPENAI_HOST")!;

// Initialize the kernel
Kernel kernel;
if (OPENAI_HOST == "azure"){
    kernel = Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!)
        .Build();
}
else{
    kernel = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(Environment.GetEnvironmentVariable("OPENAI_MODEL")!,Environment.GetEnvironmentVariable("OPENAI_API_KEY")!)
        .Build();
}

// Create a new chat
IChatCompletionService ai = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory chat = new("You are an AI assistant that helps people find information.");
StringBuilder builder = new();

// Download a document and add all of its contents to our chat
using (HttpClient client = new())
{
    string s = await client.GetStringAsync("https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7");
    s = WebUtility.HtmlDecode(Regex.Replace(s, @"<[^>]+>|&nbsp;", ""));
    chat.AddUserMessage("Here's some additional information: " + s); // uh oh!
}

// Q&A loop
while (true)
{
    Console.Write("Question: ");
    chat.AddUserMessage(Console.ReadLine()!);

    builder.Clear();
    await foreach (var message in ai.GetStreamingChatMessageContentsAsync(chat))
    {
        Console.Write(message);
        builder.Append(message.Content);
    }
    Console.WriteLine();
    chat.AddAssistantMessage(builder.ToString());

    Console.WriteLine();
}