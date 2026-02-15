# ImageSearchSample — Multimodal CLIP Search

Text-to-image semantic search using CLIP models running entirely locally via ONNX Runtime.

## Overview

This sample demonstrates **multimodal text-to-image semantic search** using CLIP (Contrastive Language–Image Pretraining) models. It shows how to:

- Load and run CLIP text and vision encoders with ONNX Runtime
- Implement BPE tokenization for CLIP text encoding
- Preprocess images for CLIP vision encoding (resize, normalize, NCHW format)
- Index a collection of images by computing their embeddings
- Perform natural language queries to find semantically matching images
- Rank results by cosine similarity

Unlike the library's text-only embedding models, CLIP enables cross-modal search where you can search images using natural language descriptions.

## Prerequisites

1. **.NET 10 SDK**
2. **CLIP ONNX models** — text encoder, vision encoder, vocabulary, and merge files
3. **Sample images** — a directory of images to search (see `images/README.md`)

## CLIP Model Setup

### Option 1: Export from HuggingFace (Recommended)

Install the Optimum CLI and export CLIP to ONNX:

```bash
pip install optimum[exporters]
optimum-cli export onnx --model openai/clip-vit-base-patch32 ./clip-models/
```

This creates a `clip-models/` directory with:
- `text_model.onnx` — Text encoder
- `vision_model.onnx` — Image encoder  
- `vocab.json` — Vocabulary
- `merges.txt` — BPE merge rules
- `tokenizer.json` — Tokenizer config (not used by this sample)

### Option 2: Pre-exported Models

Download pre-exported ONNX models from HuggingFace:

```bash
# Clone the repository with ONNX models
git clone https://huggingface.co/Xenova/clip-vit-base-patch32 clip-models
```

### Model Details

**Recommended model:** `openai/clip-vit-base-patch32`

- Text/Image embedding dimensions: **512**
- Context length: **77 tokens**
- Image size: **224×224 pixels**
- Vocabulary size: **49,408**

## Sample Images

Place your images in the `images/` directory, or use any directory you prefer. Supported formats: `.jpg`, `.jpeg`, `.png`, `.bmp`, `.gif`.

For test images, see [`images/README.md`](images/README.md).

## Run

```bash
dotnet run --project samples/ImageSearchSample -- <model-directory> <image-directory>
```

Example:

```bash
dotnet run --project samples/ImageSearchSample -- ./clip-models ./images
```

### Interactive Search

Once the sample loads and indexes your images, you can enter natural language queries:

```
> a cat sitting on a couch
> a person riding a bicycle
> sunset over the ocean
> a red car
> exit
```

The sample returns the top 5 matching images with similarity scores.

## Example Output

```
=== CLIP Image Search Sample ===

Loading CLIP models...
Models loaded successfully.

Indexing 8 images...
  [1/8] Indexed: cat.jpg
  [2/8] Indexed: dog.jpg
  [3/8] Indexed: bicycle.jpg
  [4/8] Indexed: sunset.jpg
  [5/8] Indexed: car.jpg
  [6/8] Indexed: mountains.jpg
  [7/8] Indexed: beach.jpg
  [8/8] Indexed: forest.jpg
Indexing complete. 8 images ready for search.

Enter a search query (or 'exit' to quit):

> a cat

Top 5 results for "a cat":
------------------------------------------------------------
1. cat.jpg (score: 0.8234)
2. dog.jpg (score: 0.6543)
3. forest.jpg (score: 0.3421)
4. beach.jpg (score: 0.3102)
5. mountains.jpg (score: 0.2987)
------------------------------------------------------------

> sunset

Top 5 results for "sunset":
------------------------------------------------------------
1. sunset.jpg (score: 0.7891)
2. beach.jpg (score: 0.7123)
3. mountains.jpg (score: 0.6543)
4. forest.jpg (score: 0.4567)
5. car.jpg (score: 0.2345)
------------------------------------------------------------

> exit

Thank you for using CLIP Image Search!
```

## Architecture

This sample is **separate from the core library** because CLIP uses BPE tokenization (not WordPiece like BERT). The sample implements its own `ClipTokenizer` rather than modifying the core library.

### Components

- **`ClipTokenizer.cs`** — Minimal BPE tokenizer compatible with CLIP
- **`ClipTextEncoder.cs`** — Loads text ONNX model, tokenizes and encodes text
- **`ClipImageEncoder.cs`** — Loads vision ONNX model, preprocesses and encodes images
- **`ImageSearchEngine.cs`** — Indexes images and performs cosine similarity search
- **`Program.cs`** — Main entry point with CLI argument parsing and interactive loop

### Design Notes

1. **No library changes required** — This sample demonstrates extensibility without modifying `ElBruno.LocalEmbeddings` core code.
2. **Manual model download** — Unlike the main library's automatic model download, CLIP requires separate text/vision ONNX files that must be manually exported.
3. **Simplified BPE tokenizer** — This is a minimal implementation. For production use, consider more robust BPE tokenization.
4. **L2-normalized embeddings** — Both text and image embeddings are L2-normalized, so cosine similarity reduces to dot product.

## Troubleshooting

### "Required file not found: vocab.json"

Make sure you've exported or downloaded all required CLIP model files. See [CLIP Model Setup](#clip-model-setup).

### "No images found to index"

Ensure your image directory contains supported image formats (`.jpg`, `.jpeg`, `.png`, `.bmp`, `.gif`).

### ONNX Runtime errors

Make sure you're using compatible ONNX models. If you exported the model yourself, try using the exact command from [Option 1](#option-1-export-from-huggingface-recommended).

## Future Enhancements

- Full BPE tokenizer implementation with proper Unicode handling
- GPU acceleration for faster image encoding
- Persistent image index (save/load pre-computed embeddings)
- Batch processing for large image collections
- Web UI for visual search results

## References

- [CLIP paper (OpenAI)](https://arxiv.org/abs/2103.00020)
- [openai/clip-vit-base-patch32 on HuggingFace](https://huggingface.co/openai/clip-vit-base-patch32)
- [Optimum ONNX export](https://huggingface.co/docs/optimum/exporters/onnx/usage_guides/export_a_model)
- [SixLabors.ImageSharp documentation](https://docs.sixlabors.com/api/ImageSharp/)
