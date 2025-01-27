<div align="center">

#  **.Net Agentic Abstract Analysis Framework**   
_This is an illustration of cdascientitst's use of an agentic framework to facilitate analysis between agents of Machine Learning Output._
 **Download Project**  
   ```bash
   https://github.com/cdascientist/AgenticExample.git
   ```

---

## 🔄 **Installation Steps**  
These instrctions are designed to illustrate an implementation of Agentic architecture and instructing Agents to use machine learning and machine learning results to formulate analysis:

 **Lets generate a temporary agentic Framework**  
   ```bash
   https://colab.research.google.com/
   ```
   
   ```python
   + New notebook
   ```
   
   **Copy and paste to: Install of Ollama, Download and install TinyLlama, Download and install NGROK, Create a tunnle to TinyLlama through NGROK** 
   ```python
   #!/usr/bin/env python3
import subprocess
import os
import sys
import json
import time
import requests
import platform
import shutil
import threading
import tempfile
import logging
from logging.handlers import RotatingFileHandler
import socket

# Configure logging
LOG_FILE = os.path.join(os.getcwd(), 'ollama_installer.log')
logger = logging.getLogger('OllamaInstaller')
logger.setLevel(logging.DEBUG)

console_handler = logging.StreamHandler(sys.stdout)
file_handler = RotatingFileHandler(LOG_FILE, maxBytes=5*1024*1024, backupCount=2)

console_format = logging.Formatter('%(asctime)s - %(levelname)s - %(message)s')
file_format = logging.Formatter('%(asctime)s - %(levelname)s - %(process)d - %(message)s')
console_handler.setFormatter(console_format)
file_handler.setFormatter(file_format)

logger.addHandler(console_handler)
logger.addHandler(file_handler)

def status_indicator(message, success=True):
    status = "✅ SUCCESS" if success else "❌ FAILED"
    print(f"{status} {message}")
    logger.info(f"{status} {message}")

def download_file(url, path):
    try:
        logger.debug(f"Downloading {url} to {path}")
        response = requests.get(url, stream=True, timeout=20)
        total_size = int(response.headers.get('content-length', 0))
        block_size = 1024
        downloaded = 0

        with open(path, 'wb') as file:
            for data in response.iter_content(block_size):
                downloaded += len(data)
                file.write(data)
                progress = (downloaded / total_size) * 100 if total_size > 0 else 0
                print(f"\rDownloading: {progress:.2f}%", end='', flush=True)

        print("\rDownload complete.                ")
        logger.info(f"Downloaded {url} to {path}")
        status_indicator(f"Downloaded {url} to {path}")
        return True
    except Exception as e:
        logger.error(f"Download failed from {url}: {str(e)}")
        status_indicator(f"Download failed from {url}: {str(e)}", False)
        return False

def install_ollama():
    try:
        script_path = os.path.join(tempfile.gettempdir(), 'install.sh')
        url = "https://ollama.ai/install.sh"

        if not download_file(url, script_path):
            return False

        os.chmod(script_path, 0o755)
        subprocess.run(['sh', script_path], check=True, capture_output=True, text=True)
        logger.info("Ollama installed successfully")
        status_indicator("Ollama installed successfully")
        return True
    except Exception as e:
        logger.error(f"Ollama installation error: {str(e)}")
        status_indicator(f"Ollama installation error: {str(e)}", False)
        return False

def start_ollama_server():
    try:
        def run_server():
            logger.debug("Starting Ollama server process")
            env = os.environ.copy()
            env['OLLAMA_HOST'] = '0.0.0.0'
            subprocess.run(['ollama', 'serve'],
                         stdout=subprocess.PIPE,
                         stderr=subprocess.PIPE,
                         env=env)

        server_thread = threading.Thread(target=run_server)
        server_thread.daemon = True
        server_thread.start()
        time.sleep(2)

        for attempt in range(5):
            try:
                headers = {"Accept": "application/json"}
                response = requests.get("http://localhost:11434/api/version",
                                     headers=headers,
                                     timeout=5)
                if response.status_code == 200:
                    logger.info("Ollama server started successfully")
                    status_indicator("Ollama server started successfully")
                    return True
            except requests.RequestException:
                time.sleep(2)

        logger.error("Ollama server failed to start")
        status_indicator("Ollama server failed to start", False)
        return False
    except Exception as e:
        logger.error(f"Ollama server start error: {str(e)}")
        status_indicator(f"Ollama server start error: {str(e)}", False)
        return False

def pull_and_run_tinyllama():
    try:
        logger.debug("Starting TinyLlama model download")
        result = subprocess.run(
            ['ollama', 'pull', 'tinyllama'],
            capture_output=True,
            text=True,
            timeout=600
        )

        if result.returncode != 0:
            raise Exception(f"Model pull failed: {result.stderr}")

        logger.info("TinyLlama model downloaded successfully")
        status_indicator("TinyLlama model downloaded successfully")

        test_response = subprocess.run(
            ['ollama', 'run', 'tinyllama', 'Hello, can you respond?'],
            capture_output=True,
            text=True,
            timeout=120
        )

        if test_response.returncode == 0:
            logger.info("TinyLlama model running successfully")
            status_indicator("TinyLlama model running successfully")
            return True

        raise Exception("Model test failed")
    except Exception as e:
        logger.error(f"TinyLlama error: {str(e)}")
        status_indicator(f"TinyLlama error: {str(e)}", False)
        return False

def install_ngrok():
    try:
        system = platform.system().lower()
        arch = 'arm64' if platform.machine().startswith('aarch64') else 'amd64'
        url = f"https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-{system}-{arch}.tgz"
        tgz_path = os.path.join(tempfile.gettempdir(), 'ngrok.tgz')

        if not download_file(url, tgz_path):
            return False

        import tarfile
        logger.debug("Extracting Ngrok from tarball")
        with tarfile.open(tgz_path) as tar:
            tar.extract('ngrok', path=tempfile.gettempdir())

        ngrok_path = os.path.join(tempfile.gettempdir(), 'ngrok')
        os.chmod(ngrok_path, 0o755)
        shutil.move(ngrok_path, '/usr/local/bin/ngrok')
        os.remove(tgz_path)

        logger.info("Ngrok installed successfully")
        status_indicator("Ngrok installed successfully")
        return True
    except Exception as e:
        logger.error(f"Ngrok installation failed: {str(e)}")
        status_indicator(f"Ngrok installation failed: {str(e)}", False)
        return False

def setup_ngrok(token):
    try:
        logger.debug("Configuring Ngrok with authentication token")

        # Remove any existing config
        config_dir = os.path.expanduser('~/.ngrok2')
        if os.path.exists(config_dir):
            shutil.rmtree(config_dir)

        subprocess.run(['ngrok', 'config', 'add-authtoken', token], check=True)
        logger.info("Ngrok token configured")
        status_indicator("Ngrok token configured")
        return True
    except Exception as e:
        logger.error(f"Ngrok token configuration failed: {str(e)}")
        status_indicator(f"Ngrok token configuration failed: {str(e)}", False)
        return False

def wait_for_ngrok_web():
    max_attempts = 10
    for i in range(max_attempts):
        try:
            response = requests.get("http://localhost:4040/status", timeout=1)
            if response.status_code == 200:
                return True
        except requests.RequestException:
            time.sleep(2)
    return False

def create_ngrok_tunnel(port=11434):
    try:
        logger.debug("Killing any existing Ngrok processes")
        try:
            subprocess.run(['pkill', '-f', 'ngrok'], capture_output=True)
            time.sleep(3)
        except subprocess.CalledProcessError:
            pass

        ngrok_cmd = [
            'ngrok', 'http',
            '--log=stdout',
            '--log-level=info',
            '--region=us',
            f'--authtoken={os.getenv("NGROK_TOKEN", "2rvdwgdJEb2yEMP9lDhUyxWHFxl_5JB43VinTGtB9ntnsbxJV")}',
            str(port)
        ]

        with open('ngrok.log', 'w') as log_file:
            subprocess.Popen(ngrok_cmd, stdout=log_file, stderr=subprocess.STDOUT)

        if not wait_for_ngrok_web():
            raise Exception("Ngrok web interface failed to start")

        max_attempts = 5
        for attempt in range(max_attempts):
            try:
                response = requests.get("http://localhost:4040/api/tunnels", timeout=5)
                tunnels = response.json().get('tunnels', [])
                if tunnels:
                    tunnel_url = tunnels[0]['public_url']
                    logger.info(f"Ngrok Tunnel Created: {tunnel_url}")
                    status_indicator(f"Ngrok Tunnel Created: {tunnel_url}")
                    return tunnel_url
            except requests.RequestException as e:
                if attempt < max_attempts - 1:
                    logger.warning(f"Attempt {attempt + 1} failed, retrying: {str(e)}")
                    time.sleep(2)
                continue

        raise Exception("Failed to get tunnel URL after maximum attempts")
    except Exception as e:
        logger.error(f"Ngrok tunnel creation failed: {str(e)}")
        status_indicator(f"Ngrok tunnel creation failed: {str(e)}", False)
        return None

def verify_tinyllama_access(ngrok_url):
    try:
        logger.debug(f"Verifying access to TinyLlama via {ngrok_url}")
        payload = {
            "model": "tinyllama",
            "prompt": "Hello, can you respond?",
            "stream": False
        }
        headers = {
            "Accept": "application/json",
            "Content-Type": "application/json"
        }

        for attempt in range(3):
            try:
                response = requests.post(
                    f"{ngrok_url}/api/generate",
                    json=payload,
                    headers=headers,
                    timeout=30
                )
                if response.status_code == 200:
                    logger.info("TinyLlama model access verified")
                    status_indicator("TinyLlama model access verified")
                    return True
            except Exception as e:
                logger.warning(f"Attempt {attempt + 1} failed: {str(e)}")
                if attempt < 2:
                    time.sleep(5)

        raise Exception("All verification attempts failed")
    except Exception as e:
        logger.error(f"TinyLlama access error: {str(e)}")
        status_indicator(f"TinyLlama access error: {str(e)}", False)
        return False

def start_chat_session(ngrok_url):
    try:
        logger.info("Starting chat session...")
        print("\n=== TinyLlama Chat Session ===")
        print("Type 'exit' to end the chat\n")

        while True:
            user_input = input("You: ").strip()
            if user_input.lower() == 'exit':
                break

            payload = {
                "model": "tinyllama",
                "prompt": user_input,
                "stream": False
            }
            headers = {
                "Accept": "application/json",
                "Content-Type": "application/json"
            }

            try:
                response = requests.post(
                    f"{ngrok_url}/api/generate",
                    json=payload,
                    headers=headers,
                    timeout=30
                )
                response.raise_for_status()
                response_data = response.json()
                print(f"\nTinyLlama: {response_data['response']}\n")
            except Exception as e:
                print(f"\nError: Failed to get response - {str(e)}\n")
                logger.error(f"Chat error: {str(e)}")

    except KeyboardInterrupt:
        print("\nChat session terminated.")
    except Exception as e:
        logger.error(f"Chat session error: {str(e)}")
        print(f"\nError: {str(e)}")

def main():
    ngrok_token = "2rvdwgdJEb2yEMP9lDhUyxWHFxl_5JB43VinTGtB9ntnsbxJV"

    if not shutil.which('ollama'):
        if not install_ollama():
            return

    if not start_ollama_server():
        return

    if not pull_and_run_tinyllama():
        return

    if not shutil.which('ngrok'):
        if not install_ngrok():
            return

    if not setup_ngrok(ngrok_token):
        return

    ngrok_url = create_ngrok_tunnel(11434)
    if not ngrok_url:
        return

    if verify_tinyllama_access(ngrok_url):
        logger.info(f"Installation complete. TinyLlama is accessible at {ngrok_url}")
        status_indicator("TinyLlama setup complete")
        start_chat_session(ngrok_url)
    else:
        status_indicator("Failed to verify TinyLlama access", False)

if __name__ == "__main__":
    main()
   ```


   **Automated Script Output after Creating, Installing, and Tunnling to a small LLM:** 
   ```bash
  
   **Example Output:

INFO:OllamaInstaller:✅ SUCCESS Ngrok token configured

2025-01-25 16:43:50,392 - DEBUG - Killing any existing Ngrok processes

DEBUG:OllamaInstaller:Killing any existing Ngrok processes

2025-01-25 16:43:55,459 - INFO - Ngrok Tunnel Created: https://da31-34-74-128-6.ngrok-free.app

INFO:OllamaInstaller:Ngrok Tunnel Created: https://da31-34-74-128-6.ngrok-free.app

✅ SUCCESS Ngrok Tunnel Created: https://da31-34-74-128-6.ngrok-free.app
2025-01-25 16:43:55,464 - INFO - ✅ SUCCESS Ngrok Tunnel Created: https://da31-34-74-128-6.ngrok-free.app

INFO:OllamaInstaller:✅ SUCCESS Ngrok Tunnel Created: https://da31-34-74-128-6.ngrok-free.app

2025-01-25 16:43:55,472 - DEBUG - Verifying access to TinyLlama via https://da31-34-74-128-6.ngrok-free.app

DEBUG:OllamaInstaller:Verifying access to TinyLlama via https://da31-34-74-128-6.ngrok-free.app

2025-01-25 16:44:25,606 - WARNING - Attempt 1 failed: HTTPSConnectionPool(host='da31-34-74-128-6.ngrok-free.app', port=443): Read timed out. (read timeout=30)

WARNING:OllamaInstaller:Attempt 1 failed: HTTPSConnectionPool(host='da31-34-74-128-6.ngrok-free.app', port=443): Read timed out. (read timeout=30)

2025-01-25 16:44:47,100 - INFO - TinyLlama model access verified

INFO:OllamaInstaller:TinyLlama model access verified

✅ SUCCESS TinyLlama model access verified
2025-01-25 16:44:47,106 - INFO - ✅ SUCCESS TinyLlama model access verified

INFO:OllamaInstaller:✅ SUCCESS TinyLlama model access verified

2025-01-25 16:44:47,109 - INFO - Installation complete. TinyLlama is accessible at https://da31-34-74-128-6.ngrok-free.app

INFO:OllamaInstaller:Installation complete. TinyLlama is accessible at https://da31-34-74-128-6.ngrok-free.app

✅ SUCCESS TinyLlama setup complete
2025-01-25 16:44:47,112 - INFO - ✅ SUCCESS TinyLlama setup complete

INFO:OllamaInstaller:✅ SUCCESS TinyLlama setup complete

2025-01-25 16:44:47,115 - INFO - Starting chat session...

INFO:OllamaInstaller:Starting chat session...


=== TinyLlama Chat Session ===
Type 'exit' to end the chat

You: 
   
   ```

   **Install Dependency:**  
   ```Powershell
Install-Package AutoGen
Install-Package Accord
Install-Package Accord.MachineLearning
Install-Package Accord.Math
Install-Package Accord.Statistics
Install-Package Microsoft.AspNet.WebApi.Core
Install-Package Microsoft.AspNetCore.Cors
Install-Package Microsoft.AspNetCore.OpenApi -Version 8.0.1
Install-Package Microsoft.AspNetCore.SpaProxy
Install-Package Microsoft.EntityFrameworkCore
Install-Package Microsoft.EntityFrameworkCore.InMemory
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Newtonsoft.Json
Install-Package NumSharp
Install-Package SciSharp.TensorFlow.Redist
Install-Package Swashbuckle.AspNetCore
Install-Package Swashbuckle.AspNetCore.Swagger
Install-Package System.Net.Http
Install-Package System.Text.Json
Install-Package TensorFlow.NET
Install-Package Microsoft.SemanticKernel 
   ```
      
