using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using DotNetEnv;

namespace SupportedLocales
{
    
    public static class Configuration
    {
        // API endpoints
        public static string SttUri => $"https://{SpeechRegion}.stt.speech.microsoft.com/api/v1.0/languages/recognition";
        public static string VoicesUri => $"https://{SpeechRegion}.tts.speech.microsoft.com/cognitiveservices/voices/list";
        public static string BaseModelsUri => $"https://{SpeechRegion}.api.cognitive.microsoft.com/speechtotext/v3.2/models/base";
        public static string FastTranscriptionLocalesUri => $"https://{SpeechRegion}.api.cognitive.microsoft.com/speechtotext/transcriptions/locales?api-version=2024-11-15";
        public const string QueryString = "?alt=json";
        
        // Authentication
        public static string SpeechKey = GetEnvOrDefault("SPEECH_KEY", "");
        public static string SpeechRegion = GetEnvOrDefault("SPEECH_REGION", "eastus");

        private static string GetEnvOrDefault(string key, string defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        // Feature support lists
        public static readonly Dictionary<string, string> AdaptationTypes = new Dictionary<string, string> {
            {"Acoustic", "Audio + human-labeled transcript"},
            {"AudioFiles", "Audio"},
            {"Language", "Plain text"},
            {"Pronunciation", "Pronunciation"},
            {"LanguageMarkdown", "Structured text"},
            {"OutputFormatting", "Output format"},
        };
        
        public static readonly HashSet<string> PhraseListLocales =
            [
                "ar-SA", "de-CH", "de-DE", "en-AU", "en-CA", "en-GB", "en-IE", "en-IN", "en-US", "en-ZA", "es-ES", "es-MX", "es-US", "fr-CA", "fr-FR", "hi-IN", "id-ID", "it-IT", "ja-JP", "ko-KR", "nl-NL", "pl-PL", "pt-BR", "pt-PT", "ru-RU", "sv-SE", "th-TH", "vi-VN", "zh-CN", "zh-HK", "zh-TW"
            ];
        
        public static readonly HashSet<string> VisemesLocales =
            [
                "ar-AE", "ar-BH", "ar-DZ", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY", "ar-MA", "ar-OM", "ar-QA", "ar-SA", "ar-SY", "ar-TN", "ar-YE", "bg-BG", "ca-ES", "cs-CZ", "da-DK", "de-AT", "de-CH", "de-DE", "el-GR", "en-AU", "en-CA", "en-GB", "en-HK", "en-IE", "en-IN", "en-KE", "en-NG", "en-NZ", "en-PH", "en-SG", "en-TZ", "en-US", "en-ZA", "es-AR", "es-BO", "es-CL", "es-CO", "es-CR", "es-CU", "es-DO", "es-EC", "es-ES", "es-GQ", "es-GT", "es-HN", "es-MX", "es-NI", "es-PA", "es-PE", "es-PR", "es-PY", "es-SV", "es-US", "es-UY", "es-VE", "fi-FI", "fr-BE", "fr-CA", "fr-CH", "fr-FR", "gu-IN", "he-IL", "hi-IN", "hr-HR", "hu-HU", "id-ID", "it-IT", "ja-JP", "ko-KR", "mr-IN", "ms-MY", "nb-NO", "nl-BE", "nl-NL", "pl-PL", "pt-BR", "pt-PT", "ro-RO", "ru-RU", "sk-SK", "sl-SI", "sv-SE", "sw-TZ", "ta-IN", "ta-LK", "ta-MY", "ta-SG", "te-IN", "th-TH", "tr-TR", "uk-UA", "ur-IN", "ur-PK", "vi-VN", "zh-CN", "zh-HK", "zh-TW"
            ];
                
        public static readonly HashSet<string> ChildVoices =
            [
                "de-DE-GiselaNeural", "en-GB-MaisieNeural", "en-US-AnaNeural", "es-MX-MarinaNeural", "fr-FR-EloiseNeural", "it-IT-PierinaNeural", "pt-BR-LeticiaNeural", "zh-CN-XiaoshuangNeural", "zh-CN-XiaoyouNeural"
            ];
        
        public static readonly HashSet<string> IndianRegionLocales =
            [
                "as-IN", "or-IN", "pa-IN"
            ];
        
        public static readonly HashSet<string> ChineseAccents = new HashSet<string> { 
            "shandong", "liaoning", "sichuan", "henan", "shaanxi",
            "SHANDONG", "LIAONING", "SICHUAN", "HENAN", "SHAANXI"
        };

        // Output configuration
        public const string OutputDirectory = "output";
        public static readonly Dictionary<string, string> OutputFiles = new Dictionary<string, string>
        {
            {"stt", "stt.md"},
            {"language-identification", "language-identification.md"},
            {"tts", "tts.md"},
            {"voice-styles-and-roles", "voice-styles-and-roles.md"}
        };
    }

    
    public class SpeechServiceApiClient
    {
        private readonly Dictionary<string, string> _headers;

