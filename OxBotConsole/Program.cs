using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Files;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
// 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
// 
namespace OxBotConsole
{
    #region SUPPORT CLASSES

    public class ToolEqualityComparer : IEqualityComparer<Tool>
    {
        public bool Equals(Tool? x, Tool? y)
        {
            if (x == null || y == null) return x == y; // both are null or one is null
            return x.Id == y.Id && x.Type == y.Type;
        }

        public int GetHashCode(Tool obj)
        {
            return HashCode.Combine(obj.Id, obj.Type);
        }
    }

    #endregion SUPPORT CLASSES

    public class Program
    {
        #region PROPERTIES

        #region prop.CONSTANTS & READONLY

        // OpenAI Constants
        public const string OPENAI_API_KEYNAME = "OPENAI-API-KEY";

        // Azure Constants
        public const string ASSISTANT_REGAPP_SECRET = "COY8Q~UEgN2DWkglg9wOv1YkCmqSw1Mq4R5zpblT";
        public const string ASSISTANT_REGAPP_CLIENTID = "e2b63096-f3b9-41f7-97f3-bda2b55e62f5";
        public const string TENANT_ID = "3367b936-5c61-409a-8bef-24d790063788";
        public const string KEY_VAULT_URL = "https://oxaicog.vault.azure.net/";

        // OxBot Constants
        public const string OXBOT_NAME = "OxBot Advisor";
        public const string OXBOT_MODEL = "gpt-4-1106-preview";
        public const string OXBOT_ORGANIZATION = "oilwrx-llc";
        public const string OXBOT_DESCRIPTION = "Energy Sector Vendor Location Assistant";
        public const string OXBOT_INSTRUCTION_PATH = @"assets\assistants\oxbot_advisor\oxbot_advisor_instructions.txt";
        public static readonly IEnumerable<Tool> OXBOT_TOOLS = new[]
        {
            Tool.Retrieval,
            Tool.CodeInterpreter
        };
        public static readonly IEnumerable<string> OXBOT_FILEPATHS = new[]
        {
            @"assets\assistants\oxbot_advisor\files\geo_flat_listing_min_jsonl.json"
        };
        public static readonly Dictionary<string,bool> InSync = new()
        {
            { "name", true },
            { "model", true },
            { "description", true },
            { "instructions", true },
            { "tools", true },
            { "metadata", true },
            { "files", true }
        };
        public static bool IsInSync => InSync.All(kv => kv.Value);

        public static readonly Dictionary<string, string> OXBOT_METADATA = new()
        {
            { "version", "0.0.1" },
            { "author", "Amram E. Dworkin" },
            { "source_license", "MIT" },
            { "source_license_url", "https://opensource.org/licenses/MIT" },
            { "source_contact", "amram.dworkin@gmail.com"}
        };

        public static CreateAssistantRequest OXBOT_CREATE_REQUEST => 
            new(
                name: OXBOT_NAME,
                model: OXBOT_MODEL,
                description: OXBOT_DESCRIPTION,
                instructions: AssistInstructions,
                metadata: OXBOT_METADATA,
                tools: OXBOT_TOOLS
            );

        #endregion prop.CONSTANTS & READONLY

        #region prop.OXBOT

        public static string AssistInstructions
            => File.ReadAllText(OXBOT_INSTRUCTION_PATH);

        private static ListResponse<AssistantResponse>? Assistants = null;
        private static ListResponse<AssistantFileResponse>? AssistantFiles = null;
        private static IReadOnlyList<FileResponse>? Files = null;
        private static List<(string, long)>? RequestFilesInfo = null;
        private static List<(string, long)>? AssistantFilesInfo = null;
        private static AssistantResponse? Assistant = null;

        #endregion prop.OXBOT

        #region prop.AZURE

        public static ClientSecretCredential ClientCred =>
            new(TENANT_ID,
                ASSISTANT_REGAPP_CLIENTID,
                ASSISTANT_REGAPP_SECRET);
        public static Uri KeyVaultUri => new(KEY_VAULT_URL);
        public static SecretClient Cli => new(KeyVaultUri, ClientCred);
        public static KeyVaultSecret OpenaiApiKey => Cli.GetSecret(OPENAI_API_KEYNAME);

        #endregion prop.AZURE

        #region prop.OPENAI

        public static OpenAIClient OaiCli => new(OpenaiApiKey.Value);

        #endregion prop.OPENAI

        #endregion PROPERTIES

        #region METHODS

        #region meth.CTOR, MAIN, & INIT

