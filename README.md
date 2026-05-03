                                           **GenAI-DotNet**
**Funtion Call -** 
A .NET console application demonstrating Function Calling using Microsoft.Extensions.AI and GitHub Models.

**Key Features**

Use a list to highlight why someone should care.
 -> Uses gpt-4o-mini via GitHub Models.
 -> Demonstrates asynchronous tool calling.
 -> Lightweight implementation without Semantic Kernel.

**Prerequisites**
.NET 8.0 or 9.0 SDK
A GitHub Personal Access Token (PAT)
Setup
Clone the repo: git clone ...
Set your secret: dotnet user-secrets set "GitHubModels:Token" "your_token_here"
Run the app: dotnet run

// Example of adding a weather tool
var chatOptions = new ChatOptions {
    Tools = [AIFunctionFactory.Create(GetWeatherAsync, "get_weather", "Get weather")]
};

**Technologies Used**
.NET 9
Microsoft.Extensions.AI
Azure.AI.Inference (GitHub Models)
