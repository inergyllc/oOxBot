from ox_svc import OpenAIAssistantClient

client = OpenAIAssistantClient("OxBot Advisor")
responses = client.ask_question("Suggest a vendor for a malfunctioning derrick.  Give website urls if any")
for response in responses:
    print(response)