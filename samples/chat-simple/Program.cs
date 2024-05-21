using Microsoft.SemanticKernel;

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


// Q&A loop
while (true)
{
    Console.Write("Question: ");
    Console.WriteLine(await kernel.InvokePromptAsync(Console.ReadLine()!));
    Console.WriteLine();
}
