import json
import openai
import os
import tiktoken

def count_file_tokens(file_path, openai_api_key, model_name):
    # Read the content of the specified file
    with open(file_path, 'r', encoding='utf-8') as file:
        text = file.read()

    # Get the encoding for the specified model
    enc = tiktoken.encoding_for_model(model_name)

    # Encode the text into tokens
    encoded_tokens = enc.encode(text)

    # Decode the encoded tokens to verify the result (optional)
    decoded_text = enc.decode(encoded_tokens)

    # Return the string length and token count
    return len(decoded_text), len(encoded_tokens)

## Example usage
# openai_model = "gpt-4-1106-preview"
# oxbot_file = "path_to_your_file.txt"  # Replace with your file path
# string_length, token_count = count_file_tokens(oxbot_file, openai_model)
# print(f"String Length={string_length:,}, Token Count: {token_count:,}")
