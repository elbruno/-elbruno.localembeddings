using System.Runtime.CompilerServices;
using LocalEmbeddings;
using LocalEmbeddings.Options;
using Microsoft.Extensions.AI;
using RagOllama;
using OllamaSharp;
using OllamaChatRole = OllamaSharp.Models.Chat.ChatRole;
using OllamaChatRequest = OllamaSharp.Models.Chat.ChatRequest;
using OllamaMessage = OllamaSharp.Models.Chat.Message;

Console.WriteLine("RAG sample with elbruno.LocalEmbeddings + Ollama phi3.5");
Console.WriteLine("Type 'exit' to quit.");

using var embeddingGenerator = new LocalEmbeddingGenerator(new LocalEmbeddingsOptions());
var vectorStore = new SimpleVectorStore(embeddingGenerator);
await vectorStore.AddAsync(KnowledgeBase.Documents);

using var ollama = new OllamaApiClient(new Uri("http://localhost:11434"), "phi3.5");
IChatClient chatClient = new OllamaChatClient(ollama);

while (true)
{
    Console.Write("\nYou> ");
    var query = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(query))
    {
        continue;
    }

    if (query.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    var contextDocs = await vectorStore.SearchAsync(query, topK: 3);
    var prompt = BuildPrompt(query, contextDocs.Select(d => d.Content));

    Console.Write("Assistant> ");
    await foreach (var update in chatClient.GetStreamingResponseAsync([new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, prompt)]))
    {
        if (!string.IsNullOrEmpty(update.Text))
        {
            Console.Write(update.Text);
        }
    }

    Console.WriteLine();
}

static string BuildPrompt(string question, IEnumerable<string> contextDocs)
{
    var context = string.Join("\n- ", contextDocs);
    return $"""
You are a helpful assistant. Use the provided context to answer briefly and accurately.

Context:
- {context}

Question: {question}
""";
}

internal sealed class OllamaChatClient(IOllamaApiClient client) : IChatClient
{
    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var text = string.Empty;
        await foreach (var update in GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                text += update.Text;
            }
        }

        return new ChatResponse(new ChatMessage(ChatRole.Assistant, text));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new OllamaChatRequest
        {
            Model = client.SelectedModel,
            Stream = true,
            Messages = messages.Select(m => new OllamaMessage(MapRole(m.Role), m.Text ?? string.Empty))
        };

        await foreach (var update in client.ChatAsync(request, cancellationToken))
        {
            if (update?.Message is { Content: { Length: > 0 } text })
            {
                yield return new ChatResponseUpdate(Microsoft.Extensions.AI.ChatRole.Assistant, text);
            }
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType.IsInstanceOfType(client) ? client : null;
    }

    public void Dispose()
    {
    }

    private static OllamaChatRole MapRole(Microsoft.Extensions.AI.ChatRole role)
    {
        if (role == Microsoft.Extensions.AI.ChatRole.System)
        {
            return OllamaChatRole.System;
        }

        if (role == Microsoft.Extensions.AI.ChatRole.Assistant)
        {
            return OllamaChatRole.Assistant;
        }

        return OllamaChatRole.User;
    }
}