        public static async Task Main()
        {
            await GetAssistantsAsync();
            await LogState("PRE-SYNC");
            if (!await AssistantExists(OXBOT_NAME))
            {
                await CreateAssistantAsync();
            }
            await SyncAssistant();
            await LogState("POST-SYNC");
        }

        #endregion meth.CTOR, MAIN, & INIT

        #region meth.ASSISTANT

        public static async Task IdentifyAndLogDiffereneces(
            AssistantResponse assistant, 
            CreateAssistantRequest request,
            IEnumerable<string>files)
        {
            InSync["name"] = assistant.Name == request.Name;
            InSync["model"] = assistant.Model == request.Model;
            InSync["description"] = assistant.Description == request.Description;
            InSync["instructions"] = assistant.Instructions == request.Instructions;
            InSync["tools"] = AreToolsEqual(assistant.Tools, request.Tools);
            InSync["metadata"] = 
                AreStringDictsEqual(assistant.Metadata, request.Metadata);
            InSync["files"] = await AreFilesEqual(assistant,files);
            if (InSync["name"]) { Console.WriteLine("1. NAME SAME"); }
            else
            {
                inSync = false;
                Console.WriteLine("1. NAME DIFFERENT");
                Console.WriteLine($"--> ASSISTANT: {GetStringOrNull(assistant.Name)}");
                Console.WriteLine($"--> REQUEST  : {GetStringOrNull(request.Name)}");
            }

            if (InSync["model"]) { Console.WriteLine("2. MODEL SAME"); }
            else
            {
                inSync = false;
                Console.WriteLine("2. MODEL DIFFERENT:");
                Console.WriteLine($"--> ASSISTANT: {GetStringOrNull(assistant.Model)}");
                Console.WriteLine($"--> REQUEST  : {GetStringOrNull(request.Model)}");
            }

            if (InSync["description"]) { Console.WriteLine("3. DESCRIPTION SAME"); }
            else
            {
                inSync = false;
                Console.WriteLine("3. DESCRIPTION DIFFERENT:");
                Console.WriteLine($"--> ASSISTANT: {GetStringOrNull(assistant.Description)}");
                Console.WriteLine($"--> REQUEST  : {GetStringOrNull(request.Description)}");
            }

            if (InSync["instructions"]) { Console.WriteLine("4. DESCRIPTION SAME"); }
            else
            {
                inSync = false;
                Console.WriteLine("4. INSTRUCTIONS DIFFERENT: at char " +
                    $"{DisjointStringIndex(
                        assistant.Instructions, 
                        request.Instructions)}");  
            }
            

            if (InSync["tools"]) { Console.WriteLine("5. TOOLS SAME"); }
            else
            {
                inSync = false;
                Console.WriteLine("5. TOOLS DIFFERENT");
                Console.WriteLine($"--> ASSISTANT");
                LogToolsOrNull(assistant.Tools);
                Console.WriteLine($"--> REQUEST");
                LogToolsOrNull(request.Tools);
            }

            if (InSync["metadata"]) { Console.WriteLine("6. METADATA SAME"); }
            else
            {
                inSync = false;
                Console.WriteLine("6. METADATA DIFFERENT");
                Console.WriteLine("--> ASSISTANT");
                LogDict(assistant.Metadata);
                Console.WriteLine("--> REQUEST");
                LogDict(request.Metadata);
            }

            if (InSync["files"]) { Console.WriteLine("7. FILES SAME"); }
            else 
            {
                Console.WriteLine("7. FILES DIFFERENT");
                Console.WriteLine("--> ASSISTANT");
                LogFiles(AssistantFilesInfo);
                Console.WriteLine("--> REQUEST");
                LogFiles(RequestFilesInfo);
            }
        }

        public static async Task SyncAssistant()
        {
            Assistant = await GetAssistantAsync(OXBOT_NAME);
            if (Assistant == null)
            {
                Console.WriteLine("Assistant is null");
                return;
            }
            await IdentifyAndLogDiffereneces(
                assistant: Assistant, 
                request: OXBOT_CREATE_REQUEST,
                files: OXBOT_FILEPATHS);
            if (IsInSync)
            {
                Console.WriteLine("Assistant is in sync");
            }
            else
            {
                await UpdateAssistant();
            }
            Console.WriteLine($"inSync: {InSync}");
            return;
        }

        public static async Task<bool> AssistantExists(string name)
        {
            await GetAssistantsAsync(refresh: false);
            return Assistants != null &&
                Assistants.Items.Any(assist => assist.Name == name);
        }

        public static async Task CreateAssistantAsync()
        {
            Assistant = 
                await OaiCli.AssistantsEndpoint.CreateAssistantAsync
                    (OXBOT_CREATE_REQUEST);

            await GetAssistantsAsync(refresh: true);
        }

