import json

class AssistantConfig:
    def __init__(self, **kwargs):
        self.__dict__.update(kwargs)

    @property
    def tools(self):
        return [{'type': tool} for tool in self.__dict__.get('tools', [])]

class AssistantManager:
    def __init__(self, filepath):
        self.filepath = filepath
        self.config_prefix = None
        self.env_prefix = None
        self.assistants = self._load_data()

    def _load_data(self):
        with open(self.filepath, 'r') as file:
            data = json.load(file)
            self.config_prefix = data.get('config_prefix', '')
            self.env_prefix = data.get('env_prefix', '')
            return {assistant['name']: AssistantConfig(**assistant) for assistant in data.get('assistants', [])}

    def get_assistant_by_name(self, name):
        # Convert the input name to lower case for case-insensitive comparison
        lower_name = name.lower()
        # Find the assistant with a name matching the lower_name (case-insensitive)
        for assistant_name, assistant_config in self.assistants.items():
            if assistant_name.lower() == lower_name:
                return assistant_config
        return None

# Example usage:
# manager = AssistantManager('path_to_your_json_file.json')
# assistant = manager.get_assistant_by_name('OxBot Advisor')
# if assistant:
#     print(assistant.name)  # Now you can use dot notation
# else:
#     print('Assistant not found')
