using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.OpenAI; // For chat completions client
using OpenAI.Chat;
using Azure; // Required for ApiKeyCredential if using API keys


var endpoint = new Uri("https://firsttestkalyan-resource.cognitiveservices.azure.com/");
var model = "gpt-5.2-chat";
var deploymentName = "gpt-5.2-chat";
var apiKey = "<REDACTED>";

AzureOpenAIClient azureClient = new(
    endpoint,
    new AzureKeyCredential(apiKey));
ChatClient chatClient = azureClient.GetChatClient(deploymentName);

// Cancellation setup: Ctrl+C triggers token cancellation
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Cancellation requested - shutting down after current iteration...");
    e.Cancel = true; // prevent the process from terminating immediately
    cts.Cancel();
};

Console.WriteLine("Running loop. Press Ctrl+C to exit.");

while (!cts.IsCancellationRequested)
{
    Console.Write("Enter a prompt: ");
    string? userPrompt = Console.ReadLine();

    // If Console.ReadLine returned null (input stream closed), wait briefly and continue.
    if (userPrompt is null)
    {
        try
        {
            await Task.Delay(200, cts.Token);
        }
        catch (OperationCanceledException) { break; }
        continue;
    }

    if (string.IsNullOrWhiteSpace(userPrompt))
    {
        // ignore empty input
        continue;
    }

    var messages = new List<ChatMessage>
    {
        new SystemChatMessage("You are a helpful assistant."),
        new UserChatMessage(userPrompt)
    };

    try
    {
        // Make the call (will return when complete)
        var response = await chatClient.CompleteChatAsync(messages);
        var completion = response.Value;

        // Print first available text result safely
        var firstText = completion.Content?.FirstOrDefault()?.Text;
        Console.WriteLine($"Response: {firstText}");
    }
    catch (OperationCanceledException)
    {
        // Cancellation requested - break the loop
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

Console.WriteLine("Exited loop. Goodbye.");