        public static async Task UpdateAssistant()
        {
            if (Assistant == null)
            {
                Console.WriteLine("Assistant is null");
                return;
            }
            await OaiCli.AssistantsEndpoint.ModifyAssistantAsync
                (Assistant.Id, OXBOT_CREATE_REQUEST);

            if (InSync["files"])
            {
                Console.WriteLine("Files are in sync");
            }
            else
            {
                await RemoveAssistantFiles();
                await GetFilesAsync();
            }
            await LogState("After Sync");
        }
        public static async Task RemoveAssistantFiles()
        {
            if (Assistant == null)
            {
                Console.WriteLine("Assistant is null");
                return;
            }

            foreach (string fileid in Assistant.FileIds)
            {
                await OaiCli.AssistantsEndpoint.RemoveFileAsync
                    (Assistant.Id, fileid);
            }
        }

        public static async Task RemoveFiles()
        { 
            await GetFilesAsync(refresh:false);
            if (Files == null || Files.Count == 0) { return; }
            foreach (string requestFilename in OXBOT_FILEPATHS)
            {
                foreach(FileResponse assistantFile in Files) {                     
                    if (assistantFile.FileName == requestFilename)
                    {
                        await OaiCli.FilesEndpoint.DeleteFileAsync
                            (assistantFile.Id);
                    }
                }
            }
        }

        public static async Task UploadFilesAsync()
        {
            foreach(string filepath in OXBOT_FILEPATHS)
            {
                await Assistant.UploadFileAsync(filepath);
            }
        }

        public static async Task<AssistantResponse?> GetAssistantAsync(string name)
        {
            await GetAssistantsAsync(refresh: false);
            return Assistants?.Items.FirstOrDefault(assist => assist.Name == name);
        }

        public static async Task GetAssistantsAsync(bool refresh = false)
        {
            if (Assistants == null || refresh)
            {
                Assistants = await OaiCli.AssistantsEndpoint.ListAssistantsAsync();
            }
        }

        #region assist.LOGGING

