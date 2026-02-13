# LocalEmbeddings

[![NuGet](https://img.shields.io/nuget/v/elbruno.LocalEmbeddings.svg)](https://www.nuget.org/packages/elbruno.LocalEmbeddings)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET library for generating text embeddings locally using ONNX Runtime and Microsoft.Extensions.AI abstractions.

## Installation

```bash
dotnet add package elbruno.LocalEmbeddings
```

## Quick Start

```csharp
using LocalEmbeddings;
using LocalEmbeddings.Options;

using var generator = new LocalEmbeddingGenerator(new LocalEmbeddingsOptions());
var embeddings = await generator.GenerateAsync(["Hello, world!"]);
Console.WriteLine($"Dimensions: {embeddings[0].Vector.Length}");
```

## Samples

See [samples/README.md](samples/README.md) for a full overview.

- [samples/ConsoleApp](samples/ConsoleApp)
- [samples/RagChat](samples/RagChat)
- [samples/RagOllama](samples/RagOllama)
- [samples/RagFoundryLocal](samples/RagFoundryLocal)

## Documentation

- [API Reference](docs/api-reference.md)
- [Configuration](docs/configuration.md)
- [Dependency Injection](docs/dependency-injection.md)
- [Contributing](docs/contributing.md)
- [Publishing](docs/publishing.md)

## Building from Source

```bash
git clone https://github.com/elbruno/elbruno.localembeddings.git
cd elbruno.localembeddings
dotnet build
dotnet test
```

## Requirements

- .NET 10.0 SDK or later
- ONNX Runtime compatible platform (Windows, Linux, macOS)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