### Once you execute the snippit to create the TinyLlama instance Pleae Update NgRok Tunnle


```csharp
 public static class AppConfig
    {
        private static string _ngrokTunnelAddress = "019e-34-74-45-28.ngrok-free.app";
        private static string _protocol = "https://";

```


### Then Just Run the script; here is example output
```console
=== INUV Stock Analysis System Starting ===

Executing Phase One - Data Retrieval
Initializing Phase One - Data Retrieval
INUV High Array:
DateTime: 2025-01-24 15:30:00, High: 0.51629901, Volume: 24408
DateTime: 2025-01-24 15:00:00, High: 0.51770002, Volume: 141045
DateTime: 2025-01-24 14:30:00, High: 0.51630002, Volume: 26377
DateTime: 2025-01-24 14:00:00, High: 0.51719999, Volume: 71271
DateTime: 2025-01-24 13:30:00, High: 0.51999998, Volume: 73734
DateTime: 2025-01-24 13:00:00, High: 0.52509999, Volume: 41078
DateTime: 2025-01-24 12:30:00, High: 0.52850002, Volume: 93781
DateTime: 2025-01-24 12:00:00, High: 0.53130001, Volume: 92300
DateTime: 2025-01-24 11:30:00, High: 0.51980001, Volume: 62247
DateTime: 2025-01-24 11:00:00, High: 0.50860000, Volume: 45014
DateTime: 2025-01-24 10:30:00, High: 0.52190000, Volume: 108991
DateTime: 2025-01-24 10:00:00, High: 0.52200001, Volume: 74758
DateTime: 2025-01-24 09:30:00, High: 0.55000001, Volume: 184099
DateTime: 2025-01-23 15:30:00, High: 0.536, Volume: 83458
DateTime: 2025-01-23 15:00:00, High: 0.53250003, Volume: 19305
DateTime: 2025-01-23 14:30:00, High: 0.52749997, Volume: 19032
DateTime: 2025-01-23 14:00:00, High: 0.53020000, Volume: 79507
DateTime: 2025-01-23 13:30:00, High: 0.52700001, Volume: 28528
DateTime: 2025-01-23 13:00:00, High: 0.53404999, Volume: 175941
DateTime: 2025-01-23 12:30:00, High: 0.54149997, Volume: 268074

INUV Low Array:
DateTime: 2025-01-24 15:30:00, Low: 0.50720000, Volume: 24408
DateTime: 2025-01-24 15:00:00, Low: 0.50700003, Volume: 141045
DateTime: 2025-01-24 14:30:00, Low: 0.50700003, Volume: 26377
DateTime: 2025-01-24 14:00:00, Low: 0.50680000, Volume: 71271
DateTime: 2025-01-24 13:30:00, Low: 0.50599998, Volume: 73734
DateTime: 2025-01-24 13:00:00, Low: 0.51520002, Volume: 41078
DateTime: 2025-01-24 12:30:00, Low: 0.51510000, Volume: 93781
DateTime: 2025-01-24 12:00:00, Low: 0.51520002, Volume: 92300
DateTime: 2025-01-24 11:30:00, Low: 0.50779998, Volume: 62247
DateTime: 2025-01-24 11:00:00, Low: 0.50120002, Volume: 45014
DateTime: 2025-01-24 10:30:00, Low: 0.5, Volume: 108991
DateTime: 2025-01-24 10:00:00, Low: 0.50500000, Volume: 74758
DateTime: 2025-01-24 09:30:00, Low: 0.51300001, Volume: 184099
DateTime: 2025-01-23 15:30:00, Low: 0.52149999, Volume: 83458
DateTime: 2025-01-23 15:00:00, Low: 0.52139997, Volume: 19305
DateTime: 2025-01-23 14:30:00, Low: 0.52060002, Volume: 19032
DateTime: 2025-01-23 14:00:00, Low: 0.51999998, Volume: 79507
DateTime: 2025-01-23 13:30:00, Low: 0.51870000, Volume: 28528
DateTime: 2025-01-23 13:00:00, Low: 0.51300001, Volume: 175941
DateTime: 2025-01-23 12:30:00, Low: 0.52999997, Volume: 268074
Phase One execution time: 4.132 seconds

Executing Phase Two - Machine Learning Analysis
Initializing Phase Two - Machine Learning Analysis
High Cluster Vector Magnitude: 0.9204

INUV High ML Clusters (Ordered by time and volume):

Cluster Average: 0.5178
Time: 10:00 AM, Volume: 74758
Time: 10:30 AM, Volume: 108991
Time: 11:00 AM, Volume: 45014
Time: 11:30 AM, Volume: 62247
Time: 01:30 PM, Volume: 73734
Time: 02:00 PM, Volume: 71271
Time: 02:30 PM, Volume: 26377
Time: 03:00 PM, Volume: 141045
Time: 03:30 PM, Volume: 24408

Cluster Average: 0.5302
Time: 01:00 PM, Volume: 175941
Time: 01:30 PM, Volume: 28528
Time: 02:00 PM, Volume: 79507
Time: 02:30 PM, Volume: 19032
Time: 03:00 PM, Volume: 19305
Time: 03:30 PM, Volume: 83458
Time: 12:00 PM, Volume: 92300
Time: 12:30 PM, Volume: 93781
Time: 01:00 PM, Volume: 41078

Cluster Average: 0.5457
Time: 12:30 PM, Volume: 268074
Time: 09:30 AM, Volume: 184099
Low Cluster Vector Magnitude: 0.8966

INUV Low ML Clusters (Ordered by time and volume):

Cluster Average: 0.5053
Time: 10:00 AM, Volume: 74758
Time: 10:30 AM, Volume: 108991
Time: 11:00 AM, Volume: 45014
Time: 11:30 AM, Volume: 62247
Time: 01:30 PM, Volume: 73734
Time: 02:00 PM, Volume: 71271
Time: 02:30 PM, Volume: 26377
Time: 03:00 PM, Volume: 141045
Time: 03:30 PM, Volume: 24408

Cluster Average: 0.5174
Time: 01:00 PM, Volume: 175941
Time: 01:30 PM, Volume: 28528
Time: 02:00 PM, Volume: 79507
Time: 02:30 PM, Volume: 19032
Time: 03:00 PM, Volume: 19305
Time: 03:30 PM, Volume: 83458
Time: 09:30 AM, Volume: 184099
Time: 12:00 PM, Volume: 92300
Time: 12:30 PM, Volume: 93781
Time: 01:00 PM, Volume: 41078

Cluster Average: 0.5300
Time: 12:30 PM, Volume: 268074
Phase Two execution time: 0.804 seconds

Starting parallel execution of Phase Three and Four
Initializing Phase Three - TinyLlama Communication

=== Retrieved Values in Phase Three ===

INUV High Values:
DateTime: 2025-01-24 15:30:00, High: 0.51629901, Volume: 24408
DateTime: 2025-01-24 15:00:00, High: 0.51770002, Volume: 141045
DateTime: 2025-01-24 14:30:00, High: 0.51630002, Volume: 26377
DateTime: 2025-01-24 14:00:00, High: 0.51719999, Volume: 71271
DateTime: 2025-01-24 13:30:00, High: 0.51999998, Volume: 73734
DateTime: 2025-01-24 13:00:00, High: 0.52509999, Volume: 41078
DateTime: 2025-01-24 12:30:00, High: 0.52850002, Volume: 93781
DateTime: 2025-01-24 12:00:00, High: 0.53130001, Volume: 92300
DateTime: 2025-01-24 11:30:00, High: 0.51980001, Volume: 62247
DateTime: 2025-01-24 11:00:00, High: 0.50860000, Volume: 45014
DateTime: 2025-01-24 10:30:00, High: 0.52190000, Volume: 108991
DateTime: 2025-01-24 10:00:00, High: 0.52200001, Volume: 74758
DateTime: 2025-01-24 09:30:00, High: 0.55000001, Volume: 184099
DateTime: 2025-01-23 15:30:00, High: 0.536, Volume: 83458
DateTime: 2025-01-23 15:00:00, High: 0.53250003, Volume: 19305
DateTime: 2025-01-23 14:30:00, High: 0.52749997, Volume: 19032
DateTime: 2025-01-23 14:00:00, High: 0.53020000, Volume: 79507
DateTime: 2025-01-23 13:30:00, High: 0.52700001, Volume: 28528
DateTime: 2025-01-23 13:00:00, High: 0.53404999, Volume: 175941
DateTime: 2025-01-23 12:30:00, High: 0.54149997, Volume: 268074

INUV High ML Clusters:

Cluster Average: 0.5178
Time: 10:00 AM, Volume: 74758
Time: 10:30 AM, Volume: 108991
Time: 11:00 AM, Volume: 45014
Time: 11:30 AM, Volume: 62247
Time: 01:30 PM, Volume: 73734
Time: 02:00 PM, Volume: 71271
Time: 02:30 PM, Volume: 26377
Time: 03:00 PM, Volume: 141045
Time: 03:30 PM, Volume: 24408

Cluster Average: 0.5302
Time: 01:00 PM, Volume: 175941
Time: 01:30 PM, Volume: 28528
Time: 02:00 PM, Volume: 79507
Time: 02:30 PM, Volume: 19032
Time: 03:00 PM, Volume: 19305
Time: 03:30 PM, Volume: 83458
Time: 12:00 PM, Volume: 92300
Time: 12:30 PM, Volume: 93781
Time: 01:00 PM, Volume: 41078

Cluster Average: 0.5457
Time: 12:30 PM, Volume: 268074
Time: 09:30 AM, Volume: 184099

High Cluster Vector Magnitude: 0.9204

=== End of Retrieved Values ===

? Phase Three Progress: 0% - Starting TinyLlama connection...
? Phase Three Progress: 25% - Attempt 1 of 3
?? Phase Three: Sending request to TinyLlama (Attempt 1)...
Initializing Phase Four - TinyLlama Communication

=== Retrieved Values in Phase Four ===

INUV Low Values:
DateTime: 2025-01-24 15:30:00, Low: 0.50720000, Volume: 24408
DateTime: 2025-01-24 15:00:00, Low: 0.50700003, Volume: 141045
DateTime: 2025-01-24 14:30:00, Low: 0.50700003, Volume: 26377
DateTime: 2025-01-24 14:00:00, Low: 0.50680000, Volume: 71271
DateTime: 2025-01-24 13:30:00, Low: 0.50599998, Volume: 73734
DateTime: 2025-01-24 13:00:00, Low: 0.51520002, Volume: 41078
DateTime: 2025-01-24 12:30:00, Low: 0.51510000, Volume: 93781
DateTime: 2025-01-24 12:00:00, Low: 0.51520002, Volume: 92300
DateTime: 2025-01-24 11:30:00, Low: 0.50779998, Volume: 62247
DateTime: 2025-01-24 11:00:00, Low: 0.50120002, Volume: 45014
DateTime: 2025-01-24 10:30:00, Low: 0.5, Volume: 108991
DateTime: 2025-01-24 10:00:00, Low: 0.50500000, Volume: 74758
DateTime: 2025-01-24 09:30:00, Low: 0.51300001, Volume: 184099
DateTime: 2025-01-23 15:30:00, Low: 0.52149999, Volume: 83458
DateTime: 2025-01-23 15:00:00, Low: 0.52139997, Volume: 19305
DateTime: 2025-01-23 14:30:00, Low: 0.52060002, Volume: 19032
DateTime: 2025-01-23 14:00:00, Low: 0.51999998, Volume: 79507
DateTime: 2025-01-23 13:30:00, Low: 0.51870000, Volume: 28528
DateTime: 2025-01-23 13:00:00, Low: 0.51300001, Volume: 175941
DateTime: 2025-01-23 12:30:00, Low: 0.52999997, Volume: 268074

INUV Low ML Clusters:

Cluster Average: 0.5053
Time: 10:00 AM, Volume: 74758
Time: 10:30 AM, Volume: 108991
Time: 11:00 AM, Volume: 45014
Time: 11:30 AM, Volume: 62247
Time: 01:30 PM, Volume: 73734
Time: 02:00 PM, Volume: 71271
Time: 02:30 PM, Volume: 26377
Time: 03:00 PM, Volume: 141045
Time: 03:30 PM, Volume: 24408

Cluster Average: 0.5174
Time: 01:00 PM, Volume: 175941
Time: 01:30 PM, Volume: 28528
Time: 02:00 PM, Volume: 79507
Time: 02:30 PM, Volume: 19032
Time: 03:00 PM, Volume: 19305
Time: 03:30 PM, Volume: 83458
Time: 09:30 AM, Volume: 184099
Time: 12:00 PM, Volume: 92300
Time: 12:30 PM, Volume: 93781
Time: 01:00 PM, Volume: 41078

Cluster Average: 0.5300
Time: 12:30 PM, Volume: 268074

Low Cluster Vector Magnitude: 0.8966

=== End of Retrieved Values ===

? Phase Four Progress: 0% - Starting TinyLlama connection...
? Phase Four Progress: 25% - Attempt 1 of 3
?? Phase Four: Sending request to TinyLlama (Attempt 1)...
?? Phase Three Raw Response: {"id":"chatcmpl-212","object":"chat.completion","created":1737967180,"model":"tinyllama","system_fingerprint":"fp_ollama","choices":[{"index":0,"message":{"role":"assistant","content":"Yes, the patterns observed in Inuvo's stock prices over the past few years demonstrate a pattern of upward fluctuation. The High values are seen in consecutive trends, with high-highs occurring roughly every three months. This indicates that a price reversal is likely to occur towards the end of the current trend or beginning of the next one.\n\nThe clusters identified by the given data set also show an upward movement toward the magnitude of 0.9203624493023799 and Extrapolate projection analysi, which may indicate the presence of a directional tranend or a trending market. The highest High seen is dated on July 14, 2021, when the price reached a maximum of $53.66 before the index decreased by 0.97%. This might be an indicator for the start of the next downward trend.\n\nIn conclusion, Inuvo's stock prices show a pattern of upward fluctuation with high-highs occurring in the intervals between regular price changes. The clusters found by clustering the data set reveal that a reversal is likely to occur in the 1st of next month or in the beginning of the next one, indicating a trending market or formation of a new trend."},"finish_reason":"stop"}],"usage":{"prompt_tokens":179,"completion_tokens":283,"total_tokens":462}}

? Phase Three Progress: 75% - Processing response...
Phase Three TinyLlama Response Content: Yes, the patterns observed in Inuvo's stock prices over the past few years demonstrate a pattern of upward fluctuation. The High values are seen in consecutive trends, with high-highs occurring roughly every three months. This indicates that a price reversal is likely to occur towards the end of the current trend or beginning of the next one.

The clusters identified by the given data set also show an upward movement toward the magnitude of 0.9203624493023799 and Extrapolate projection analysi, which may indicate the presence of a directional tranend or a trending market. The highest High seen is dated on July 14, 2021, when the price reached a maximum of $53.66 before the index decreased by 0.97%. This might be an indicator for the start of the next downward trend.

In conclusion, Inuvo's stock prices show a pattern of upward fluctuation with high-highs occurring in the intervals between regular price changes. The clusters found by clustering the data set reveal that a reversal is likely to occur in the 1st of next month or in the beginning of the next one, indicating a trending market or formation of a new trend.
Phase Three Response Details:
- Model: tinyllama
- Tokens Used: 462
- Finish Reason: stop
? Phase Three Progress: 100% - Success
? SUCCESS Phase Three TinyLlama interaction complete
Phase Three execution time: 89.380 seconds
?? Phase Four Raw Response: {"id":"chatcmpl-671","object":"chat.completion","created":1737967234,"model":"tinyllama","system_fingerprint":"fp_ollama","choices":[{"index":0,"message":{"role":"assistant","content":"Sure! The pattern of downward fluctuation observed in Inuvo's stock prices is indeed also present in the given list. The Cluster Analysis found that the dataset displays directional magnitudes, meaning that some elements have positive values and some negative. In order to confirm this analysis further, it is possible to retrieve specific instances in the List or Map data structure and run operations like Extrapolation with a reference indicator. \n\nHere are some examples:\n1. To get the first instance with upward fluctuation in dateTime, we can use ExtractPoint() method with reference indicator set to true:\n```csharp\n// using System.Collections.Generic;\nusing System.Text.Json;\nusing System.Threading.Tasks;\n\nnamespace ExtractionUsingClusterAnalysis\n{\n    class Program\n    {\n        static async Task Main(string[] args)\n        {\n            string inputPath = @\"C:\\path\\to\\inputfolder\";\n            var fileNames = Directory.EnumerateFiles(inputPath).Select((x, i) =\u003e $\"{i:09}.csv\");\n\n            var data = JsonSerializer.Deserialize\u003cList\u003cSystem.Collections.Generic.List\u003cstring\u003e\u003e\u003e(await File.ReadAllTextAsync(fileNames[0])); // Read CSV files at first\n\n            foreach (var dataItem in data)\n            {\n                Console.WriteLine($\"{dataItem.Count} - {dataItem[dataItem.Count-1].Split(',')[0]}: \");\n                var cluster = data.ClusterLocations().OfType\u003cDouble\u003e().FirstOrDefault();\n                Console.WriteLine(cluster); // Extract cluster location for next 2 elements to check if it is upward fluctuation\n                Console.ReadLine(); // Wait until user press Enter key\n            }\n        }\n    }\n}\n```\n\nIn this case, we use the ClusterLocations() method of the first column's value dataset (data[0] being the first element) and ExtractPoint(clusterLocation, referenceIndicator: true).\n\n2. To get the last instance with downward fluctuation in dateTime, here's an example:\n```csharp\nvar data = JsonSerializer.Deserialize\u003cList\u003cSystem.Collections.Generic.List\u003cstring\u003e\u003e\u003e(await File.ReadAllTextAsync(fileNames[0])); // Read CSV files at first\nforeach (var dataItem in data)\n{\n    Console.WriteLine($\"{dataItem.Count} - {dataItem[dataItem.Count-1].Split(',')[0]}: \"); // Get the 2nd element's cluster location\n    var cluster = data.ClusterLocations().OfType\u003cDouble\u003e().Last(); // Extract last element's cluster location according to previous data\n    Console.WriteLine(cluster); // Check if it is downward fluctuation from last day in same format as above, then exit the while loop and go on with the next data\n}\n```"},"finish_reason":"stop"}],"usage":{"prompt_tokens":175,"completion_tokens":659,"total_tokens":834}}

