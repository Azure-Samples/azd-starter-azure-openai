using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text;

// Read the environment variable
DotNetEnv.Env.Load("../../.env");

string OPENAI_HOST = Environment.GetEnvironmentVariable("OPENAI_HOST")!;

// Initialize the kernel
Kernel kernel;
if (OPENAI_HOST == "azure"){
    IKernelBuilder kb = Kernel.CreateBuilder();
    kb.AddAzureOpenAIChatCompletion(Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!);
    kb.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    kernel = kb.Build();
}
else{
    IKernelBuilder kb = Kernel.CreateBuilder();
    kb.AddOpenAIChatCompletion(Environment.GetEnvironmentVariable("OPENAI_MODEL")!,Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);
    kb.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    kernel = kb.Build();
}

kernel.ImportPluginFromFunctions("Demographics",
[
    kernel.CreateFunctionFromMethod(
        [Description("Gets the age of the named person")]
        ([Description("The name of a person")] string name) => name switch
        {
            "Elsa" => 21,
            "Anna" => 18,
            _ => -1,
        }, "get_person_age")
]);

// Create a new chat
var ai = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory chat = new("You are an AI assistant that helps people find information.");
StringBuilder builder = new();
OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

// Q&A loop
while (true)
{
    Console.Write("Question: ");
    chat.AddUserMessage(Console.ReadLine()!);

    builder.Clear();
    await foreach (var message in ai.GetStreamingChatMessageContentsAsync(chat, settings, kernel))
    {
        Console.Write(message);
        builder.Append(message.Content);
    }
    Console.WriteLine();
    chat.AddAssistantMessage(builder.ToString());
}