        public static async Task LogState(string stateLine = "")
        {
            Console.WriteLine(new string('=', 135));
            Console.WriteLine(stateLine);
            Console.WriteLine(new string('-', 135));
            LogAssistantHeader();
            await LogAssistants();
            LogRequest();
            Console.WriteLine(new string('=', 135));

        }
        public static void LogAssistantHeader()
        {
            Console.WriteLine(
                $"{"Type",-12} | " +
                $"{"Name",-30} | " +
                $"{"Id",-30} | " +
                $"{"Model",-20} | " +
                $"{"Organization",-20} | " +
                $"{"# Files",-8}");

            Console.WriteLine(
                $"{new string('-', 12)} | " +
                $"{new string('-', 30)} | " +
                $"{new string('-', 30)} | " +
                $"{new string('-', 20)} | " +
                $"{new string('-', 20)} | " +
                $"{new string('-', 8)}");
        }
        public static void LogRequest()
        {
            Console.WriteLine(
                $"{"Request",-12} | " +
                $"{OXBOT_CREATE_REQUEST.Name,-30} | " +
                $"{"<NA>", 30} | " +
                $"{OXBOT_CREATE_REQUEST.Model,-20} | " +
                $"{OXBOT_ORGANIZATION,-20} | " +
                $"{OXBOT_FILEPATHS.Count(),-8}"
                );
        }
        public static async Task LogAssistants()
        {
            await GetAssistantsAsync();
            if (Assistants == null)
            {
                Console.WriteLine("No Assistants found.");
                return;
            }
            foreach (AssistantResponse assist in Assistants.Items)
            {
                Console.WriteLine(
                    $"{"Assistant",-12} | " +
                    $"{assist.Name,-30} | " + 
                    $"{assist.Id,-30} | " +
                    $"{assist.Model,-20} | " + 
                    $"{assist.Organization,-20} | " +
                    $"{assist.FileIds.Count,-8}");
            }
        }

        #endregion assist.LOGGING

        #endregion meth.ASSISTANT

        #region meth.FILE

        public static async Task GetFilesAsync(bool refresh = false)
        {
            if (Files == null || refresh)
            {
                Files = await OaiCli.FilesEndpoint.ListFilesAsync();
            }
        }

        public static async Task GetAssistantFilesAsync(
            AssistantResponse assistant,
            bool refresh = false)
        {
            if (AssistantFiles == null || refresh)
            {
                AssistantFiles = await assistant.ListFilesAsync();
            }
        }


        #endregion meth.FILE

        #region meth.HELPER

        public static async Task<bool> AreFilesEqual(
            AssistantResponse assistant,
            IEnumerable<string> filePaths)
        {
            await GetAssistantFilesAsync(assistant: assistant, refresh: true);
            await GetFilesAsync(refresh: true);
            RequestFilesInfo = filePaths?
                .Select(path => (
                    Path.GetFileName(path),
                    new FileInfo(path).Length
                ))
                .ToList() ?? new List<(string, long)>();

            AssistantFilesInfo = AssistantFiles?.Items
                .Join(Files ?? Enumerable.Empty<FileResponse>(),
                      afile => afile.Id,
                      file => file.Id,
                      (afile, file) => (file.FileName, file.Size ?? 0L))
                .ToList() ?? new List<(string, long)>();
            var isNotInAssistant = RequestFilesInfo.Except(AssistantFilesInfo).Any();
            var isNotInRequest = AssistantFilesInfo.Except(RequestFilesInfo).Any();
            return !isNotInAssistant && !isNotInRequest;
        }

        public static async Task<bool> AreFilesUnequal(
            AssistantResponse assistant,
            IEnumerable<string> filePaths) => 
                !await AreFilesEqual(assistant, filePaths);

        public static bool AreDictsEqual<TKey, TValue>(
            IReadOnlyDictionary<TKey, TValue> dict1,
            IReadOnlyDictionary<TKey, TValue> dict2)
        {
            return dict1.Count == dict2.Count && !dict1.Except(dict2).Any();
        }

        public static bool AreStringDictsEqual(IReadOnlyDictionary<string, string> dict1, IReadOnlyDictionary<string, string> dict2)
            => AreDictsEqual(dict1, dict2);
 
        public static bool AreStringDictsUnequal(
            IReadOnlyDictionary<string, string> dict1, 
            IReadOnlyDictionary<string, string> dict2)
                => !AreDictsEqual(dict1, dict2);

        public static bool AreToolsEqual(
           IReadOnlyList<Tool> list1,
           IReadOnlyList<Tool> list2)
        {
            var set1 = new HashSet<Tool>(list1, new ToolEqualityComparer());
            var set2 = new HashSet<Tool>(list2, new ToolEqualityComparer());
            return set1.SetEquals(set2);
        }

        public static bool AreToolsUnequal(
            IReadOnlyList<Tool> list1, 
            IReadOnlyList<Tool> list2)
                => !AreToolsEqual(list1, list2);

        public static void ChunkyLog(
            string text = "",
            int lineLength = 50,
            string prefix = "\t",
            bool prefixFirstLine = false)
        {
            prefix = prefix == "tab" ? "\t" : prefix;
            for (int i = 0; i < text.Length; i += lineLength)
            {
                string line = text.Substring(i, Math.Min(lineLength, text.Length - i));
                if (i == 0 && prefixFirstLine)
                {
                    Console.WriteLine($"{prefix}{line}");
                }
                else if (i == 0)
                {
                    Console.WriteLine($"{line}");
                }
                else
                {
                    Console.WriteLine($"{prefix}{line}");
                }
            }
        }

        public static int? DisjointStringIndex(string str1, string str2)
        {
            int length = Math.Min(str1.Length, str2.Length);
            for (int i = 0; i < length; i++)
            {
                if (str1[i] != str2[i])
                    return i;
            }
            return null; // Return null if the strings are identical up to the length of the shorter one
        }

        public static string GetStringOrNull(string input) =>
            string.IsNullOrWhiteSpace(input) ? "<null>" : input;

        public static void LogToolsOrNull(IReadOnlyList<Tool> tools)
        {
            if (tools == null || tools.Count == 0)
            {
                Console.WriteLine("       <NULL>");
            }
            else
            {
                tools
                    .Select(tool => tool.Type)
                    .ToList()
                    .ForEach(type => Console.WriteLine($"-----+ {type}"));

            }
        }

        public static void LogDict(IReadOnlyDictionary<string,string> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                Console.WriteLine("       <NULL>");
            }
            else
            {
                dict
                    .ToList()
                    .ForEach(kv => 
                        Console.WriteLine($"-----+ {kv.Key,-25} = {kv.Value}"));

            }
        }
        public static void LogFiles(List<(string FileName, long Size)>? files)
        {
            if (files == null || files.Count == 0)
            {
                Console.WriteLine("       <NULL>");
            }
            else
            {
                files.ForEach(file =>
                    Console.WriteLine
                        ($"-----+ {file.FileName,-25} = {file.Size} bytes"));
            }
        }

        #endregion meth.METHODS

        #endregion METHODS
    }
}