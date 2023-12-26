# openai_file_manager.py
import os
import openai

class FileManager:
    def __init__(self, api_key):
        openai.api_key = api_key

    def upload_file(self, file_path, purpose='assistants'):
        with open(file_path, 'rb') as file:
            response = openai.File.create(file=file, purpose=purpose)
        return response.id

    def list_files(self, purpose=None):
        response = openai.File.list(purpose=purpose)
        return response.data

    def retrieve_file(self, file_id):
        response = openai.File.retrieve(file_id)
        return response

    def delete_file(self, file_id):
        response = openai.File.delete(file_id)
        return response.deleted

    def find_file_by_name(self, file_path_or_name):
        # Extract the base name of the file in case a path is provided
        file_name = os.path.basename(file_path_or_name)

        files = self.list_files()  # Retrieve all files
        for file in files:
            if file_name == file['filename']:  # Check if the file name matches exactly
                return file['id']
        return None  # Return None if no matching file is found


# Example usage:
# You would use this module in another script as follows:

# from openai_file_manager import OpenAIFileManager
# manager = OpenAIFileManager('your-api-key')
# upload_id = manager.upload_file('path/to/your/file.jsonl')
# print("Uploaded File ID:", upload_id)
# print("List of Files:", manager.list_files())
# print("Retrieve Specific File:", manager.retrieve_file(upload_id))
# print("Delete File:", manager.delete_file(upload_id))

