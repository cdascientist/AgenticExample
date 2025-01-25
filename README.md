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
      
### Rendered Example

Here's how the above snippet will appear when rendered:

```csharp
// This is a simple C# example
using System;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
```



   ```bash
   npm install
   ```

### Install Code
```javascript
export const add = (a, b) => a + b;
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