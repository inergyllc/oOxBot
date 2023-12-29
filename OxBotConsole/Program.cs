using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace OxBotConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string OxBotAssistantAppSecret = "COY8Q~UEgN2DWkglg9wOv1YkCmqSw1Mq4R5zpblT";
            string OxBotAssistantAppId = "e2b63096-f3b9-41f7-97f3-bda2b55e62f5";
            string AzAdTenantId = "3367b936-5c61-409a-8bef-24d790063788";
            string OxBotAssistantAppName = "OxBot Assistant";
            string OxBotOpenaiApiKeyName = "OPENAI-API-KEY";
            string OxBotModelKeyName = "OPENAI-OXBOT-MODEL";
            var OxaicogKeyVaultUri = "https://oxaicog.vault.azure.net/";
            var OxaicogVaultUri = new Uri(OxaicogKeyVaultUri);
            var OxaicogVaultCredential = 
                new ClientSecretCredential(
                    AzAdTenantId,
                    OxBotAssistantAppId,
                    OxBotAssistantAppSecret);
            var client = new SecretClient(OxaicogVaultUri, OxaicogVaultCredential);

            // Retrieve a secret
            KeyVaultSecret OxbotOpenaiApiKey = client.GetSecret(OxBotOpenaiApiKeyName);
            KeyVaultSecret OxbotOpenaiModel = client.GetSecret(OxBotModelKeyName);

            Console.WriteLine($"{OxbotOpenaiApiKey.Value}, {OxbotOpenaiModel.Value}");
        }
    }
}