# Contributing — elbruno.LocalEmbeddings

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Git

## Getting Started

```bash
git clone https://github.com/elbruno/elbruno.localembeddings.git
cd elbruno.localembeddings
dotnet build
```

## Running Tests

```bash
dotnet test
```

## Repository Structure

```
├── README.md                  # Project overview (packed into NuGet)
├── LICENSE                    # MIT license
├── LocalEmbeddings.slnx       # Solution file
├── Directory.Build.props       # Shared build properties
├── docs/                       # Extended documentation
├── src/LocalEmbeddings/        # Main library source
├── tests/LocalEmbeddings.Tests/ # Unit tests
└── samples/                    # Sample applications
    ├── ConsoleApp/
    └── RagChat/
```

## Guidelines

- Keep the root directory clean — only README, LICENSE, solution, and build config files belong there.
- All extended documentation goes in `docs/`.
- The NuGet package ID is always `elbruno.LocalEmbeddings` (with the `elbruno.` prefix).
- Target .NET 10.0 or later.

## License

This project is licensed under the MIT License — see the [LICENSE](../LICENSE) file for details.
