from ox_svc import assistant_manager

client = OpenAIAssistantClient("Adam Algebot")
responses = client.ask_question("I need to solve the equation `u^2 - 5u -14 = 0`. Please show your work.")
for response in responses:
    print(response)