? Phase Four Progress: 75% - Processing response...
Phase Four TinyLlama Response Content: Sure! The pattern of downward fluctuation observed in Inuvo's stock prices is indeed also present in the given list. The Cluster Analysis found that the dataset displays directional magnitudes, meaning that some elements have positive values and some negative. In order to confirm this analysis further, it is possible to retrieve specific instances in the List or Map data structure and run operations like Extrapolation with a reference indicator.

Here are some examples:
1. To get the first instance with upward fluctuation in dateTime, we can use ExtractPoint() method with reference indicator set to true:
```csharp
// using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExtractionUsingClusterAnalysis
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string inputPath = @"C:\path\to\inputfolder";
            var fileNames = Directory.EnumerateFiles(inputPath).Select((x, i) => $"{i:09}.csv");

            var data = JsonSerializer.Deserialize<List<System.Collections.Generic.List<string>>>(await File.ReadAllTextAsync(fileNames[0])); // Read CSV files at first

            foreach (var dataItem in data)
            {
                Console.WriteLine($"{dataItem.Count} - {dataItem[dataItem.Count-1].Split(',')[0]}: ");
                var cluster = data.ClusterLocations().OfType<Double>().FirstOrDefault();
                Console.WriteLine(cluster); // Extract cluster location for next 2 elements to check if it is upward fluctuation
                Console.ReadLine(); // Wait until user press Enter key
            }
        }
    }
}
```

