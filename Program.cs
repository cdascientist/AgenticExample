using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using AutoGen;
using Accord;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NumSharp;
using System.Linq;
using System.Text.Json.Serialization;

namespace AgenticExample
{
    public class ChatCompletionMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class ChatCompletionChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public ChatCompletionMessage Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }
    }

    public class ChatCompletionUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class ChatCompletionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("system_fingerprint")]
        public string SystemFingerprint { get; set; }

        [JsonPropertyName("choices")]
        public List<ChatCompletionChoice> Choices { get; set; }

        [JsonPropertyName("usage")]
        public ChatCompletionUsage Usage { get; set; }
    }

    //Step One - Implementation of Just In Time Memory Object for dynamic storage
    public class Jit_Memory_Object
    {
        private static readonly ExpandoObject _dynamicStorage = new ExpandoObject();
        private static readonly dynamic _dynamicObject = _dynamicStorage;
        private static RuntimeMethodHandle _jitMethodHandle;

        public static void AddProperty(string propertyName, object value)
        {
            var timer = Stopwatch.StartNew();
            var dictionary = (IDictionary<string, object>)_dynamicStorage;
            dictionary[propertyName] = value;
            timer.Stop();
            Debug.WriteLine($"AddProperty execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }

        public static object GetProperty(string propertyName)
        {
            var timer = Stopwatch.StartNew();
            var dictionary = (IDictionary<string, object>)_dynamicStorage;
            var result = dictionary.TryGetValue(propertyName, out var value) ? value : null;
            timer.Stop();
            Debug.WriteLine($"GetProperty execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
            return result;
        }

        public static dynamic DynamicObject => _dynamicObject;

        public static void SetJitMethodHandle(RuntimeMethodHandle handle)
        {
            var timer = Stopwatch.StartNew();
            _jitMethodHandle = handle;
            timer.Stop();
            Debug.WriteLine($"SetJitMethodHandle execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }

        public static RuntimeMethodHandle GetJitMethodHandle()
        {
            var timer = Stopwatch.StartNew();
            var result = _jitMethodHandle;
            timer.Stop();
            Debug.WriteLine($"GetJitMethodHandle execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
            return result;
        }
    }

    //Step Two - Base program structure with API configurations
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var totalTimer = Stopwatch.StartNew();
            Debug.WriteLine("Hello");

            var factory = new PhaseFactory();
            var orchestrator = new PhaseOrchestrator(factory);

            await factory.CreatePhase(1).ExecuteAsync();
            await factory.CreatePhase(2).ExecuteAsync();

            Debug.WriteLine("Orchestrator starting parallel execution of Phase Three and Four");
            await orchestrator.ExecuteParallelPhasesAsync();

            // Get phase execution statuses
            var phaseThreeStatus = Jit_Memory_Object.GetProperty("PHASE_THREE_COMPLETE");
            var phaseFourStatus = Jit_Memory_Object.GetProperty("PHASE_FOUR_COMPLETE");

            if (phaseThreeStatus != null && phaseFourStatus != null &&
                (bool)phaseThreeStatus && (bool)phaseFourStatus)
            {
                Debug.WriteLine("Phase Three and Four completed successfully, starting Phase Five");
                await factory.CreatePhase(5).ExecuteAsync();
            }
            else
            {
                Debug.WriteLine("Error: Phase Three and/or Four did not complete successfully");
            }

            totalTimer.Stop();
            Debug.WriteLine($"Total execution time: {totalTimer.Elapsed.TotalSeconds:F3} seconds");
        }
    }

    //Step Three - Factory pattern implementation with explicit typing
    public class PhaseFactory
    {
        public IPhase CreatePhase(int phaseNumber)
        {
            var timer = Stopwatch.StartNew();
            IPhase result = phaseNumber switch
            {
                1 => new PhaseOne(),
                2 => new PhaseTwo(),
                3 => new PhaseThree(),
                4 => new PhaseFour(),
                5 => new PhaseFive(),
                _ => throw new ArgumentException("Invalid phase number")
            };
            timer.Stop();
            Debug.WriteLine($"Phase creation time for Phase {phaseNumber}: {timer.Elapsed.TotalSeconds:F3} seconds");
            return result;
        }
    }

    //Step Four - Orchestrator implementation
    public class PhaseOrchestrator
    {
        private readonly PhaseFactory _factory;

        public PhaseOrchestrator(PhaseFactory factory)
        {
            var timer = Stopwatch.StartNew();
            _factory = factory;
            timer.Stop();
            Debug.WriteLine($"Orchestrator initialization time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }

        public async Task ExecuteParallelPhasesAsync()
        {
            var timer = Stopwatch.StartNew();
            var phaseThree = _factory.CreatePhase(3).ExecuteAsync();
            var phaseFour = _factory.CreatePhase(4).ExecuteAsync();
            await Task.WhenAll(phaseThree, phaseFour);
            timer.Stop();
            Debug.WriteLine($"Parallel execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }
    }

    public interface IPhase
    {
        Task ExecuteAsync();
    }

    //Step Five - Phase implementations
    public class PhaseOne : IPhase
    {
        private const string API_KEY = "ac5809d04d834e37bb687ed193139097";
        private const string BASE_URL = "https://api.twelvedata.com/time_series";
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task ExecuteAsync()
        {
            var timer = Stopwatch.StartNew();
            Debug.WriteLine("Initializing Phase One - Data Retrieval");

            try
            {
                string url = $"{BASE_URL}?apikey={API_KEY}&interval=30min&symbol=INUV&format=JSON&outputsize=20";
                string response = await _httpClient.GetStringAsync(url);

                var data = JObject.Parse(response);
                var values = data["values"].ToArray();

                var inuvHigh = values.Select(v => new
                {
                    DateTime = v["datetime"].ToString(),
                    High = v["high"].Value<decimal>(),
                    Volume = v["volume"].Value<int>()
                }).ToArray();

                var inuvLow = values.Select(v => new
                {
                    DateTime = v["datetime"].ToString(),
                    Low = v["low"].Value<decimal>(),
                    Volume = v["volume"].Value<int>()
                }).ToArray();

                Jit_Memory_Object.AddProperty("INUV_High", inuvHigh);
                Jit_Memory_Object.AddProperty("INUV_Low", inuvLow);

                Debug.WriteLine("INUV High Array:");
                foreach (var item in inuvHigh)
                {
                    Debug.WriteLine($"DateTime: {item.DateTime}, High: {item.High}, Volume: {item.Volume}");
                }

                Debug.WriteLine("\nINUV Low Array:");
                foreach (var item in inuvLow)
                {
                    Debug.WriteLine($"DateTime: {item.DateTime}, Low: {item.Low}, Volume: {item.Volume}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Phase One: {ex.Message}");
            }

            timer.Stop();
            Debug.WriteLine($"Phase One execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }
    }

    public class PhaseTwo : IPhase
    {
        public async Task ExecuteAsync()
        {
            var timer = Stopwatch.StartNew();
            Debug.WriteLine("Initializing Phase Two - Machine Learning Analysis");

            try
            {
                // Process INUV_High data
                var highData = (dynamic[])Jit_Memory_Object.GetProperty("INUV_High");
                if (highData != null)
                {
                    // Prepare data for clustering
                    double[] highValues = highData.Select(x => (double)x.High).ToArray();
                    var kmeans = new KMeans(3);
                    var highClusters = kmeans.Learn(highValues.Select(x => new[] { x }).ToArray());

                    // Calculate cluster averages and organize data
                    var highClusterData = new List<object>();
                    for (int i = 0; i < 3; i++)
                    {
                        var clusterIndices = Enumerable.Range(0, highValues.Length)
                            .Where(idx => highClusters.Decide(new[] { highValues[idx] }) == i)
                            .ToList();

                        if (clusterIndices.Any())
                        {
                            var clusterAverage = clusterIndices.Average(idx => highValues[idx]);
                            var clusterInfo = new
                            {
                                ClusterAverage = clusterAverage,
                                Points = clusterIndices
                                    .Select(idx => new
                                    {
                                        OriginalDateTime = DateTime.Parse(highData[idx].DateTime),
                                        FormattedTime = DateTime.Parse(highData[idx].DateTime).ToString("hh:mm tt"),
                                        Volume = highData[idx].Volume
                                    })
                                    .OrderBy(x => x.OriginalDateTime)
                                    .ThenBy(x => x.Volume)
                                    .Select(x => new
                                    {
                                        DateTime = x.FormattedTime,
                                        Volume = x.Volume
                                    })
                                    .ToList()
                            };
                            highClusterData.Add(clusterInfo);
                        }
                    }

                    // Order clusters by their average values
                    highClusterData = highClusterData.OrderBy(x => ((dynamic)x).ClusterAverage).ToList();

                    Jit_Memory_Object.AddProperty("INUV_High_ML", highClusterData);
                    Debug.WriteLine("\nINUV High ML Clusters (Ordered by time and volume):");
                    foreach (dynamic cluster in highClusterData)
                    {
                        Debug.WriteLine($"\nCluster Average: {cluster.ClusterAverage:F4}");
                        foreach (var point in cluster.Points)
                        {
                            Debug.WriteLine($"Time: {point.DateTime}, Volume: {point.Volume}");
                        }
                    }
                }

                // Process INUV_Low data
                var lowData = (dynamic[])Jit_Memory_Object.GetProperty("INUV_Low");
                if (lowData != null)
                {
                    // Prepare data for clustering
                    double[] lowValues = lowData.Select(x => (double)x.Low).ToArray();
                    var kmeans = new KMeans(3);
                    var lowClusters = kmeans.Learn(lowValues.Select(x => new[] { x }).ToArray());

                    // Calculate cluster averages and organize data
                    var lowClusterData = new List<object>();
                    for (int i = 0; i < 3; i++)
                    {
                        var clusterIndices = Enumerable.Range(0, lowValues.Length)
                            .Where(idx => lowClusters.Decide(new[] { lowValues[idx] }) == i)
                            .ToList();

                        if (clusterIndices.Any())
                        {
                            var clusterAverage = clusterIndices.Average(idx => lowValues[idx]);
                            var clusterInfo = new
                            {
                                ClusterAverage = clusterAverage,
                                Points = clusterIndices
                                    .Select(idx => new
                                    {
                                        OriginalDateTime = DateTime.Parse(lowData[idx].DateTime),
                                        FormattedTime = DateTime.Parse(lowData[idx].DateTime).ToString("hh:mm tt"),
                                        Volume = lowData[idx].Volume
                                    })
                                    .OrderBy(x => x.OriginalDateTime)
                                    .ThenBy(x => x.Volume)
                                    .Select(x => new
                                    {
                                        DateTime = x.FormattedTime,
                                        Volume = x.Volume
                                    })
                                    .ToList()
                            };
                            lowClusterData.Add(clusterInfo);
                        }
                    }

                    // Order clusters by their average values
                    lowClusterData = lowClusterData.OrderBy(x => ((dynamic)x).ClusterAverage).ToList();

                    Jit_Memory_Object.AddProperty("INUV_Low_ML", lowClusterData);
                    Debug.WriteLine("\nINUV Low ML Clusters (Ordered by time and volume):");
                    foreach (dynamic cluster in lowClusterData)
                    {
                        Debug.WriteLine($"\nCluster Average: {cluster.ClusterAverage:F4}");
                        foreach (var point in cluster.Points)
                        {
                            Debug.WriteLine($"Time: {point.DateTime}, Volume: {point.Volume}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Phase Two: {ex.Message}");
            }

            timer.Stop();
            Debug.WriteLine($"Phase Two execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }
    }

    public class PhaseThree : IPhase
    {
        private const string TINYLLAMA_URL = "https://b21b-34-73-230-37.ngrok-free.app";
        private readonly HttpClient _httpClient;
        private const int MAX_RETRIES = 3;
        private const int RETRY_DELAY_MS = 1000;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public PhaseThree()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task ExecuteAsync()
        {
            var timer = Stopwatch.StartNew();
            Debug.WriteLine("Initializing Phase Three - TinyLlama Communication");
            Debug.WriteLine("⏳ Phase Three Progress: 0% - Starting TinyLlama connection...");

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    Debug.WriteLine($"⏳ Phase Three Progress: {(attempt * 25)}% - Attempt {attempt} of {MAX_RETRIES}");

                    var payload = new
                    {
                        model = "tinyllama",
                        messages = new[]
                        {
                            new { role = "user", content = "How are you?" }
                        },
                        stream = false,
                        options = new
                        {
                            temperature = 0.7,
                            max_tokens = 100
                        }
                    };

                    var content = new StringContent(
                         JsonSerializer.Serialize(payload, _jsonOptions),
                         Encoding.UTF8,
                         "application/json");

                    Debug.WriteLine($"📡 Phase Three: Sending request to TinyLlama (Attempt {attempt})...");
                    var response = await _httpClient.PostAsync($"{TINYLLAMA_URL}/v1/chat/completions", content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"📥 Phase Three Raw Response: {responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                    }

                    var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent, _jsonOptions);

                    if (result?.Choices != null && result.Choices.Count > 0)
                    {
                        var message = result.Choices[0].Message;
                        Debug.WriteLine($"⏳ Phase Three Progress: 75% - Processing response...");
                        Debug.WriteLine($"Phase Three TinyLlama Response Content: {message.Content}");
                        Debug.WriteLine($"Phase Three Response Details:");
                        Debug.WriteLine($"- Model: {result.Model}");
                        Debug.WriteLine($"- Tokens Used: {result.Usage.TotalTokens}");
                        Debug.WriteLine($"- Finish Reason: {result.Choices[0].FinishReason}");

                        Jit_Memory_Object.AddProperty("PHASE_THREE_TINYLLAMA_RESPONSE", message.Content);
                        Jit_Memory_Object.AddProperty("PHASE_THREE_TINYLLAMA_STATUS", "CONNECTED");
                        Jit_Memory_Object.AddProperty("PHASE_THREE_TINYLLAMA_USAGE", result.Usage);
                        Jit_Memory_Object.AddProperty("PHASE_THREE_COMPLETE", true);
                        Debug.WriteLine("⏳ Phase Three Progress: 100% - Success");
                        Debug.WriteLine("✅ SUCCESS Phase Three TinyLlama interaction complete");
                        break;
                    }
                    else
                    {
                        throw new Exception("Invalid response format from TinyLlama");
                    }
                }
                catch (JsonException jex)
                {
                    Debug.WriteLine($"❌ Phase Three JSON Error (Attempt {attempt}/{MAX_RETRIES}): {jex.Message}");
                    Debug.WriteLine($"Path: {jex.Path}, LineNumber: {jex.LineNumber}, BytePositionInLine: {jex.BytePositionInLine}");

                    if (attempt == MAX_RETRIES)
                    {
                        Jit_Memory_Object.AddProperty("PHASE_THREE_TINYLLAMA_STATUS", "FAILED");
                        Jit_Memory_Object.AddProperty("PHASE_THREE_COMPLETE", false);
                        Debug.WriteLine("❌ ERROR: Phase Three - All retry attempts exhausted");
                    }
                    else
                    {
                        Debug.WriteLine($"⏳ Phase Three: Retrying in {RETRY_DELAY_MS}ms...");
                        await Task.Delay(RETRY_DELAY_MS * attempt); // Exponential backoff
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Phase Three Error (Attempt {attempt}/{MAX_RETRIES}): {ex.Message}");

                    if (attempt == MAX_RETRIES)
                    {
                        Jit_Memory_Object.AddProperty("PHASE_THREE_TINYLLAMA_STATUS", "FAILED");
                        Jit_Memory_Object.AddProperty("PHASE_THREE_COMPLETE", false);
                        Debug.WriteLine("❌ ERROR: Phase Three - All retry attempts exhausted");
                    }
                    else
                    {
                        Debug.WriteLine($"⏳ Phase Three: Retrying in {RETRY_DELAY_MS}ms...");
                        await Task.Delay(RETRY_DELAY_MS * attempt); // Exponential backoff
                    }
                }
            }

            timer.Stop();
            Debug.WriteLine($"Phase Three execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }
    }

    public class PhaseFour : IPhase
    {
        private const string TINYLLAMA_URL = "https://b21b-34-73-230-37.ngrok-free.app";
        private readonly HttpClient _httpClient;
        private const int MAX_RETRIES = 3;
        private const int RETRY_DELAY_MS = 1000;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public PhaseFour()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task ExecuteAsync()
        {
            var timer = Stopwatch.StartNew();
            Debug.WriteLine("Initializing Phase Four - TinyLlama Communication");
            Debug.WriteLine("⏳ Phase Four Progress: 0% - Starting TinyLlama connection...");

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    Debug.WriteLine($"⏳ Phase Four Progress: {(attempt * 25)}% - Attempt {attempt} of {MAX_RETRIES}");

                    var payload = new
                    {
                        model = "tinyllama",
                        messages = new[]
                        {
                            new { role = "user", content = "What time is it?" }
                        },
                        stream = false,
                        options = new
                        {
                            temperature = 0.7,
                            max_tokens = 100
                        }
                    };

                    var content = new StringContent(
                         JsonSerializer.Serialize(payload, _jsonOptions),
                         Encoding.UTF8,
                         "application/json");

                    Debug.WriteLine($"📡 Phase Four: Sending request to TinyLlama (Attempt {attempt})...");
                    var response = await _httpClient.PostAsync($"{TINYLLAMA_URL}/v1/chat/completions", content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"📥 Phase Four Raw Response: {responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                    }

                    var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent, _jsonOptions);

                    if (result?.Choices != null && result.Choices.Count > 0)
                    {
                        var message = result.Choices[0].Message;
                        Debug.WriteLine($"⏳ Phase Four Progress: 75% - Processing response...");
                        Debug.WriteLine($"Phase Four TinyLlama Response Content: {message.Content}");
                        Debug.WriteLine($"Phase Four Response Details:");
                        Debug.WriteLine($"- Model: {result.Model}");
                        Debug.WriteLine($"- Tokens Used: {result.Usage.TotalTokens}");
                        Debug.WriteLine($"- Finish Reason: {result.Choices[0].FinishReason}");

                        Jit_Memory_Object.AddProperty("PHASE_FOUR_TINYLLAMA_RESPONSE", message.Content);
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_TINYLLAMA_STATUS", "CONNECTED");
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_TINYLLAMA_USAGE", result.Usage);
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_COMPLETE", true);
                        Debug.WriteLine("⏳ Phase Four Progress: 100% - Success");
                        Debug.WriteLine("✅ SUCCESS Phase Four TinyLlama interaction complete");
                        break;
                    }
                    else
                    {
                        throw new Exception("Invalid response format from TinyLlama");
                    }
                }
                catch (JsonException jex)
                {
                    Debug.WriteLine($"❌ Phase Four JSON Error (Attempt {attempt}/{MAX_RETRIES}): {jex.Message}");
                    Debug.WriteLine($"Path: {jex.Path}, LineNumber: {jex.LineNumber}, BytePositionInLine: {jex.BytePositionInLine}");

                    if (attempt == MAX_RETRIES)
                    {
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_TINYLLAMA_STATUS", "FAILED");
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_COMPLETE", false);
                        Debug.WriteLine("❌ ERROR: Phase Four - All retry attempts exhausted");
                    }
                    else
                    {
                        Debug.WriteLine($"⏳ Phase Four: Retrying in {RETRY_DELAY_MS}ms...");
                        await Task.Delay(RETRY_DELAY_MS * attempt); // Exponential backoff
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Phase Four Error (Attempt {attempt}/{MAX_RETRIES}): {ex.Message}");

                    if (attempt == MAX_RETRIES)
                    {
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_TINYLLAMA_STATUS", "FAILED");
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_COMPLETE", false);
                        Debug.WriteLine("❌ ERROR: Phase Four - All retry attempts exhausted");
                    }
                    else
                    {
                        Debug.WriteLine($"⏳ Phase Four: Retrying in {RETRY_DELAY_MS}ms...");
                        await Task.Delay(RETRY_DELAY_MS * attempt); // Exponential backoff
                    }
                }
            }

            timer.Stop();
            Debug.WriteLine($"Phase Four execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }
    }

    public class PhaseFive : IPhase
    {
        public async Task ExecuteAsync()
        {
            var timer = Stopwatch.StartNew();
            Debug.WriteLine("Initializing Phase Five");

            try
            {
                // Check if both Phase Three and Four completed successfully
                var phaseThreeStatus = Jit_Memory_Object.GetProperty("PHASE_THREE_COMPLETE");
                var phaseFourStatus = Jit_Memory_Object.GetProperty("PHASE_FOUR_COMPLETE");

                if (phaseThreeStatus != null && phaseFourStatus != null &&
                    (bool)phaseThreeStatus && (bool)phaseFourStatus)
                {
                    await Task.Delay(300); // Simulated work
                    Debug.WriteLine("✅ SUCCESS Completing Phase Five");
                    Jit_Memory_Object.AddProperty("PHASE_FIVE_COMPLETE", true);
                }
                else
                {
                    Debug.WriteLine("❌ ERROR: Phase Five cancelled - Dependencies not met");
                    Jit_Memory_Object.AddProperty("PHASE_FIVE_COMPLETE", false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Phase Five: {ex.Message}");
                Jit_Memory_Object.AddProperty("PHASE_FIVE_COMPLETE", false);
            }

            timer.Stop();
            Debug.WriteLine($"Phase Five execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }
    }
}