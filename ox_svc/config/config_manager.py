# config_manager.py

import os

class NestedConfig:
    def __init__(self, mappings):
        self._initialize_nested_configs(mappings)

    def _initialize_nested_configs(self, mappings):
        for attr, env_var in mappings.items():
            self._set_nested_attr(attr.split("."), env_var)

    def _set_nested_attr(self, attr_list, env_var):
        if len(attr_list) == 1:
            setattr(self, attr_list[0], os.environ.get(env_var))
        else:
            nested_attr = attr_list[0]
            if not hasattr(self, nested_attr):
                setattr(self, nested_attr, NestedConfig({}))
            nested_config = getattr(self, nested_attr)
            nested_config._set_nested_attr(attr_list[1:], env_var)

class ConfigManager(NestedConfig):
    def __init__(self, mappings=None):
        default_mappings = {
            "oai.advisor.api_key": "OPENAI_OXBOT_CONSOLE_API_KEY",
            "oai.advisor.model": "OPENAI_OXBOT_MODEL",
            "vendor.original_file": "OXBOT_VENDOR_FILE",
            "vendor.minimized_file": "OXBOT_VENDOR_MINIMIZED_FILE"
        }
        mappings = mappings if mappings else default_mappings
        super().__init__(mappings)

''' Example usage
if __name__ == "__main__":
    os.environ['OPENAI_OXBOT_CONSOLE_API_KEY'] = 'your_key_here'  # Mocking environment variable
    os.environ['OPENAI_OXBOT_MODEL'] = 'your_model_here'          # Mocking environment variable
    os.environ['OXBOT_VENDOR_FILE'] = 'file_path_here'                    # Mocking environment variable
    os.environ['OXBOT_VENDOR_MINIMIZED_FILE'] = 'minimized_file_path_here'  # Mocking environment variable

    config = ConfigManager()

    print(config.oai.advisor.api_key)        # Accesses OPENAI_OXBOT_CONSOLE_API_KEY
    print(config.oai.advisor.model)          # Accesses OPENAI_OXBOT_MODEL
    print(config.vendor.original_file)       # Accesses OXBOT_VENDOR_FILE
    print(config.vendor.minimized_file)      # Accesses OXBOT_VENDOR_MINIMIZED_FILE
'''