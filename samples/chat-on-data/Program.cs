﻿using Azure;
using Azure.Identity;
using Azure.AI.OpenAI;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Read the environment variable
            DotNetEnv.Env.Load("../../.env");
            
            // Q&A loop
            while (true)
            {
                Console.Write("Question: ");
                await StreamingChatWithData(Console.ReadLine()!);
                Console.WriteLine();
            }
        }

        static async Task StreamingChatWithData(string Message)
        {
            string azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            string azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
            OpenAIClient client = new OpenAIClient(new Uri(azureOpenAIEndpoint), new AzureKeyCredential(azureOpenAIKey));

            AzureSearchChatExtensionConfiguration contosoExtensionConfig = new()
            {
                SearchEndpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT")),
                Authentication = new OnYourDataApiKeyAuthenticationOptions(Environment.GetEnvironmentVariable("AZURE_SEARCH_KEY")),
                IndexName = "index",
            };


            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "Gpt35Turbo_0301", // Use DeploymentName for "model" with non-Azure clients
                Messages =
                {
                    new ChatRequestSystemMessage("You are an AI assistant that helps people find information."),
                    new ChatRequestUserMessage("Can you help me?"),
                    new ChatRequestAssistantMessage("Of course, I'd be happy to help. What can I do for you?"),
                    new ChatRequestUserMessage(Message),
                },

                AzureExtensionsOptions = new AzureChatExtensionsOptions()
                {
                    Extensions = { contosoExtensionConfig }
                }
            };
            // Download a document and add all of its contents to our chat
            // using (HttpClient httpClient = new())
            // {
            //     string s = await httpClient.GetStringAsync("https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7");
            //     s = WebUtility.HtmlDecode(Regex.Replace(s, @"<[^>]+>|&nbsp;", ""));
            //     chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Here's some additional information: " + s)); // uh oh!
            // }
            // await foreach (StreamingChatCompletionsUpdate chatUpdate in client.GetChatCompletionsStreaming(chatCompletionsOptions))
            // {
            //     if (chatUpdate.Role.HasValue)
            //     {
            //         Console.Write($"{chatUpdate.Role.Value.ToString().ToUpperInvariant()}: ");
            //     }
            //     if (!string.IsNullOrEmpty(chatUpdate.ContentUpdate))
            //     {
            //         Console.Write(chatUpdate.ContentUpdate);
            //     }
            // }
            Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
            ChatResponseMessage message = response.Value.Choices[0].Message;

            // The final, data-informed response still appears in the ChatMessages as usual
            Console.WriteLine($"{message.Role}: {message.Content}");

            // Responses that used extensions will also have Context information to explain extension activity
            // and provide supplemental information like citations.
            if (message.AzureExtensionsContext.Intent != "[]")
            {
                Console.WriteLine($"Citations and other information:");
                foreach (AzureChatExtensionDataSourceResponseCitation citation in message.AzureExtensionsContext.Citations)
                {
                    Console.WriteLine($"Citation: {citation.Content}");
                }
                Console.WriteLine($"Intent: {message.AzureExtensionsContext.Intent}");
            }

            
        }
    }
}




