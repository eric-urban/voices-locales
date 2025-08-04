# Azure AI Speech Voices and Locales Documentation Generator

This simple console app generates markdown tables for Azure AI Speech supported languages, text-to-speech voices, and their features. It uses some of the Azure AI Speech REST APIs to fetch the data and formats it into markdown files.

The generated markdown tables can be used to replace the existing tables in the [Azure AI documentation repository](https://github.com/MicrosoftDocs/azure-ai-docs-pr/tree/main/articles/ai-services/speech-service/includes/language-support). 

An example of rendered language and locales tables at `learn.microsoft.com` can be found [here](https://learn.microsoft.com/azure/ai-services/speech-service/language-support?tabs=stt#supported-languages).

## Setup

1. Prerequisites:
   - .NET 8.0 or later
   - C# 12 or later (required for collection expression syntax)

1. Install dependencies:
   ```bash
   dotnet add package Newtonsoft.Json
   dotnet add package DotNetEnv
   ```

1. Create a `.env` file in the project root with your Azure Speech credentials:
   ```env
   SPEECH_KEY=your-key-here
   SPEECH_REGION=your-region-here
   ```

1. Run the application:
   ```bash
   dotnet run
   ```


## What it does

Generates markdown tables for Azure AI Speech voices, locales, and features. Output files are saved in the `output/` directory:

- `language-identification.md`: Language identification support
- `stt.md`: Speech to text languages and features
- `tts.md`: Text to speech voices
- `voice-styles-and-roles.md`: Voice styles and roles

## Known issues

- The code currently uses the endpoint `api.cognitive.microsoft.com/speechtotext/v3.2/models/base` for speech base models. This will need to be upgraded to `api.cognitive.microsoft.com/speechtotext/models/base?api-version=2024-11-15` before Microsoft retires speech to text v3.2. 

