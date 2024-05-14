using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure;

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
          #region Snippet:ChatTools:DefineTool
          var getTimeTool = new ChatCompletionsFunctionToolDefinition()
          {
              Name = "get_current_time",
              Description = "Get the current time",
              Parameters = BinaryData.FromObjectAsJson(
              new
              {
                  Type = "object",
                  Properties = new
                  {
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
          #endregion


            Uri azureOpenAIResourceUri = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_RESOURCE_URI"));
            AzureKeyCredential azureOpenAIApiKey = new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));
            OpenAIClient client = new OpenAIClient(azureOpenAIResourceUri, azureOpenAIApiKey);
            // Console.WriteLine(Message);
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
                Tools = { getTimeTool },
            };

            Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
            ChatResponseMessage responseMessage = response.Value.Choices[0].Message;
            // Console.WriteLine($"[{responseMessage.Role.ToString().ToUpperInvariant()}]: {responseMessage.Content}");


            // Purely for convenience and clarity, this standalone local method handles tool call responses.
            ChatRequestToolMessage GetToolCallResponseMessage(ChatCompletionsToolCall toolCall)
            {
                var functionToolCall = toolCall as ChatCompletionsFunctionToolCall;
                if (functionToolCall?.Name == getTimeTool.Name)
                {
                    // Validate and process the JSON arguments for the function call
                    string unvalidatedArguments = functionToolCall.Arguments;
                    var functionResultData = (object)null; // GetYourFunctionResultData(unvalidatedArguments);
                    // Here, replacing with an example as if returned from "GetYourFunctionResultData"
                    functionResultData = DateTime.Now.ToString();
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