In this case, we use the ClusterLocations() method of the first column's value dataset (data[0] being the first element) and ExtractPoint(clusterLocation, referenceIndicator: true).

2. To get the last instance with downward fluctuation in dateTime, here's an example:
```csharp
var data = JsonSerializer.Deserialize<List<System.Collections.Generic.List<string>>>(await File.ReadAllTextAsync(fileNames[0])); // Read CSV files at first
foreach (var dataItem in data)
{
    Console.WriteLine($"{dataItem.Count} - {dataItem[dataItem.Count-1].Split(',')[0]}: "); // Get the 2nd element's cluster location
    var cluster = data.ClusterLocations().OfType<Double>().Last(); // Extract last element's cluster location according to previous data
    Console.WriteLine(cluster); // Check if it is downward fluctuation from last day in same format as above, then exit the while loop and go on with the next data
}
```
Phase Four Response Details:
- Model: tinyllama
- Tokens Used: 834
- Finish Reason: stop
? Phase Four Progress: 100% - Success
? SUCCESS Phase Four TinyLlama interaction complete
Phase Four execution time: 141.483 seconds

Executing Phase Five - Final Analysis
Initializing Phase Five - Final Analysis

=== Phase Five Initial Analysis Results ===
High Analysis: Yes, the patterns observed in Inuvo's stock prices over the past few years demonstrate a pattern of upward fluctuation. The High values are seen in consecutive trends, with high-highs occurring roughly every three months. This indicates that a price reversal is likely to occur towards the end of the current trend or beginning of the next one.

