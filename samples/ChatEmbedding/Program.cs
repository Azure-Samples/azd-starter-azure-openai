using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Numerics.Tensors;
using LoadEnvVariables;

#pragma warning disable SKEXP0010

// Read the environment variable
var env = new AzureEnvManager();
env.LoadEnvVariables();


string OPENAI_HOST = Environment.GetEnvironmentVariable("OPENAI_HOST")!;

// Create a new chat
string input = "What is an amphibian?";
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

// Initialize the kernel

if (OPENAI_HOST == "azure"){
    var embeddingGen = new AzureOpenAITextEmbeddingGenerationService(Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDING_MODEL")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!, Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!);
    // Generate embeddings for each piece of text
    ReadOnlyMemory<float> inputEmbedding = (await embeddingGen.GenerateEmbeddingsAsync(new string[] { input }))[0];
    IList<ReadOnlyMemory<float>> exampleEmbeddings = await embeddingGen.GenerateEmbeddingsAsync(examples);

    // Print the cosine similarity between the input and each example
    float[] similarity = exampleEmbeddings.Select(e => TensorPrimitives.CosineSimilarity(e.Span, inputEmbedding.Span)).ToArray();
    similarity.AsSpan().Sort(examples.AsSpan(), (f1, f2) => f2.CompareTo(f1));
    Console.WriteLine("Similarity Example");
    for (int i = 0; i < similarity.Length; i++)
        Console.WriteLine($"{similarity[i]:F6}   {examples[i]}");
}
else{
    var embeddingGen = new OpenAITextEmbeddingGenerationService(Environment.GetEnvironmentVariable("OPENAI_EMBEDDING_MODEL")!,Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);
    // Generate embeddings for each piece of text
    ReadOnlyMemory<float> inputEmbedding = (await embeddingGen.GenerateEmbeddingsAsync(new string[] { input }))[0];
    IList<ReadOnlyMemory<float>> exampleEmbeddings = await embeddingGen.GenerateEmbeddingsAsync(examples);

    // Print the cosine similarity between the input and each example
    float[] similarity = exampleEmbeddings.Select(e => TensorPrimitives.CosineSimilarity(e.Span, inputEmbedding.Span)).ToArray();
    similarity.AsSpan().Sort(examples.AsSpan(), (f1, f2) => f2.CompareTo(f1));
    Console.WriteLine("Similarity Example");
    for (int i = 0; i < similarity.Length; i++)
        Console.WriteLine($"{similarity[i]:F6}   {examples[i]}");
}



