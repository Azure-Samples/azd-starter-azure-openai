﻿using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
// Read the environment variable
DotNetEnv.Env.Load("../../.env");

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

// Q&A loop
while (true)
{
    Console.Write("Question: ");
    chat.AddUserMessage(Console.ReadLine()!);

    builder.Clear();
    await foreach (StreamingChatMessageContent message in ai.GetStreamingChatMessageContentsAsync(chat))
    {
        Console.Write(message);
        builder.Append(message.Content);
    }
    Console.WriteLine();
    chat.AddAssistantMessage(builder.ToString());

    Console.WriteLine();
}