The clusters identified by the given data set also show an upward movement toward the magnitude of 0.9203624493023799 and Extrapolate projection analysi, which may indicate the presence of a directional tranend or a trending market. The highest High seen is dated on July 14, 2021, when the price reached a maximum of $53.66 before the index decreased by 0.97%. This might be an indicator for the start of the next downward trend.

In conclusion, Inuvo's stock prices show a pattern of upward fluctuation with high-highs occurring in the intervals between regular price changes. The clusters found by clustering the data set reveal that a reversal is likely to occur in the 1st of next month or in the beginning of the next one, indicating a trending market or formation of a new trend.
Low Analysis: Sure! The pattern of downward fluctuation observed in Inuvo's stock prices is indeed also present in the given list. The Cluster Analysis found that the dataset displays directional magnitudes, meaning that some elements have positive values and some negative. In order to confirm this analysis further, it is possible to retrieve specific instances in the List or Map data structure and run operations like Extrapolation with a reference indicator.

Here are some examples:
1. To get the first instance with upward fluctuation in dateTime, we can use ExtractPoint() method with reference indicator set to true:
```csharp
// using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExtractionUsingClusterAnalysis
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string inputPath = @"C:\path\to\inputfolder";
            var fileNames = Directory.EnumerateFiles(inputPath).Select((x, i) => $"{i:09}.csv");

            var data = JsonSerializer.Deserialize<List<System.Collections.Generic.List<string>>>(await File.ReadAllTextAsync(fileNames[0])); // Read CSV files at first

            foreach (var dataItem in data)
            {
                Console.WriteLine($"{dataItem.Count} - {dataItem[dataItem.Count-1].Split(',')[0]}: ");
                var cluster = data.ClusterLocations().OfType<Double>().FirstOrDefault();
                Console.WriteLine(cluster); // Extract cluster location for next 2 elements to check if it is upward fluctuation
                Console.ReadLine(); // Wait until user press Enter key
            }
        }
    }
}
```

