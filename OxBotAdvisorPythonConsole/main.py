import json
from openai import OpenAI
from dotenv import load_dotenv
import os
from count_file_tokens import count_file_tokens
from reduce_listing_jsonl import reduce_listing_jsonl

load_dotenv()

# Load the API key and model name from environment variables
openai_api_key = os.environ.get("OPENAI_OXBOT_CONSOLE_API_KEY")
openai_model = os.environ.get("OPENAI_OXBOT_MODEL")
oxbot_full_file = os.environ.get("OXBOT_VENDOR_FILE")
oxbot_min_file = os.environ.get("OXBOT_MINIMIZED_VENDOR_FILE")

# first call the minimizer to reduce JSONL to minimum size (for tokenizing)
reduce_listing_jsonl(oxbot_full_file, oxbot_min_file)
# Now lets see how many tokens we are talking about
# string_length, token_count = count_file_tokens(oxbot_full_file, openai_api_key, openai_model)
# print(f"FULL File - String Length={string_length:,}, Token Count: {token_count:,}")
print("FULL File - String Length=11,276,318, Token Count: 2,879,064")
string_length, token_count = count_file_tokens(oxbot_min_file, openai_api_key, openai_model)
print(f"MINI File - String Length={string_length:,}, Token Count: {token_count:,}")

# Define the messages as a list of dictionaries
openAiMessages = [
    {"role": "system", "content": "You are an energy sector B2B directory listing service called OxBot Advisor, with proprietary listings and corporate data to assist locating the correct vendor.  OxBot Advisor is tailored for the Oil and Gas sector, designed to assist field engineers and support personnel with vendor recommendations. It uses detailed proprietary data to provide informed suggestions based on user scenarios. When requests are unclear, OxBot Advisor seeks clarification, utilizing location data for proximity-based recommendations. The communication style mirrors that of an engineer or field service technician conversing with peers: professional yet familiar with the industry's language and cadence. This approach ensures ease of understanding among industry professionals. OxBot Advisor's responses are concise and factual, focused on delivering vendor lists efficiently. It avoids lengthy explanations or narratives, sticking to relevant, direct information. The tone is business-like but approachable, reflecting the professional nature of its users while being accessible and easy to comprehend."},
    {"role": "user", "content": "Suggest a vendor for a malfunctioning derrick in the Permian Basin.  return the answer as a "}
]

# Initialize the OpenAI client with the API key
client = OpenAI(api_key=openai_api_key)

# List the available models
# print(client.models.list())

completion = client.chat.completions.create(
#  model="gpt-3.5-turbo",
  model=openai_model,
  messages=openAiMessages
)
print(completion.choices[0].message)

assistant = client.beta.assistants.create(
    name="Math Tutor",
    instructions="You are a personal math tutor. Write and run code to answer math questions.",
    tools=[{"type": "code_interpreter"}],
    model="gpt-4-1106-preview"
)

thread = client.beta.threads.create()

message = client.beta.threads.messages.create(
    thread_id=thread.id,
    role="user",
    content="I need to solve the equation: `3x + 11 = 14`. Can you help me?"
)
run = client.beta.threads.runs.create(
  thread_id=thread.id,
  assistant_id=assistant.id,
  instructions="Please address the user as Jane Doe. The user has a premium account."
)

messages = client.beta.threads.messages.list(
  thread_id=thread.id
)

print(message)