# Samples

| Sample | Description | Prerequisites | Run |
|---|---|---|---|
| [ConsoleApp](./ConsoleApp) | Embedding basics: single/batch generation, similarity, DI integration. | .NET 10 SDK | `dotnet run --project samples/ConsoleApp` |
| [RagChat](./RagChat) | Embedding-only semantic search over an in-memory FAQ dataset. | .NET 10 SDK | `dotnet run --project samples/RagChat` |
| [RagOllama](./RagOllama) | RAG chat using LocalEmbeddings for retrieval and Ollama `phi3.5` for responses. | Ollama running at `http://localhost:11434` and `phi3.5` pulled | `dotnet run --project samples/RagOllama` |
| [RagFoundryLocal](./RagFoundryLocal) | RAG chat using LocalEmbeddings for retrieval and Foundry Local `phi-3.5-mini` for responses. | Foundry Local installed and model available locally | `dotnet run --project samples/RagFoundryLocal` |

## Quick run notes

- ConsoleApp and RagChat run with only the .NET SDK.
- RagOllama requires an active Ollama service and local model:
  - `ollama pull phi3.5`
  - `ollama serve`
- RagFoundryLocal starts `phi-3.5-mini` through `FoundryLocalManager`.
