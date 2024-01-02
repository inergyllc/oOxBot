using System.Text;
using System.Xml.Linq;
using OpenAI;
using OpenAI.Assistants;

namespace OxBotSvc;
public class AssistantSvc
{
    #region CONSTANTS

    public const int WIDTH_NAME = 20;
    public const int WIDTH_ID = 36;
    public const int WIDTH_MODEL = 25;
    public const int WIDTH_ORG = 30;

    #endregion CONSTANTS

    #region PROPERTIES

    public OpenAIClient Cli;
    public ListResponse<AssistantResponse> CachedAssistants { get; private set; } = new();
    public List<AssistantResponse> DeletedAssistants { get; private set; } = new();


    #endregion PROPERTIES

    #region CTOR AND INIT

    public AssistantSvc(OpenAIClient client) { Cli = client; }


    #endregion CTOR AND INIT

    #region API FNS

    public async Task<ListResponse<AssistantResponse>?> ListAsync(bool refresh=false)
    {
        if (refresh || CachedAssistants == null)
        {
            CachedAssistants = 
                await Cli!.AssistantsEndpoint.ListAssistantsAsync() ?? new();
        }
        return CachedAssistants;
    }

    public async Task<AssistantResponse?> RetrieveByNameAsync(string name)
    {
        await ListAsync(false);
        return CachedAssistants.Items.FirstOrDefault(a => a.Name == name); 
    }

    public async Task<AssistantResponse?> RetrieveByIdAsync(string id)
    {
        await ListAsync(false);
        return CachedAssistants.Items.FirstOrDefault(a => a.Id == id);
    }

    public async Task<bool> DeleteByIdAsync(string id)
    {
        // Going to move this one to the deleted list
        AssistantResponse? assistant = await RetrieveByIdAsync(id);

        // Cli can be null so 
        if (assistant != null && Cli != null)
        {
            bool isDeleted = await Cli.AssistantsEndpoint.DeleteAssistantAsync(id);
            if (isDeleted)
            {
                DeletedAssistants.Add(assistant);
                Console.WriteLine($"Deleted: {assistant.Id}, {assistant.Name}, {assistant.Model}");
            }
            return isDeleted;
        }
        return false;
    }

    public async Task<bool> DeleteByNameAsync(string name, bool first = false)
    {
        bool anyDeleted = false;
        await ListAsync(false);
        var matchedAssistants = 
            CachedAssistants.Items.Where(a => a.Name == name).ToList();

        foreach (var assistant in matchedAssistants)
        {
            var isDeleted = await DeleteByIdAsync(assistant.Id);
            anyDeleted = anyDeleted || isDeleted;
            if (first) { break; }
        }

        return anyDeleted;
    }














    public async Task ToStringAsync()
    {
        CachedAssistants ??= await Cli.AssistantsEndpoint.ListAssistantsAsync();
        Console.WriteLine($"{"Name",-WIDTH_NAME}" + 
            $"{"Id",-WIDTH_ID}" + 
            $"{"Model",-WIDTH_MODEL}" + 
            $"{"Organization",-WIDTH_ORG}");
        foreach (var assistant in CachedAssistants.Items)
        {
            Console.WriteLine(
                $"{assistant.Name,-WIDTH_NAME}" +
                $"{assistant.Id,-WIDTH_ID}" +
                $"{assistant.Model,-WIDTH_MODEL}" +
                $"{assistant.Organization,-WIDTH_ORG}");
        }
    }

    #endregion API FNS

}