        public SpeechServiceApiClient(string subscriptionKey)
        {
            _headers = new Dictionary<string, string>();
            _headers["Ocp-Apim-Subscription-Key"] = subscriptionKey;
        }

        public List<SttLanguage> GetSttLanguages()
        {
            var url = $"{Configuration.SttUri}{Configuration.QueryString}&format=detailed";
            var sttLanguages = GetResourceAsync<List<SttLanguage>>(url).GetAwaiter().GetResult();
            sttLanguages.Sort((x, y) => string.Compare(x.Name ?? string.Empty, y.Name ?? string.Empty, StringComparison.Ordinal));
            for(int i = 0; i < sttLanguages.Count; i++)
            {
                sttLanguages[i].Name = LocaleHelper.NormalizeLocale(sttLanguages[i].Name ?? string.Empty);
                sttLanguages[i].EnglishName = LocaleHelper.NormalizeLanguage(sttLanguages[i].Name ?? string.Empty, sttLanguages[i].EnglishName ?? string.Empty);
            }
            return sttLanguages;
        }

        public FastTranscriptionLanguages GetFastTranscriptionLanguages()
        {
            return GetResourceAsync<FastTranscriptionLanguages>(Configuration.FastTranscriptionLocalesUri).GetAwaiter().GetResult();
        }

        public List<BaseModel> GetCustomSpeechBaseModels()
        {
            var url = $"{Configuration.BaseModelsUri}{Configuration.QueryString}";
            var customSpeechBaseModels = new List<BaseModel>();
            do 
            {
                var baseModelCollection = GetResourceAsync<BaseModelCollection>(url).GetAwaiter().GetResult();
                var baseModels = baseModelCollection.BaseModels;
                if (baseModels != null)
                {
                    foreach (var baseModel in baseModels)
                    {
                        if (baseModel?.Properties?.DeprecationDates?.AdaptationDateTime != null && baseModel.Properties.DeprecationDates.AdaptationDateTime > DateTime.Now)
                        {
                            customSpeechBaseModels.Add(baseModel);
                        }
                    }
                }
                url = baseModelCollection.NextLink ?? string.Empty;
            } while(!string.IsNullOrEmpty(url));
            customSpeechBaseModels.Sort((x, y) => string.Compare(x.Locale ?? string.Empty, y.Locale ?? string.Empty, StringComparison.Ordinal));
            for(int i = 0; i < customSpeechBaseModels.Count; i++)
            {
                customSpeechBaseModels[i].Locale = LocaleHelper.NormalizeLocale(customSpeechBaseModels[i].Locale ?? string.Empty);
            }
            return customSpeechBaseModels;
        }

