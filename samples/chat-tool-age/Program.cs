using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure;
using Azure.Identity;

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
          var getPersonAge = new ChatCompletionsFunctionToolDefinition()
          {
              Name = "get_person_age",
              Description = "Gets the age of the named person",
              Parameters = BinaryData.FromObjectAsJson(
              new
              {
                  Type = "object",
                  Properties = new
                  {
                    name = new
                    {
                        Type = "string",
                        Description = "The name of the person",
                    },
                      // Location = new
                      // {
                      //     Type = "string",
                      //     Description = "The city and state, e.g. San Francisco, CA",
                      // },
                      // Unit = new
                      // {
                      //     Type = "string",
                      //     Enum = new[] { "celsius", "fahrenheit" },
                      // }
                  },
                  // Required = new[] { "location" },
              },
              new JsonSerializerOptions() {  PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
          };



            string azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            string azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
            OpenAIClient client = new OpenAIClient(new Uri(azureOpenAIEndpoint), new AzureKeyCredential(azureOpenAIKey));

            // Console.WriteLine(Message);
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
                Tools = { getPersonAge },
            };

            Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
            ChatResponseMessage responseMessage = response.Value.Choices[0].Message;
            // Console.WriteLine($"[{responseMessage.Role.ToString().ToUpperInvariant()}]: {responseMessage.Content}");


            // Purely for convenience and clarity, this standalone local method handles tool call responses.
            ChatRequestToolMessage GetToolCallResponseMessage(ChatCompletionsToolCall toolCall)
            {
                var functionToolCall = toolCall as ChatCompletionsFunctionToolCall;
                if (functionToolCall?.Name == getPersonAge.Name)
                {
                    // Validate and process the JSON arguments for the function call
                    var unvalidatedArguments = functionToolCall.Arguments;
                    var functionResultData = (object)null; // GetYourFunctionResultData(unvalidatedArguments);
                    // Here, replacing with an example as if returned from "GetYourFunctionResultData"
                    
                    var argumentsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(functionToolCall.Arguments.ToString());
                    string name = argumentsDict["name"].ToString();
                    switch (name)
                    {
                        case "Elsa":
                            functionResultData = 21;
                            break;
                        case "Anna":
                            functionResultData = 18;
                            break;
                        default:
                            functionResultData = "Unknown";
                            break;
                    }
                    
                    return new ChatRequestToolMessage(functionResultData.ToString(), toolCall.Id);
                }
                else
                {
                    // Handle other or unexpected calls
                    throw new NotImplementedException();
                }
            }

            ChatChoice responseChoice = response.Value.Choices[0];
            if (responseChoice.FinishReason == CompletionsFinishReason.ToolCalls)
            {
                // Add the assistant message with tool calls to the conversation history
                ChatRequestAssistantMessage toolCallHistoryMessage = new(responseChoice.Message);
                chatCompletionsOptions.Messages.Add(toolCallHistoryMessage);

                // Add a new tool message for each tool call that is resolved
                foreach (ChatCompletionsToolCall toolCall in responseChoice.Message.ToolCalls)
                {
                    chatCompletionsOptions.Messages.Add(GetToolCallResponseMessage(toolCall));
                }

                // Now make a new request with all the messages thus far, including the original

                response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                responseMessage = response.Value.Choices[0].Message;
            }

            return responseMessage.Content;
        }
    }
}

