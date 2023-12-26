import time
import subprocess
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler

class MyHandler(FileSystemEventHandler):
    def on_modified(self, event):
        # This function is called when a file/directory is modified.
        print(f"Event type: {event.event_type} - Path: {event.src_path}")
        # Trigger the build process
        if self.build_package():
            self.install_package()

    def build_package(self):
        print("Attempting to build the package...")
        # Replace with the appropriate command for building your package
        process = subprocess.run(['python', 'setup.py', 'sdist', 'bdist_wheel'], cwd='path/to/ox_svc')
        return process.returncode == 0  # Return True if build was successful

    def install_package(self):
        print("Building and installing the package...")
        # Assuming the new wheel file is created in the dist/ directory and named according to the version of the package
        # You might need logic here to determine the name of the newly created .whl file or to always create it with a known name.
        package_wheel_file = 'dist/ox_svc-0.1.0-py3-none-any.whl'  # Update with actual file naming convention
        subprocess.run(['pip', 'install', '--upgrade', package_wheel_file], cwd='path/to/ox_svc')


if __name__ == "__main__":
    path = r'E:\oxai\src\OxBot\ox_svc'
    event_handler = MyHandler()
    observer = Observer()
    observer.schedule(event_handler, path, recursive=False)
    observer.start()

    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()

    observer.join()