        public List<TtsVoice> GetTtsVoices()
        {
            var url = $"{Configuration.VoicesUri}{Configuration.QueryString}";
            var allTtsVoices = GetResourceAsync<List<TtsVoice>>(url).GetAwaiter().GetResult();
            var ttsVoices = allTtsVoices.Where(v => (v.Status?.Contains("GA") ?? false) || (v.Status?.Contains("Preview") ?? false)).ToList();
            for(int i = 0; i < ttsVoices.Count; i++)
            {
                ttsVoices[i].Order = i;
                ttsVoices[i].Locale = LocaleHelper.NormalizeLocale(ttsVoices[i].Locale ?? string.Empty);
                ttsVoices[i].LocaleName = LocaleHelper.NormalizeLanguage(ttsVoices[i].Locale ?? string.Empty, ttsVoices[i].LocaleName ?? string.Empty);
                if (ttsVoices[i].StyleList?.Length > 0)
                    Array.Sort(ttsVoices[i].StyleList!);
                if (ttsVoices[i].RolePlayList?.Length > 0)
                    Array.Sort(ttsVoices[i].RolePlayList!);
            }
            ttsVoices.Sort((x, y) => string.Compare(x.Locale ?? string.Empty, y.Locale ?? string.Empty, StringComparison.Ordinal));
            return ttsVoices;
        }

        private async System.Threading.Tasks.Task<T> GetResourceAsync<T>(string uri)
        {
            using var httpClient = new System.Net.Http.HttpClient();
            foreach (var header in _headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };
            return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings)!;
        }
    }

    
    public static class LocaleHelper
    {
        public static string NormalizeLocale(string locale)
        {
            var keyComponents = locale.Split('-');
            var newLocale = keyComponents[0];
            
            for(int i = 1; i < keyComponents.Length; i++)
            {
                if(Configuration.ChineseAccents.Contains(keyComponents[i]))
                {
                    newLocale += "-" + keyComponents[i].ToLower();
                }
                else
                {
                    newLocale += "-" + keyComponents[i].ToUpper();
                }
            }
            return newLocale;
        }

        public static string NormalizeLanguage(string locale, string language)
        {
            // Handle special cases
            if (locale.Contains("ca-ES")) return "Catalan";
            if (locale.Contains("tr-TR")) return "Turkish (Türkiye)";
            if (locale.Contains("sw-KE")) return "Kiswahili (Kenya)";
            if (locale.Contains("sw-TZ")) return "Kiswahili (Tanzania)";
            if (locale.Contains("zu-ZA")) return "isiZulu (South Africa)";
            
            return language;
        }

        public static Dictionary<string, string> ExtractLocalesDictionary(List<SttLanguage> sttLanguages)
        {
            var locales = new Dictionary<string, string>();
            foreach(var sttLanguage in sttLanguages)
            {
                locales.Add(NormalizeLocale(sttLanguage.Name ?? string.Empty), 
                    NormalizeLanguage(sttLanguage.Name ?? string.Empty, sttLanguage.EnglishName ?? string.Empty));
            }
            return locales;
        }

        public static Dictionary<string, string> ExtractLocalesDictionary(List<TtsVoice> ttsVoices)
        {
            var locales = new Dictionary<string, string>();
            foreach(var ttsVoice in ttsVoices)
            {
                if(ttsVoice.Locale != null && ttsVoice.LocaleName != null && !locales.ContainsKey(ttsVoice.Locale))
                {
                    locales.Add(ttsVoice.Locale, ttsVoice.LocaleName);
                }
            }
            return locales;
        }
    }

    class Program

