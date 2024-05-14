using Azure;
using Azure.AI.OpenAI;
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

            
            #region Snippet:GenerateEmbeddings
            EmbeddingsOptions embeddingsOptions = new()
            {
                DeploymentName = "text-embedding-ada-002",
                Input = { Message },
            };
            Response<Embeddings> response = await client.GetEmbeddingsAsync(embeddingsOptions);

            // The response includes the generated embedding.
            EmbeddingItem item = response.Value.Data[0];
            ReadOnlyMemory<float> embedding = item.Embedding;
            return $"Embedding: {string.Join(", ", embedding.ToArray())}";
            #endregion
        }
    }
}




