namespace ElBruno.LocalEmbeddings;

/// <summary>
/// Represents a semantic search match from a text corpus.
/// </summary>
/// <param name="Text">The matched corpus text.</param>
/// <param name="Index">The zero-based index of the matched item in the original corpus.</param>
/// <param name="Score">The cosine similarity score for the match.</param>
public sealed record SemanticSearchResult(string Text, int Index, float Score);
