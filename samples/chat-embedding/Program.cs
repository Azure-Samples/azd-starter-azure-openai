using Azure;
using Azure.Identity;
using Azure.AI.OpenAI;

using MathNet.Numerics.LinearAlgebra; 

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

            var embedding = await ChatCompletions("What is an amphibian?");
            string[] examples =
            {
                "What is an amphibian?",
                "Cos'è un anfibio?",
                "A frog is an amphibian.",
                "Frogs, toads, and salamanders are all examples.",
                "Amphibians are four-limbed and ectothermic vertebrates of the class Amphibia.",
                "They are four-limbed and ectothermic vertebrates.",
                "A frog is green.",
                "A tree is green.",
                "It's not easy bein' green.",
                "A dog is a mammal.",
                "A dog is a man's best friend.",
                "You ain't never had a friend like me.",
                "Rachel, Monica, Phoebe, Joey, Chandler, Ross",
            };
            Vector<float> vector1 = CreateVector.DenseOfArray(embedding);

            for (int i = 0; i < examples.Length; i++)
            {
                var embeddingCompare = await ChatCompletions(examples[i]);
                Vector<float> vector2 = CreateVector.DenseOfArray(embeddingCompare);
                float cosineSimilarity = (float)(vector1.DotProduct(vector2) / (vector1.L2Norm() * vector2.L2Norm()));
                Console.WriteLine($"{cosineSimilarity:F6}  {examples[i]}");
            }

        }
        
        static async Task<float[]> ChatCompletions(string Message)
        {
            string azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            string azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
            OpenAIClient client = new OpenAIClient(new Uri(azureOpenAIEndpoint), new AzureKeyCredential(azureOpenAIKey));

            EmbeddingsOptions embeddingsOptions = new()
            {
                DeploymentName = "TextEmbedding3",
                Input = { Message },
            };
            Response<Embeddings> response = await client.GetEmbeddingsAsync(embeddingsOptions);

            // The response includes the generated embedding.
            EmbeddingItem item = response.Value.Data[0];
            ReadOnlyMemory<float> embedding = item.Embedding;
            float[] embeddingArray = embedding.ToArray();
            return embeddingArray;
        }
    }
}