In this case, we use the ClusterLocations() method of the first column's value dataset (data[0] being the first element) and ExtractPoint(clusterLocation, referenceIndicator: true).

2. To get the last instance with downward fluctuation in dateTime, here's an example:
```csharp
var data = JsonSerializer.Deserialize<List<System.Collections.Generic.List<string>>>(await File.ReadAllTextAsync(fileNames[0])); // Read CSV files at first
foreach (var dataItem in data)
{
    Console.WriteLine($"{dataItem.Count} - {dataItem[dataItem.Count-1].Split(',')[0]}: "); // Get the 2nd element's cluster location
    var cluster = data.ClusterLocations().OfType<Double>().Last(); // Extract last element's cluster location according to previous data
    Console.WriteLine(cluster); // Check if it is downward fluctuation from last day in same format as above, then exit the while loop and go on with the next data
}
```
=== End of Initial Analysis Results ===

? Phase Five Progress: 0% - Starting TinyLlama connection...
? Phase Five Progress: 25% - Attempt 1 of 3
?? Phase Five: Sending request to TinyLlama (Attempt 1)...
?? Phase Five Raw Response: {"id":"chatcmpl-565","object":"chat.completion","created":1737967356,"model":"tinyllama","system_fingerprint":"fp_ollama","choices":[{"index":0,"message":{"role":"assistant","content":"Based on the analysis from Inuvo's stock prices, HIGH ANALYSE SHOWS THAT The pattern of downward fluctuation observed in Inuvo's stock prices has been confirmed with high-highs occurring roughly every three months. This indicates that a price reversal is likely to occur towards the end of the current trend or beginning of the next one.\n\nIn summary, the clusters identified by ExtractPoints() (in the given data set) reveal that there are instances of downward fluctuation seen in Inuvo's stock prices. These clusters have average values around -0.22, with peaks occurring at about 7th-9th month of each year. Based on these trends, we can predict the next significant upward movement after April 2021 will be a time frame between May and August. Note that these predictions are based solely on the findings from ExtractPoints() data structure, it is possible to run further operations with ClusterAnalysis or reference Indicator to confirm specific instances."},"finish_reason":"stop"}],"usage":{"prompt_tokens":1062,"completion_tokens":227,"total_tokens":1289}}

? Phase Five Progress: 75% - Processing response...
Phase Five TinyLlama Response Content: Based on the analysis from Inuvo's stock prices, HIGH ANALYSE SHOWS THAT The pattern of downward fluctuation observed in Inuvo's stock prices has been confirmed with high-highs occurring roughly every three months. This indicates that a price reversal is likely to occur towards the end of the current trend or beginning of the next one.

