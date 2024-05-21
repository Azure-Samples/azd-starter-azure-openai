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


// Create the prompt function as part of a plugin and add it to the kernel.
// These operations can be done separately, but helpers also enable doing
// them in one step.
kernel.ImportPluginFromFunctions("DateTimeHelpers",
[
    kernel.CreateFunctionFromMethod(() => $"{DateTime.UtcNow:r}", "Now", "Gets the current date and time")
]);

KernelFunction qa = kernel.CreateFunctionFromPrompt("""
    The current date and time is {{ datetimehelpers.now }}.
    {{ $input }}
    """);

// Q&A loop
var arguments = new KernelArguments();
while (true)
{
    Console.Write("Question: ");
    arguments["input"] = Console.ReadLine();
    Console.WriteLine(await qa.InvokeAsync(kernel, arguments));
    Console.WriteLine();
}