    {
        static void Main(string[] args)
        {
            // Load environment variables from .env file
            Env.Load();
            try
            {
                Console.WriteLine("Starting Speech voices and locales tables generation...");

                // Initialize API client
                var apiClient = new SpeechServiceApiClient(Configuration.SpeechKey);

                // Create output directory if it doesn't exist
                Directory.CreateDirectory(Configuration.OutputDirectory);

                // Fetch data from APIs
                Console.WriteLine("Fetching speech to text locales...");
                var sttLanguages = apiClient.GetSttLanguages();

                Console.WriteLine("Fetching fast transcription locales...");
                var fastTranscriptionLanguages = apiClient.GetFastTranscriptionLanguages();

                Console.WriteLine("Fetching custom speech base models...");
                var customSpeechBaseModels = apiClient.GetCustomSpeechBaseModels();

                Console.WriteLine("Fetching text to speech voices...");
                var ttsVoices = apiClient.GetTtsVoices();

                // Initialize markdown generators
                var sttGenerator = new SttMarkdownGenerator();
                var languageIdGenerator = new LanguageIdentificationMarkdownGenerator();
                var ttsGenerator = new TtsMarkdownGenerator();
                var voiceStylesGenerator = new VoiceStylesRolesMarkdownGenerator();

                // Generate and save markdown files
                
                Console.WriteLine("Generating language identification markdown...");
                var languageIdMarkdown = languageIdGenerator.Generate(sttLanguages);
                var languageIdFilePath = Path.Combine(Configuration.OutputDirectory, Configuration.OutputFiles["language-identification"]);
                File.WriteAllLines(languageIdFilePath, languageIdMarkdown, Encoding.UTF8);

                Console.WriteLine("Generating speech to text markdown...");
                var sttMarkdown = sttGenerator.Generate(sttLanguages, fastTranscriptionLanguages, customSpeechBaseModels);
                var sttFilePath = Path.Combine(Configuration.OutputDirectory, Configuration.OutputFiles["stt"]);
                File.WriteAllLines(sttFilePath, sttMarkdown, Encoding.UTF8);

                Console.WriteLine("Generating text to speech markdown...");
                var ttsMarkdown = ttsGenerator.Generate(ttsVoices);
                var ttsFilePath = Path.Combine(Configuration.OutputDirectory, Configuration.OutputFiles["tts"]);
                File.WriteAllLines(ttsFilePath, ttsMarkdown, Encoding.UTF8);

                Console.WriteLine("Generating voice styles and roles markdown...");
                var voiceStylesMarkdown = voiceStylesGenerator.Generate(ttsVoices);
                var voiceStylesFilePath = Path.Combine(Configuration.OutputDirectory, Configuration.OutputFiles["voice-styles-and-roles"]);
                File.WriteAllLines(voiceStylesFilePath, voiceStylesMarkdown, Encoding.UTF8);

                Console.WriteLine("\nSuccessfully generated markdown files:");
                Console.WriteLine($"- {sttFilePath}");
                Console.WriteLine($"- {languageIdFilePath}");
                Console.WriteLine($"- {ttsFilePath}");
                Console.WriteLine($"- {voiceStylesFilePath}");
            }
            // Network errors are now handled by HttpClient exceptions
            catch (Exception e)
            {
                Console.WriteLine($"\nUnexpected error: {e.Message}");
                Console.WriteLine($"Stack trace: {e.StackTrace}");
            }
        }


        private static void PrintErrors(SpeechError speechError)
        {
            if (speechError?.Error != null)
            {
                Console.WriteLine($"Error code: {speechError.Error.Code}");
                Console.WriteLine($"Error message: {speechError.Error.Message}");
                if (speechError.Error.Errors != null)
                {
                    foreach (var error in speechError.Error.Errors)
                    {
                        Console.WriteLine($"Reason: {error.Reason}");
                        Console.WriteLine($"Message: {error.Message}");
                    }
                }
            }
        }
    }

    
    public abstract class MarkdownGeneratorBase
    {
        protected static Dictionary<string, string> CreateLanguageCells(Dictionary<string, string> locales)
        {
            return new Dictionary<string, string>(locales);
        }