In summary, the clusters identified by ExtractPoints() (in the given data set) reveal that there are instances of downward fluctuation seen in Inuvo's stock prices. These clusters have average values around -0.22, with peaks occurring at about 7th-9th month of each year. Based on these trends, we can predict the next significant upward movement after April 2021 will be a time frame between May and August. Note that these predictions are based solely on the findings from ExtractPoints() data structure, it is possible to run further operations with ClusterAnalysis or reference Indicator to confirm specific instances.
Phase Five Response Details:
- Model: tinyllama
- Tokens Used: 1289
- Finish Reason: stop
? Phase Five Progress: 100% - Success
? SUCCESS Phase Five TinyLlama interaction complete

=== Final Analysis Results ===

Vector Magnitudes:
High Cluster: 0.9204
Low Cluster: 0.8966

AI Analysis Results:
High Analysis:
Yes, the patterns observed in Inuvo's stock prices over the past few years demonstrate a pattern of upward fluctuation. The High values are seen in consecutive trends, with high-highs occurring roughly every three months. This indicates that a price reversal is likely to occur towards the end of the current trend or beginning of the next one.

The clusters identified by the given data set also show an upward movement toward the magnitude of 0.9203624493023799 and Extrapolate projection analysi, which may indicate the presence of a directional tranend or a trending market. The highest High seen is dated on July 14, 2021, when the price reached a maximum of $53.66 before the index decreased by 0.97%. This might be an indicator for the start of the next downward trend.

In conclusion, Inuvo's stock prices show a pattern of upward fluctuation with high-highs occurring in the intervals between regular price changes. The clusters found by clustering the data set reveal that a reversal is likely to occur in the 1st of next month or in the beginning of the next one, indicating a trending market or formation of a new trend.

Low Analysis:
Sure! The pattern of downward fluctuation observed in Inuvo's stock prices is indeed also present in the given list. The Cluster Analysis found that the dataset displays directional magnitudes, meaning that some elements have positive values and some negative. In order to confirm this analysis further, it is possible to retrieve specific instances in the List or Map data structure and run operations like Extrapolation with a reference indicator.

Here are some examples:
1. To get the first instance with upward fluctuation in dateTime, we can use ExtractPoint() method with reference indicator set to true:
```csharp
// using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExtractionUsingClusterAnalysis
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string inputPath = @"C:\path\to\inputfolder";
            var fileNames = Directory.EnumerateFiles(inputPath).Select((x, i) => $"{i:09}.csv");

            var data = JsonSerializer.Deserialize<List<System.Collections.Generic.List<string>>>(await File.ReadAllTextAsync(fileNames[0])); // Read CSV files at first

            foreach (var dataItem in data)
            {
                Console.WriteLine($"{dataItem.Count} - {dataItem[dataItem.Count-1].Split(',')[0]}: ");
                var cluster = data.ClusterLocations().OfType<Double>().FirstOrDefault();
                Console.WriteLine(cluster); // Extract cluster location for next 2 elements to check if it is upward fluctuation
                Console.ReadLine(); // Wait until user press Enter key
            }
        }
    }
}
```

In this case, we use the ClusterLocations() method of the first column's value dataset (data[0] being the first element) and ExtractPoint(clusterLocation, referenceIndicator: true).

2. To get the last instance with downward fluctuation in dateTime, here's an example:
```csharp
var data = JsonSerializer.Deserialize<List<System.Collections.Generic.List<string>>>(await File.ReadAllTextAsync(fileNames[0])); // Read CSV files at first
foreach (var dataItem in data)
{
    Console.WriteLine($"{dataItem.Count} - {dataItem[dataItem.Count-1].Split(',')[0]}: "); // Get the 2nd element's cluster location
    var cluster = data.ClusterLocations().OfType<Double>().Last(); // Extract last element's cluster location according to previous data
    Console.WriteLine(cluster); // Check if it is downward fluctuation from last day in same format as above, then exit the while loop and go on with the next data
}
```

Final Prediction:
Based on the analysis from Inuvo's stock prices, HIGH ANALYSE SHOWS THAT The pattern of downward fluctuation observed in Inuvo's stock prices has been confirmed with high-highs occurring roughly every three months. This indicates that a price reversal is likely to occur towards the end of the current trend or beginning of the next one.

In summary, the clusters identified by ExtractPoints() (in the given data set) reveal that there are instances of downward fluctuation seen in Inuvo's stock prices. These clusters have average values around -0.22, with peaks occurring at about 7th-9th month of each year. Based on these trends, we can predict the next significant upward movement after April 2021 will be a time frame between May and August. Note that these predictions are based solely on the findings from ExtractPoints() data structure, it is possible to run further operations with ClusterAnalysis or reference Indicator to confirm specific instances.

Price Movement Visualization:
  0.5500 |              H
  0.5474 |
  0.5447 |
  0.5421 |H
  0.5395 |H
  0.5368 |            H
  0.5342 |  H       H H
  0.5316 | LH   H   H             H
  0.5289 | L  H H H               H H
  0.5263 |    H   H                 H H
  0.5237 |           L L  H H         H
  0.5211 |     L L L L L  H H   H       H
  0.5184 |     L L L            H       H H H H H
  0.5158 |                         L L L  H H H H
  0.5132 |   L           L         L L L
  0.5105 |   L           L    H
  0.5079 |                    H  L       L L L L L
  0.5053 |                 L             L L L L L
  0.5026 |                 L   L
  0.5000 |                   L L
         |----------------------------------------
         |+----+----+----+----+----+----+----+----
Time     |12:3013:3015:0009:3011:0012:0013:3014:30

=== Statistical Summary ===

High Values:
  Maximum: $0.5500
  Minimum: $0.5086
  Average: $0.5262

Low Values:
  Maximum: $0.5300
  Minimum: $0.5000
  Average: $0.5126

Price Movement Metrics:
  Total Range: $0.0500
  Volatility: 10.00%

Time Analysis:
  Time Range: 27.0 hours
  Top 3 High Times: 09:30 ($0.5500), 12:30 ($0.5415), 15:30 ($0.5360)
Phase Five execution time: 122.321 seconds

=== Final Analysis Results ===

Vector Magnitudes:
High Cluster: 0.9204
Low Cluster: 0.8966

AI Analysis Results:
High Analysis:
Yes, the patterns observed in Inuvo's stock prices over the past few years demonstrate a pattern of upward fluctuation. The High values are seen in consecutive trends, with high-highs occurring roughly every three months. This indicates that a price reversal is likely to occur towards the end of the current trend or beginning of the next one.

The clusters identified by the given data set also show an upward movement toward the magnitude of 0.9203624493023799 and Extrapolate projection analysi, which may indicate the presence of a directional tranend or a trending market. The highest High seen is dated on July 14, 2021, when the price reached a maximum of $53.66 before the index decreased by 0.97%. This might be an indicator for the start of the next downward trend.

