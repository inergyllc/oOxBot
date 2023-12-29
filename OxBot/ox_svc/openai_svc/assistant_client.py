import json
import os
import time
from openai import OpenAI
from dotenv import load_dotenv
from ox_svc import ConfigManager, AssistantManager

class OpenAIAssistantClient:
    
    # CTOR & INIT
    #region CTOR & INIT
    
    def __init__(self, assistant_name):
        self._load_environment()
        self.env_conf = ConfigManager(self.env_conf_map)
        self.assistant_name = assistant_name
        self.assist_conf = self._get_assistant_config(assistant_name)
        self.client = self._create_client()
        self.assistant = self._create_assistant()
        self.thread = self._create_thread()
        self.run = None
        self.message = None
        self.messages = None
        self.question = None

    def _load_environment(self):
        load_dotenv()
        self.env_conf_map = {
            "api_key": "OPENAI_DEFAULT_API_KEY",
            "model": "OPENAI_DEFAULT_MODEL",
            "assist_conf_file": "OPENAI_ASSISTANTS_CONFIG_FILE"
        }
        
    #endregion CTOR & INIT

    # OPENAI FNs
    #region OPENAI FNs

    def _get_assistant_config(self, assistant_name):
        assist_conf = AssistantManager(self.env_conf.assist_conf_file).get_assistant_by_name(assistant_name)
        if not assist_conf:
            raise ValueError(f"Assistant '{assistant_name}' not found")
        return assist_conf

    def _create_client(self):
        return OpenAI(api_key=self.env_conf.api_key)

    def _create_assistant(self):
        return self.client.beta.assistants.create(
            name=self.assist_conf.name,
            instructions=self.assist_conf.instructions,
            tools=[{"type": "code_interpreter"}],
            # tools=self.assist_conf.tools,
            model=self.env_conf.model
        )

    def _create_thread(self):
        return self.client.beta.threads.create()

    def _send_message(self, question):
        message = self.client.beta.threads.messages.create(
            thread_id=self.thread.id,
            role="user",
            content=question
        )
        self.message = message

    def _create_run(self):
        return self.client.beta.threads.runs.create(
            thread_id=self.thread.id,
            assistant_id=self.assistant.id
        )

    def wait_for_run_to_complete(
            self, 
            max_wait_seconds=30.0, 
            sleep_seconds=0.5):
        start_time = time.time()
        while True:
            # Retrieve current run status
            run_status = self.client.beta.threads.runs.retrieve(
                thread_id=self.thread.id,
                run_id=self.run.id
            )

            # Check if run is completed
            if run_status.completed_at is not None:
                return True  # Success

            # Check for timeout
            elapsed_time = time.time() - start_time
            if elapsed_time >= max_wait_seconds:
                return False  # Timeout failure

            time.sleep(sleep_seconds)  # Sleep for a while before checking again

            
    def _retrieve_run_responses(self):
        self.wait_for_run_to_complete()
        self.messages  = self.client.beta.threads.messages.list(thread_id=self.thread.id)
        
    #endregion OPENAI FNs

    # EXTERNAL FNs
    #region EXTERNAL FNs

    def ask_question(self, question):
        self.question = question
        self._send_message(question)
        self.run = self._create_run()
        succeeded_in_time = self._retrieve_run_responses(30,.5)
        # Extract and reverse the messages from the assistant
        response = ([msg.content[0].text.value for msg in reversed(self.messages.data) if msg.role == "assistant"]
            if succeeded_in_time
            else ["Sorry, I'm taking too long to respond. Please try again later."])
        return response

    #endregion EXTERNAL FNs

# Example usage:
# client = OpenAIAssistantClient("OxBot Advisor")
# client.ask_question("I need to solve the equation `u^2 - 5u -14 = 0`. Please show your work.")
# for message in client.messages:
#     print(message.content[0].text.value if message.role == "assistant" else message.Content)
