using Azure;
using Azure.AI.OpenAI;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ChatApp
{
    class Program
    {
        public static List<ChatRequestMessage> ChatHistory = new List<ChatRequestMessage>();  

        static async Task Main(string[] args)
        {
            // Read the environment variable
            DotNetEnv.Env.Load("../../.env");

            // Q&A loop
            while (true)
            {
                Console.Write("Question: ");
                var Input = Console.ReadLine();
                var userMessage = new ChatRequestUserMessage(Input!);  
                ChatHistory.Add(userMessage); 
                Console.WriteLine(await ChatCompletions(Input!));

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
            };  
            
            // Add the previous chat history to the Messages list  
            foreach (var message in ChatHistory)  
            {  
                chatCompletionsOptions.Messages.Add(message);  
            } 
            
            var userMessage = new ChatRequestUserMessage(Message);  
            chatCompletionsOptions.Messages.Add(userMessage);  
            
            Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);  
            ChatResponseMessage responseMessage = response.Value.Choices[0].Message;  
        
            // Add the assistant's response to the chat history  
            ChatHistory.Add(new ChatRequestAssistantMessage(responseMessage.Content));  
        
            return responseMessage.Content;  
        }  
    }
}