In conclusion, Inuvo's stock prices show a pattern of upward fluctuation with high-highs occurring in the intervals between regular price changes. The clusters found by clustering the data set reveal that a reversal is likely to occur in the 1st of next month or in the beginning of the next one, indicating a trending market or formation of a new trend.

Low Analysis:
Sure! The pattern of downward fluctuation observed in Inuvo's stock prices is indeed also present in the given list. The Cluster Analysis found that the dataset displays directional magnitudes, meaning that some elements have positive values and some negative. In order to confirm this analysis further, it is possible to retrieve specific instances in the List or Map data structure and run operations like Extrapolation with a reference indicator.

Here are some examples:
1. To get the first instance with upward fluctuation in dateTime, we can use ExtractPoint() method with reference indicator set to true:
```csharp
// using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExtractionUsingClusterAnalysis
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string inputPath = @"C:\path\to\inputfolder";
            var fileNames = Directory.EnumerateFiles(inputPath).Select((x, i) => $"{i:09}.csv");

            var data = JsonSerializer.Deserialize<List<System.Collections.Generic.List<string>>>(await File.ReadAllTextAsync(fileNames[0])); // Read CSV files at first

            foreach (var dataItem in data)
            {
                Console.WriteLine($"{dataItem.Count} - {dataItem[dataItem.Count-1].Split(',')[0]}: ");
                var cluster = data.ClusterLocations().OfType<Double>().FirstOrDefault();
                Console.WriteLine(cluster); // Extract cluster location for next 2 elements to check if it is upward fluctuation
                Console.ReadLine(); // Wait until user press Enter key
            }
        }
    }
}
```

In this case, we use the ClusterLocations() method of the first column's value dataset (data[0] being the first element) and ExtractPoint(clusterLocation, referenceIndicator: true).

2. To get the last instance with downward fluctuation in dateTime, here's an example:
```csharp
var data = JsonSerializer.Deserialize<List<System.Collections.Generic.List<string>>>(await File.ReadAllTextAsync(fileNames[0])); // Read CSV files at first
foreach (var dataItem in data)
{
    Console.WriteLine($"{dataItem.Count} - {dataItem[dataItem.Count-1].Split(',')[0]}: "); // Get the 2nd element's cluster location
    var cluster = data.ClusterLocations().OfType<Double>().Last(); // Extract last element's cluster location according to previous data
    Console.WriteLine(cluster); // Check if it is downward fluctuation from last day in same format as above, then exit the while loop and go on with the next data
}
```

Final Prediction:
Based on the analysis from Inuvo's stock prices, HIGH ANALYSE SHOWS THAT The pattern of downward fluctuation observed in Inuvo's stock prices has been confirmed with high-highs occurring roughly every three months. This indicates that a price reversal is likely to occur towards the end of the current trend or beginning of the next one.

In summary, the clusters identified by ExtractPoints() (in the given data set) reveal that there are instances of downward fluctuation seen in Inuvo's stock prices. These clusters have average values around -0.22, with peaks occurring at about 7th-9th month of each year. Based on these trends, we can predict the next significant upward movement after April 2021 will be a time frame between May and August. Note that these predictions are based solely on the findings from ExtractPoints() data structure, it is possible to run further operations with ClusterAnalysis or reference Indicator to confirm specific instances.

Price Movement Visualization:
  0.5500 |               H
  0.5474 |
  0.5447 |
  0.5421 |H
  0.5395 |H
  0.5368 |            H
  0.5342 |   H       HH
  0.5316 | L H   H   H            H
  0.5289 | L  H  H H              H H
  0.5263 |    H    H                H  H
  0.5237 |          L  L  H H          H
  0.5211 |     LL L L  L  H H   H        H
  0.5184 |     LL L             H        HH H H  H
  0.5158 |                         L LL   H H H  H
  0.5132 |  L           L          L LL
  0.5105 |  L           L     H
  0.5079 |                    H  L      L  L L LL
  0.5053 |                 L            L  L L LL
  0.5026 |                 L   L
  0.5000 |                   L L
         |----------------------------------------
         |+----+----+----+----+----+----+----+----
Time     |12:3013:3015:0009:3011:0012:0013:3014:30

=== Statistical Summary ===

High Values:
  Maximum: $0.5500
  Minimum: $0.5086
  Average: $0.5262

Low Values:
  Maximum: $0.5300
  Minimum: $0.5000
  Average: $0.5126

Price Movement Metrics:
  Total Range: $0.0500
  Volatility: 10.00%

Time Analysis:
  Time Range: 27.0 hours
  Top 3 High Times: 09:30 ($0.5500), 12:30 ($0.5415), 15:30 ($0.5360)

Total execution time: 276.225 seconds

E:\Development_Sandbox\Projects\AgenticExample\bin\Debug\net8.0\AgenticExample.exe (process 17420) exited with code 0 (0x0).
To automatically close the console when debugging stops, enable Tools->Options->Debugging->Automatically close the console when debugging stops.
Press any key to close this window . . .
```



 **Start the development server:**  
   ```bash
   npm run dev
   ```

---

## 🚀 **Features**  
 ✨ Feature 1: Brief description.  
 🛠️  Feature 2: Brief description.  
 🛠️ Feature 3: Brief description.  

---

## 🗂 **Folder Structure**  
```plaintext
📆 project-name
├── src
│   ├── components
│   ├── utils
│   └── assets
├── public
├── README.md
└── package.json
```

---

## 🔄 **Sequential Steps**  
Follow these steps to get started with the project:

1. **Clone the repository:**  
   ```bash
   git clone https://github.com/username/project-name.git
   cd project-name
   ```

2. **Install dependencies:**  
   ```bash
   npm install
   ```

3. **Start the development server:**  
   ```bash
   npm run dev
   ```

4. **Open in browser:**  
   Navigate to `http://localhost:3000` to view the project.

---

## 📃 **Code Snippets**  
### Example Component
```jsx
import React from 'react';

const Example = () => {
  return <div style={{ color: '#ADD8E6' }}>Hello, World!</div>;
};

export default Example;
```

### Example Utility Function
```javascript
export const add = (a, b) => a + b;
```

### Example API Call
```javascript
import axios from 'axios';

export const fetchData = async (url) => {
  try {
    const response = await axios.get(url);
    return response.data;
  } catch (error) {
    console.error('Error fetching data:', error);
    throw error;
  }
};
```

---

## 🔒 **Notes**  
- **Ensure Node.js is installed:** This project requires Node.js version 16 or higher.
- **Use environment variables:** Configure the `.env` file for sensitive data.
- **Linting:** Run `npm run lint` before committing code to maintain consistency.

---

## 🔼 **Sidebar**  
> **Quick Links**:  
> - [Documentation](#)  
> - [Issues](#)  
> - [Pull Requests](#)  
> - [Contributing](#)  

> **Important Reminders**:  
> - Commit messages should follow [Conventional Commits](https://www.conventionalcommits.org/).
> - Always test your code before pushing changes.
> - Keep dependencies updated regularly.

</div>