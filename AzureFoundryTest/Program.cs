using Azure.AI.OpenAI; // For chat completions client
using OpenAI.Chat;
using Azure; // Required for ApiKeyCredential if using API keys

var endpoint = new Uri("https://firsttestkalyan-resource.cognitiveservices.azure.com/");
var deploymentName = "gpt-5.2-chat";

// Read API key from environment variable
// Make sure to add launchsettings.json or set the variable in your OS environment
//{
//"profiles": {
//    "FoundryTest": {
//        "commandName": "Project",
//      "environmentVariables": {
//            "AZURE_OPENAI_API_KEY": "Key-Here",
//        "ASPNETCORE_ENVIRONMENT": "Development"
//      }
//    }
//  }
//}
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("Environment variable AZURE_OPENAI_API_KEY is not set. Set it and re-run.");
    Console.WriteLine("Windows (PowerShell): $env:AZURE_OPENAI_API_KEY = \"<your-key>\"");
    Console.WriteLine("Windows (cmd): set AZURE_OPENAI_API_KEY=<your-key>");
    Console.WriteLine("Linux/macOS: export AZURE_OPENAI_API_KEY=<your-key>");
    return;
}

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


