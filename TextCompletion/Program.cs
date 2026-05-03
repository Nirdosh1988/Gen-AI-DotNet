// get credentials from user secrets
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var credential = new ApiKeyCredential(config["GitHubModels:Token"] ?? throw new InvalidOperationException("Missing configuration: GitHubModels:Token."));
var options = new OpenAIClientOptions()
{
    Endpoint = new Uri("https://models.github.ai/inference")
};

// create a chat client
IChatClient client =
    new OpenAIClient(credential, options).GetChatClient("openai/gpt-4.1-mini").AsIChatClient();
#region ChatApp

// Start the conversation with context for the AI model
var chatHistory = new List<ChatMessage>
{
    new ChatMessage(ChatRole.System,@"You are a friendly hiking enthusiast who helps people discover fun hikes in their area.
                        Introduce yourself when you first say hello.
                        
                        When helping users, always ask for:
                        1) The location where they would like to hike
                        2) The hiking intensity they are looking for (easy, moderate, hard)
                        
                        Only after you have BOTH pieces of information, provide exactly three nearby hike suggestions that vary in length (short, medium, long).
                        
                        For each suggestion include:
                        - Hike name
                        - Approximate distance
                        - Difficulty level
                        - One interesting fact about local nature on that hike
                        
                        Keep the tone friendly and helpful. At the end of every response, ask if there is anything else you can help with.")
};

while (true)
{
    // Get user prompt and add to chat history
    Console.WriteLine("Your prompt:");
    var userPrompt = Console.ReadLine();
    chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

    // Stream the AI response and add to chat history
    Console.WriteLine("AI Response:");
    var response = "";
    await foreach (var item in
        client.GetStreamingResponseAsync(chatHistory))
    {
        Console.Write(item.Text);
        response += item.Text;
    }
    chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
    Console.WriteLine();
}

#endregion