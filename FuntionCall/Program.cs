
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Assistants;
using System.ClientModel;
using System.Text.Json;
using System.Threading.Tasks;

IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var credential = new ApiKeyCredential(config["GitHubModels:Token"] ?? throw new InvalidOperationException("Missing configuration: GitHubModels:Token."));
var options = new OpenAIClientOptions()
{
    Endpoint = new Uri("https://models.github.ai/inference")
};
IChatClient client =
   new ChatClientBuilder( new OpenAIClient(credential, options).GetChatClient("openai/gpt-4.1-mini")
   .AsIChatClient()).UseFunctionInvocation().Build();


var chatOptions = new ChatOptions()
{
     Tools =  [
        // 🌦 Weather function
        AIFunctionFactory.Create(
            async (string location) =>
            {
                return await GetWeatherAsync(location);
            },
            name: "get_weather",
            description: "Get current weather of a city"
        ),

        // ➗ Math function
        AIFunctionFactory.Create(
            (int a, int b) =>
            {
                return $"Sum is {a + b}";
            },
            name: "add_numbers",
            description: "Add two numbers"
        ),

        // 🧾 Customer function
        AIFunctionFactory.Create(
            (string customerId) =>
            {
                return $"Customer {customerId} has premium plan";
            },
            name: "get_customer_info",
            description: "Get customer details by ID"
        )
    ]
};

// Helper: call OpenWeatherMap and return a human-readable summary
static async Task<string> GetWeatherAsync(string location)
{ 
    var temp = 42;
    var feelsLike = 20;
    var humidity = 10;
    var weather = "Weather conditions is hot";
    var windSpeed = 100.0; 
    var country = "India";

    return $"Weather for {location}{country}:\n" +
           $"- Conditions: {weather}\n" +
           $"- Temperature: {temp} °C (feels like {feelsLike} °C)\n" +
           $"- Humidity: {humidity}%\n" +
           (double.IsNaN(windSpeed) ? "" : $"- Wind: {windSpeed} m/s\n");
}

// Conversation starter (the assistant can use inserted weather info)
List<ChatMessage> chatHistory = new()
{
    new ChatMessage(ChatRole.System, "You are a helpful assistant. When the system / assistant provides factual weather data, use it to answer the user's question and give practical recommendations. If no weather data is provided, ask the user for the location. To explicitly request live weather, the user may type \"/weather <location>\" or ask \"What's the weather in <location>?\"")
}
;

Console.WriteLine("Type messages. Type 'exit' to quit.");
while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput))
        continue;
    if (userInput.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    // Add user's message to history
    chatHistory.Add(new ChatMessage(ChatRole.User, userInput));

    // Stream the model's response (it will see the weather message above if present)
    Console.WriteLine("AI: ");
    var responseText = "";
    await foreach (var item in client.GetStreamingResponseAsync(chatHistory, chatOptions))
    {
        Console.Write(item.Text);
        responseText += item.Text;
    }
    Console.WriteLine();

    // Save assistant response into history
    chatHistory.Add(new ChatMessage(ChatRole.Assistant, responseText));
}