        protected static Dictionary<string, string> CombineLocales(
            Dictionary<string, string> sttLocales, 
            Dictionary<string, string> ttsLocales)
        {
            var combined = new Dictionary<string, string>();
            
            // Add STT locales first (bias towards STT naming)
            foreach (var locale in sttLocales)
            {
                if (!combined.ContainsKey(locale.Key))
                {
                    combined.Add(locale.Key, locale.Value);
                }
            }
            
            // Add TTS locales that aren't already present
            foreach (var locale in ttsLocales)
            {
                if (!combined.ContainsKey(locale.Key))
                {
                    combined.Add(locale.Key, locale.Value);
                }
            }
            
            return combined.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    
    public class SttMarkdownGenerator : MarkdownGeneratorBase
    {
        public List<string> Generate(List<SttLanguage> sttLanguages, FastTranscriptionLanguages fastTranscriptionLanguages, List<BaseModel> customSpeechBaseModels)
        {
            var sttLocales = LocaleHelper.ExtractLocalesDictionary(sttLanguages);
            var languageCells = CreateLanguageCells(sttLocales);
            
            // Create fast transcription lookup
            var fastTranscriptionTranscribeLocales = new Dictionary<string, string>();
            if (fastTranscriptionLanguages.Transcribe != null)
            {
                foreach(var locale in fastTranscriptionLanguages.Transcribe)
                {
                    fastTranscriptionTranscribeLocales.Add(LocaleHelper.NormalizeLocale(locale ?? string.Empty), LocaleHelper.NormalizeLanguage(locale ?? string.Empty, locale ?? string.Empty));
                }
            }
            
            var customSpeechBaseModelCells = CreateCustomSpeechBaseModelCells(customSpeechBaseModels);
            
            var markdown = new List<string>
            {
                "| Locale (BCP-47) | Language | Fast transcription support | Custom speech support |",
                "| ----- | ----- | ----- | ----- |"
            };
            
            foreach (var locale in sttLocales)
            {
                var row = $"| `{locale.Key}` | ";
                row += languageCells.ContainsKey(locale.Key) ? $"{languageCells[locale.Key]} | " : "Not supported | ";
                row += fastTranscriptionTranscribeLocales.ContainsKey(locale.Key) ? "Yes | " : "No | ";
                row += customSpeechBaseModelCells.ContainsKey(locale.Key) ? $"{customSpeechBaseModelCells[locale.Key]} |" : "Not supported |";
                markdown.Add(row);
            }
            
            return markdown;
        }

        private Dictionary<string, string> CreateCustomSpeechBaseModelCells(List<BaseModel> customSpeechBaseModels)
        {
            var cells = new Dictionary<string, string>();
            var customSpeechBaseModelsByLocales = customSpeechBaseModels.GroupBy(v => v.Locale).Select(g => g.ToList()).ToList();

            foreach(var customSpeechBaseModelsByLocale in customSpeechBaseModelsByLocales)
            {
                var currentLocale = customSpeechBaseModelsByLocale.First().Locale;
                var supportsAdaptationsWith = new List<string>();
                
                foreach(var customSpeechBaseModel in customSpeechBaseModelsByLocale)
                {
                    if (customSpeechBaseModel?.Properties?.Features?.SupportsAdaptationsWith != null)
                    {
                        supportsAdaptationsWith.AddRange(customSpeechBaseModel.Properties.Features.SupportsAdaptationsWith.ToList());
                    }
                }
                
                var supportedCustomizations = supportsAdaptationsWith.Distinct().ToList();
                supportedCustomizations.Sort();

                // Map API names to human-readable names
                for(int i = 0; i < supportedCustomizations.Count; i++)
                {
                    supportedCustomizations[i] = Configuration.AdaptationTypes.TryGetValue(supportedCustomizations[i], out var value) 
                        ? value : supportedCustomizations[i];
                }

                // Add phrase list support if applicable
                if(Configuration.PhraseListLocales.Contains(currentLocale ?? string.Empty))
                {
                    supportedCustomizations.Add("Phrase list");
                }

                cells.Add(currentLocale ?? string.Empty, string.Join("<br/><br/>", supportedCustomizations));
            }
            
            return cells;
        }
    }

    
    public class LanguageIdentificationMarkdownGenerator : MarkdownGeneratorBase
    {
        public List<string> Generate(List<SttLanguage> sttLanguages)
        {
            var sttLocales = LocaleHelper.ExtractLocalesDictionary(sttLanguages);
            
            // Extract unique languages (without parenthetical info)
            var languagesWithoutParens = sttLocales.Values
                .Select(language => language.Split('(')[0].Trim())
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            
            var markdown = new List<string>
            {
                "| Language | Locales (BCP-47) |",
                "| ----- | ----- |"
            };
            
            foreach (var language in languagesWithoutParens)
            {
                var row = $"| {language} | ";
                var localesForLanguage = sttLocales.Where(locale => locale.Value.Contains(language))
                    .Select(locale => $"`{locale.Key}`")
                    .ToList();
                
                row += string.Join("<br/>", localesForLanguage) + " |";
                markdown.Add(row);
            }
            
            return markdown;
        }
    }

    
    public class TtsMarkdownGenerator : MarkdownGeneratorBase
    {
        public List<string> Generate(List<TtsVoice> ttsVoices)
        {
            var ttsLocales = LocaleHelper.ExtractLocalesDictionary(ttsVoices);
            var languageCells = CreateLanguageCells(ttsLocales);
            var ttsVoicesCells = CreateTtsVoicesCells(ttsVoices);
            
            var markdown = new List<string>
            {
                "| Locale (BCP-47) | Language | Text to speech voices |",
                "| ----- | ----- | ----- |"
            };
            
            foreach (var locale in ttsLocales)
            {
                var row = $"| `{locale.Key}` | ";
                row += languageCells.ContainsKey(locale.Key) ? $"{languageCells[locale.Key]} | " : "Not supported | ";
                row += ttsVoicesCells.ContainsKey(locale.Key) ? $"{ttsVoicesCells[locale.Key]} |" : "Not supported |";
                markdown.Add(row);
            }
            
            return markdown;
        }

        private Dictionary<string, string> CreateTtsVoicesCells(List<TtsVoice> ttsVoices)
        {
            var ttsVoicesCells = new Dictionary<string, string>();
            var ttsVoicesByLocales = ttsVoices.GroupBy(v => v.Locale).Select(g => g.ToList()).ToList();

            foreach(var ttsVoicesByLocale in ttsVoicesByLocales)
            {
                ttsVoicesByLocale.Sort((x, y) => x.Order.CompareTo(y.Order));
                var currentLocale = ttsVoicesByLocale.First().Locale;
                var voicesPerLocaleMarkdown = new List<string>();
                
                foreach(var ttsVoice in ttsVoicesByLocale)
                {
                    voicesPerLocaleMarkdown.Add(CreateTtsVoiceMarkdown(ttsVoice));
                }
                
                ttsVoicesCells.Add(currentLocale ?? string.Empty, string.Join("<br/>", voicesPerLocaleMarkdown));
            }
            
            return ttsVoicesCells;
        }

        private string CreateTtsVoiceMarkdown(TtsVoice ttsVoice)
        {
            var supList = new List<int>();
            
            if (ttsVoice.Status?.Contains("Preview") ?? false)
            {
                supList.Add(Configuration.IndianRegionLocales.Contains(ttsVoice.Locale ?? string.Empty) ? 2 : 1);
            }

            if (!(Configuration.VisemesLocales.Contains(ttsVoice.Locale ?? string.Empty)))
            {
                supList.Add(3);
            }

            if (ttsVoice.ShortName?.Contains("Multilingual") ?? false)
            {
                supList.Add(4);
            }

            if (ttsVoice.ShortName != null && Configuration.ChildVoices.Contains(ttsVoice.ShortName))
            {
                ttsVoice.Gender += ", Child";
            }

            var sup = supList.Count > 0 ? "<sup>" + string.Join(",", supList) + "</sup>" : "";

            return $"`{ttsVoice.ShortName}`{sup} ({ttsVoice.Gender})";
        }
    }

    
    public class VoiceStylesRolesMarkdownGenerator : MarkdownGeneratorBase
    {
        public List<string> Generate(List<TtsVoice> ttsVoices)
        {
            var markdown = new List<string>
            {
                "| Voice | Styles |Roles |",
                "| ----- | ----- | ----- |"
            };
            
            var voicesWithStylesOrRoles = ttsVoices
                .Where(v => v.RolePlayList?.Length > 0 || v.StyleList?.Length > 0)
                .OrderBy(v => v.ShortName)
                .ToList();

            foreach(var ttsVoice in voicesWithStylesOrRoles)
            {
                var row = $"| `{ttsVoice.ShortName}` | ";
                
                // Styles column
                if (ttsVoice.StyleList?.Length > 0)
                {
                    row += string.Join(", ", ttsVoice.StyleList.Select(s => $"`{s}`"));
                }
                else
                {
                    row += "Not supported";
                }
                row += " | ";
                
                // Roles column
                if (ttsVoice.RolePlayList?.Length > 0)
                {
                    row += string.Join(", ", ttsVoice.RolePlayList.Select(r => $"`{r}`"));
                }
                else
                {
                    row += "Not supported";
                }
                row += " |";
                
                markdown.Add(row);
            }

            return markdown;
        }
    }

    // Data model classes
    public class TtsVoice
    {
            [JsonProperty("shortName")]
            public string? ShortName { get; set; }
            
            [JsonProperty("gender")]
            public string? Gender { get; set; }

            [JsonProperty("locale")]
            public string? Locale { get; set; }

            [JsonProperty("localeName")]
            public string? LocaleName { get; set; }
            
            [JsonProperty("status")]
            public string? Status { get; set; }
            
            [JsonProperty("order")]
            public int Order { get; set; }
            
            [JsonProperty("styleList", NullValueHandling=NullValueHandling.Ignore)]
            public string[]? StyleList { get; set; }

            [JsonProperty("rolePlayList", NullValueHandling=NullValueHandling.Ignore)]
            public string[]? RolePlayList { get; set; }
    }

    public class SttLanguage
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
        
        [JsonProperty("englishName")]
        public string? EnglishName { get; set; }
        
        [JsonProperty("nativeName")]
        public string? NativeName { get; set; }
        
        [JsonProperty("direction")]
        public string? Direction { get; set; }
    }

