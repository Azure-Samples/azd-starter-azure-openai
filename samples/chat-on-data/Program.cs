using Azure;
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
            #region Snippet:StreamChatMessages
            Uri azureOpenAIResourceUri = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_RESOURCE_URI"));
            AzureKeyCredential azureOpenAIApiKey = new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));
            OpenAIClient client = new OpenAIClient(azureOpenAIResourceUri, azureOpenAIApiKey);

            
            AzureSearchChatExtensionConfiguration contosoExtensionConfig = new()
            {
                SearchEndpoint = new Uri("https://your-contoso-search-resource.search.windows.net"),
                Authentication = new OnYourDataApiKeyAuthenticationOptions("<your Cognitive Search resource API key>"),
            };


            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-4", // Use DeploymentName for "model" with non-Azure clients
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
            using (HttpClient httpClient = new())
            {
                string s = await httpClient.GetStringAsync("https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7");
                s = WebUtility.HtmlDecode(Regex.Replace(s, @"<[^>]+>|&nbsp;", ""));
                chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Here's some additional information: " + s)); // uh oh!
            }
            await foreach (StreamingChatCompletionsUpdate chatUpdate in client.GetChatCompletionsStreaming(chatCompletionsOptions))
            {
                if (chatUpdate.Role.HasValue)
                {
                    Console.Write($"{chatUpdate.Role.Value.ToString().ToUpperInvariant()}: ");
                }
                if (!string.IsNullOrEmpty(chatUpdate.ContentUpdate))
                {
                    Console.Write(chatUpdate.ContentUpdate);
                }
            }
            #endregion
        }
    }
}




