using Azure;
using Azure.AI.OpenAI;
using DotNetEnv;
using System;
using System.Threading.Tasks;

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
                Console.WriteLine(await ChatCompletions(Console.ReadLine()!));
                Console.WriteLine();
          }
        }

        static async Task<string> ChatCompletions(string Message)
        {
            Uri azureOpenAIResourceUri = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_RESOURCE_URI"));
            AzureKeyCredential azureOpenAIApiKey = new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));
            OpenAIClient client = new OpenAIClient(azureOpenAIResourceUri, azureOpenAIApiKey);

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-4", // Use DeploymentName for "model" with non-Azure clients
                Messages =
                {
                    new ChatRequestSystemMessage("You are an AI assistant that helps people find information."),
                    new ChatRequestUserMessage("Can you help me?"),
                    new ChatRequestAssistantMessage("Of course, I'd be happy to help. What can I do for you?"),
                    new ChatRequestUserMessage(Message),
                }
            };
            Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
            ChatResponseMessage responseMessage = response.Value.Choices[0].Message;
            // Console.WriteLine($"[{responseMessage.Role.ToString().ToUpperInvariant()}]: {responseMessage.Content}");
            return responseMessage.Content;
        }
    }
}