    public class FastTranscriptionLanguages
    {
        [JsonProperty("Submit")]
        public string[]? Submit { get; set; }

        [JsonProperty("Transcribe")]
        public string[]? Transcribe { get; set; }
    }

    public class BaseModelCollection
    {
        [JsonProperty("values")]
        public List<BaseModel>? BaseModels { get; set; }

        [JsonProperty("@nextLink", NullValueHandling=NullValueHandling.Ignore)]
        public string? NextLink { get; set; }
    }

    public class BaseModel
    {
        [JsonProperty("self")]
        public string? Self { get; set; }
        
        [JsonProperty("locale")]
        public string? Locale { get; set; }
        
        [JsonProperty("status")]
        public string? Status { get; set; }
        
        [JsonProperty("properties")]
        public ModelProperties? Properties { get; set; }
        
        [JsonProperty("displayName")]
        public string? DisplayName { get; set; }
    }

    public class ModelProperties
    {
        [JsonProperty("deprecationDates")]
        public ModelDeprecationDates? DeprecationDates { get; set; }
        
        [JsonProperty("features")]
        public ModelFeatures? Features { get; set; }
    }

    public class ModelDeprecationDates
    {
        [JsonProperty("adaptationDateTime")]
        public DateTime? AdaptationDateTime { get; set; }
        
        [JsonProperty("transcriptionDateTime")]
        public DateTime? transcriptionDateTime { get; set; }
    }

    public class ModelFeatures
    {
        [JsonProperty("supportsAdaptationsWith")]
        public string[]? SupportsAdaptationsWith { get; set; }

        [JsonProperty("supportedOutputFormats")]
        public string[]? SupportedOutputFormats { get; set; }
    }

    // Classes used to handle errors.
    public class Error
    {
        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("reason")]
        public string? Reason { get; set; }
    }

    public class ErrorCollection
    {
        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("errors")]
        public List<Error>? Errors { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }
    }

    public class SpeechError
    {
        [JsonProperty("error")]
        public ErrorCollection? Error { get; set; }
    }
}
