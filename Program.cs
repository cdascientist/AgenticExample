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
    public static class AppConfig
    {
        private static string _ngrokTunnelAddress = "019e-34-74-45-28.ngrok-free.app";
        private static string _protocol = "https://";

        public static string NgrokTunnelAddress
        {
            get { return _ngrokTunnelAddress; }
            set
            {
                _ngrokTunnelAddress = value;
                UpdateTinyLlamaUrl();
            }
        }

        public static string Protocol
        {
            get { return _protocol; }
            set
            {
                _protocol = value;
                UpdateTinyLlamaUrl();
            }
        }

        private static void UpdateTinyLlamaUrl()
        {
            TINYLLAMA_URL = $"{_protocol}{_ngrokTunnelAddress}";
        }

        public static string TINYLLAMA_URL { get; private set; }

        static AppConfig()
        {
            UpdateTinyLlamaUrl();
        }
    }

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

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var totalTimer = Stopwatch.StartNew();
            Console.WriteLine("=== INUV Stock Analysis System Starting ===");

            if (args.Length > 0)
            {
                AppConfig.NgrokTunnelAddress = args[0];
                Console.WriteLine($"Using provided Ngrok tunnel address: {AppConfig.NgrokTunnelAddress}");
            }

            var factory = new PhaseFactory();
            var orchestrator = new PhaseOrchestrator(factory);

            try
            {
                Console.WriteLine("\nExecuting Phase One - Data Retrieval");
                await factory.CreatePhase(1).ExecuteAsync();

                Console.WriteLine("\nExecuting Phase Two - Machine Learning Analysis");
                await factory.CreatePhase(2).ExecuteAsync();

                Console.WriteLine("\nStarting parallel execution of Phase Three and Four");
                await orchestrator.ExecuteParallelPhasesAsync();

                var phaseThreeStatus = Jit_Memory_Object.GetProperty("PHASE_THREE_COMPLETE");
                var phaseFourStatus = Jit_Memory_Object.GetProperty("PHASE_FOUR_COMPLETE");

                if (phaseThreeStatus != null && phaseFourStatus != null &&
                    (bool)phaseThreeStatus && (bool)phaseFourStatus)
                {
                    Console.WriteLine("\nExecuting Phase Five - Final Analysis");
                    await factory.CreatePhase(5).ExecuteAsync();

                    // Display final results
                    DisplayFinalResults();
                }
                else
                {
                    Console.WriteLine("Error: Phase Three and/or Four did not complete successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in main execution: {ex.Message}");
                Debug.WriteLine($"Detailed error: {ex}");
            }

            totalTimer.Stop();
            Console.WriteLine($"\nTotal execution time: {totalTimer.Elapsed.TotalSeconds:F3} seconds");
        }

        private static void DisplayFinalResults()
        {
            Console.WriteLine("\n=== Final Analysis Results ===");

            var highMagnitude = Jit_Memory_Object.GetProperty("HIGH_CLUSTER_VECTOR_MAGNITUDE");
            var lowMagnitude = Jit_Memory_Object.GetProperty("LOW_CLUSTER_VECTOR_MAGNITUDE");

            Console.WriteLine("\nVector Magnitudes:");
            Console.WriteLine($"High Cluster: {highMagnitude:F4}");
            Console.WriteLine($"Low Cluster: {lowMagnitude:F4}");

            var phaseThreeResponse = Jit_Memory_Object.GetProperty("PHASE_THREE_TINYLLAMA_RESPONSE");
            var phaseFourResponse = Jit_Memory_Object.GetProperty("PHASE_FOUR_TINYLLAMA_RESPONSE");
            var phaseFiveResponse = Jit_Memory_Object.GetProperty("PHASE_FIVE_TINYLLAMA_RESPONSE");

            Console.WriteLine("\nAI Analysis Results:");
            Console.WriteLine("High Analysis:");
            Console.WriteLine(phaseThreeResponse);
            Console.WriteLine("\nLow Analysis:");
            Console.WriteLine(phaseFourResponse);
            Console.WriteLine("\nFinal Prediction:");
            Console.WriteLine(phaseFiveResponse);

            var highData = Jit_Memory_Object.GetProperty("INUV_High") as dynamic[];
            var lowData = Jit_Memory_Object.GetProperty("INUV_Low") as dynamic[];

            if (highData != null && lowData != null)
            {
                Console.WriteLine("\nPrice Movement Visualization:");
                RenderPriceGraph(highData, lowData);
            }
        }

        private static void RenderPriceGraph(dynamic[] highData, dynamic[] lowData)
        {
            const int GRAPH_HEIGHT = 20;
            var allPoints = new List<(DateTime time, decimal price, string type)>();

            // Convert high data points
            foreach (var point in highData)
            {
                allPoints.Add((
                    DateTime.Parse(point.DateTime.ToString()),
                    Convert.ToDecimal(point.High),
                    "H"
                ));
            }

            // Convert low data points
            foreach (var point in lowData)
            {
                allPoints.Add((
                    DateTime.Parse(point.DateTime.ToString()),
                    Convert.ToDecimal(point.Low),
                    "L"
                ));
            }

            // Sort by time
            allPoints.Sort((a, b) => a.time.CompareTo(b.time));

            // Calculate ranges for the graph
            decimal maxValue = allPoints.Max(p => p.price);
            decimal minValue = allPoints.Min(p => p.price);
            decimal valueRange = maxValue - minValue;

            // Print the graph
            for (int i = GRAPH_HEIGHT - 1; i >= 0; i--)
            {
                decimal rowValue = minValue + (valueRange * i / (GRAPH_HEIGHT - 1));
                Console.Write($"{rowValue,8:F4} |");

                foreach (var point in allPoints)
                {
                    decimal normalizedDiff = Math.Abs(point.price - rowValue);
                    decimal threshold = valueRange / GRAPH_HEIGHT;
                    if (normalizedDiff < threshold)
                    {
                        Console.Write(point.type);
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }
                Console.WriteLine();
            }

            // Print time axis
            Console.Write("         |");
            Console.WriteLine(new string('-', allPoints.Count));

            // Print time markers
            Console.Write("         |");
            for (int i = 0; i < allPoints.Count; i++)
            {
                Console.Write(i % 5 == 0 ? "+" : "-");
            }
            Console.WriteLine();

            // Print time labels
            Console.Write("Time     |");
            var timeLabels = allPoints
                .Where((p, i) => i % 5 == 0)
                .Select(p => p.time.ToString("HH:mm").PadRight(5));
            Console.WriteLine(string.Join("", timeLabels));

            // Print statistical summary
            PrintStatisticalSummary(allPoints);
        }

        private static void PrintStatisticalSummary(List<(DateTime time, decimal price, string type)> allPoints)
        {
            var highPoints = allPoints.Where(p => p.type == "H").ToList();
            var lowPoints = allPoints.Where(p => p.type == "L").ToList();

            Console.WriteLine("\n=== Statistical Summary ===");

            Console.WriteLine($"\nHigh Values:");
            Console.WriteLine($"  Maximum: ${highPoints.Max(p => p.price):F4}");
            Console.WriteLine($"  Minimum: ${highPoints.Min(p => p.price):F4}");
            Console.WriteLine($"  Average: ${highPoints.Average(p => p.price):F4}");

            Console.WriteLine($"\nLow Values:");
            Console.WriteLine($"  Maximum: ${lowPoints.Max(p => p.price):F4}");
            Console.WriteLine($"  Minimum: ${lowPoints.Min(p => p.price):F4}");
            Console.WriteLine($"  Average: ${lowPoints.Average(p => p.price):F4}");

            // Calculate price movement metrics
            var priceRange = highPoints.Max(p => p.price) - lowPoints.Min(p => p.price);
            var volatility = priceRange / lowPoints.Min(p => p.price) * 100;

            Console.WriteLine($"\nPrice Movement Metrics:");
            Console.WriteLine($"  Total Range: ${priceRange:F4}");
            Console.WriteLine($"  Volatility: {volatility:F2}%");

            // Time-based analysis
            var timeRange = allPoints.Max(p => p.time) - allPoints.Min(p => p.time);
            var highTimePoints = highPoints
                .OrderByDescending(p => p.price)
                .Take(3)
                .Select(p => $"{p.time:HH:mm} (${p.price:F4})");

            Console.WriteLine($"\nTime Analysis:");
            Console.WriteLine($"  Time Range: {timeRange.TotalHours:F1} hours");
            Console.WriteLine($"  Top 3 High Times: {string.Join(", ", highTimePoints)}");
        }
    }

    public interface IPhase
    {
        Task ExecuteAsync();
    }

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

    public class PhaseOne : IPhase
    {
        private const string API_KEY = "ac5809d04d834e37bb687ed193139097";
        private const string BASE_URL = "https://api.twelvedata.com/time_series";
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task ExecuteAsync()
        {
            var timer = Stopwatch.StartNew();
            Console.WriteLine("Initializing Phase One - Data Retrieval");

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

                Console.WriteLine("INUV High Array:");
                foreach (var item in inuvHigh)
                {
                    Console.WriteLine($"DateTime: {item.DateTime}, High: {item.High}, Volume: {item.Volume}");
                }

                Console.WriteLine("\nINUV Low Array:");
                foreach (var item in inuvLow)
                {
                    Console.WriteLine($"DateTime: {item.DateTime}, Low: {item.Low}, Volume: {item.Volume}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Phase One: {ex.Message}");
            }

            timer.Stop();
            Console.WriteLine($"Phase One execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }
    }

    public class PhaseTwo : IPhase
    {
        public async Task ExecuteAsync()
        {
            var timer = Stopwatch.StartNew();
            Console.WriteLine("Initializing Phase Two - Machine Learning Analysis");

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

                    // Calculate vector magnitude for high cluster averages
                    var highClusterAverages = highClusterData.Select(x => ((dynamic)x).ClusterAverage).Cast<double>().ToList();
                    while (highClusterAverages.Count < 3) highClusterAverages.Add(0); // Ensure 3D coordinates

                    double highVectorMagnitude = Math.Sqrt(
                        Math.Pow(highClusterAverages[0], 2) +
                        Math.Pow(highClusterAverages[1], 2) +
                        Math.Pow(highClusterAverages[2], 2)
                    );

                    Jit_Memory_Object.AddProperty("HIGH_CLUSTER_VECTOR_MAGNITUDE", highVectorMagnitude);
                    Console.WriteLine($"High Cluster Vector Magnitude: {highVectorMagnitude:F4}");

                    Jit_Memory_Object.AddProperty("INUV_High_ML", highClusterData);
                    Console.WriteLine("\nINUV High ML Clusters (Ordered by time and volume):");
                    foreach (dynamic cluster in highClusterData)
                    {
                        Console.WriteLine($"\nCluster Average: {cluster.ClusterAverage:F4}");
                        foreach (var point in cluster.Points)
                        {
                            Console.WriteLine($"Time: {point.DateTime}, Volume: {point.Volume}");
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

                    // Calculate vector magnitude for low cluster averages
                    var lowClusterAverages = lowClusterData.Select(x => ((dynamic)x).ClusterAverage).Cast<double>().ToList();
                    while (lowClusterAverages.Count < 3) lowClusterAverages.Add(0); // Ensure 3D coordinates

                    double lowVectorMagnitude = Math.Sqrt(
                        Math.Pow(lowClusterAverages[0], 2) +
                        Math.Pow(lowClusterAverages[1], 2) +
                        Math.Pow(lowClusterAverages[2], 2)
                    );

                    Jit_Memory_Object.AddProperty("LOW_CLUSTER_VECTOR_MAGNITUDE", lowVectorMagnitude);
                    Console.WriteLine($"Low Cluster Vector Magnitude: {lowVectorMagnitude:F4}");

                    Jit_Memory_Object.AddProperty("INUV_Low_ML", lowClusterData);
                    Console.WriteLine("\nINUV Low ML Clusters (Ordered by time and volume):");
                    foreach (dynamic cluster in lowClusterData)
                    {
                        Console.WriteLine($"\nCluster Average: {cluster.ClusterAverage:F4}");
                        foreach (var point in cluster.Points)
                        {
                            Console.WriteLine($"Time: {point.DateTime}, Volume: {point.Volume}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Phase Two: {ex.Message}");
            }

            timer.Stop();
            Console.WriteLine($"Phase Two execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }
    }

    public class PhaseThree : IPhase
    {
        private string _tinyLlamaUrl;
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
            _tinyLlamaUrl = AppConfig.TINYLLAMA_URL;
        }

        public async Task ExecuteAsync()
        {
            var timer = Stopwatch.StartNew();
            Console.WriteLine("Initializing Phase Three - TinyLlama Communication");

            // Retrieve values from Jit_Memory_Object
            var inuvHigh = Jit_Memory_Object.GetProperty("INUV_High") as dynamic[];
            var inuvHighML = Jit_Memory_Object.GetProperty("INUV_High_ML") as List<object>;
            var highVectorMagnitude = Jit_Memory_Object.GetProperty("HIGH_CLUSTER_VECTOR_MAGNITUDE");

            // Display retrieved values
            Console.WriteLine("\n=== Retrieved Values in Phase Three ===");
            Console.WriteLine("\nINUV High Values:");
            if (inuvHigh != null)
            {
                foreach (var item in inuvHigh)
                {
                    Console.WriteLine($"DateTime: {item.DateTime}, High: {item.High}, Volume: {item.Volume}");
                }
            }

            Console.WriteLine("\nINUV High ML Clusters:");
            if (inuvHighML != null)
            {
                foreach (dynamic cluster in inuvHighML)
                {
                    Console.WriteLine($"\nCluster Average: {cluster.ClusterAverage:F4}");
                    foreach (var point in cluster.Points)
                    {
                        Console.WriteLine($"Time: {point.DateTime}, Volume: {point.Volume}");
                    }
                }
            }

            Console.WriteLine($"\nHigh Cluster Vector Magnitude: {highVectorMagnitude:F4}");
            Console.WriteLine("\n=== End of Retrieved Values ===\n");

            Console.WriteLine("⏳ Phase Three Progress: 0% - Starting TinyLlama connection...");

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    Console.WriteLine($"⏳ Phase Three Progress: {(attempt * 25)}% - Attempt {attempt} of {MAX_RETRIES}");

                    var payload = new
                    {
                        model = "tinyllama",
                        messages = new[]
                        {
                            new { role = "user", content = "Given the pattern of upward fluctuation found in  Inuvo's  stock prices in " + inuvHigh + " there is a reoccurrence of High values according to " + inuvHighML + ". According to the clusters found in " + inuvHighML + " there is a directional trend Magnitude found in  " + highVectorMagnitude + ", Extrapolate projection analysis , then reference indicators where at DateTime the highest High can be found" }
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

                    Console.WriteLine($"📡 Phase Three: Sending request to TinyLlama (Attempt {attempt})...");
                    var response = await _httpClient.PostAsync($"{_tinyLlamaUrl}/v1/chat/completions", content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"📥 Phase Three Raw Response: {responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                    }

                    var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent, _jsonOptions);

                    if (result?.Choices != null && result.Choices.Count > 0)
                    {
                        var message = result.Choices[0].Message;
                        Console.WriteLine($"⏳ Phase Three Progress: 75% - Processing response...");
                        Console.WriteLine($"Phase Three TinyLlama Response Content: {message.Content}");
                        Console.WriteLine($"Phase Three Response Details:");
                        Console.WriteLine($"- Model: {result.Model}");
                        Console.WriteLine($"- Tokens Used: {result.Usage.TotalTokens}");
                        Console.WriteLine($"- Finish Reason: {result.Choices[0].FinishReason}");

                        Jit_Memory_Object.AddProperty("PhaseThreeHighAnalysis", message.Content);
                        Jit_Memory_Object.AddProperty("PHASE_THREE_TINYLLAMA_RESPONSE", message.Content);
                        Jit_Memory_Object.AddProperty("PHASE_THREE_TINYLLAMA_STATUS", "CONNECTED");
                        Jit_Memory_Object.AddProperty("PHASE_THREE_TINYLLAMA_USAGE", result.Usage);
                        Jit_Memory_Object.AddProperty("PHASE_THREE_COMPLETE", true);
                        Console.WriteLine("⏳ Phase Three Progress: 100% - Success");
                        Console.WriteLine("✅ SUCCESS Phase Three TinyLlama interaction complete");
                        break;
                    }
                    else
                    {
                        throw new Exception("Invalid response format from TinyLlama");
                    }
                }
                catch (JsonException jex)
                {
                    Console.WriteLine($"❌ Phase Three JSON Error (Attempt {attempt}/{MAX_RETRIES}): {jex.Message}");
                    Console.WriteLine($"Path: {jex.Path}, LineNumber: {jex.LineNumber}, BytePositionInLine: {jex.BytePositionInLine}");

                    if (attempt == MAX_RETRIES)
                    {
                        Jit_Memory_Object.AddProperty("PHASE_THREE_TINYLLAMA_STATUS", "FAILED");
                        Jit_Memory_Object.AddProperty("PHASE_THREE_COMPLETE", false);
                        Console.WriteLine("❌ ERROR: Phase Three - All retry attempts exhausted");
                    }
                    else
                    {
                        Console.WriteLine($"⏳ Phase Three: Retrying in {RETRY_DELAY_MS}ms...");
                        await Task.Delay(RETRY_DELAY_MS * attempt);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Phase Three Error (Attempt {attempt}/{MAX_RETRIES}): {ex.Message}");

                    if (attempt == MAX_RETRIES)
                    {
                        Jit_Memory_Object.AddProperty("PHASE_THREE_TINYLLAMA_STATUS", "FAILED");
                        Jit_Memory_Object.AddProperty("PHASE_THREE_COMPLETE", false);
                        Console.WriteLine("❌ ERROR: Phase Three - All retry attempts exhausted");
                    }
                    else
                    {
                        Console.WriteLine($"⏳ Phase Three: Retrying in {RETRY_DELAY_MS}ms...");
                        await Task.Delay(RETRY_DELAY_MS * attempt);
                    }
                }
            }

            timer.Stop();
            Console.WriteLine($"Phase Three execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }
    }

    public class PhaseFour : IPhase
    {
        private string _tinyLlamaUrl;
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
            _tinyLlamaUrl = AppConfig.TINYLLAMA_URL;
        }

        public async Task ExecuteAsync()
        {
            var timer = Stopwatch.StartNew();
            Console.WriteLine("Initializing Phase Four - TinyLlama Communication");

            // Retrieve values from Jit_Memory_Object
            var inuvLow = Jit_Memory_Object.GetProperty("INUV_Low") as dynamic[];
            var inuvLowML = Jit_Memory_Object.GetProperty("INUV_Low_ML") as List<object>;
            var lowVectorMagnitude = Jit_Memory_Object.GetProperty("LOW_CLUSTER_VECTOR_MAGNITUDE");

            // Display retrieved values
            Console.WriteLine("\n=== Retrieved Values in Phase Four ===");
            Console.WriteLine("\nINUV Low Values:");
            if (inuvLow != null)
            {
                foreach (var item in inuvLow)
                {
                    Console.WriteLine($"DateTime: {item.DateTime}, Low: {item.Low}, Volume: {item.Volume}");
                }
            }

            Console.WriteLine("\nINUV Low ML Clusters:");
            if (inuvLowML != null)
            {
                foreach (dynamic cluster in inuvLowML)
                {
                    Console.WriteLine($"\nCluster Average: {cluster.ClusterAverage:F4}");
                    foreach (var point in cluster.Points)
                    {
                        Console.WriteLine($"Time: {point.DateTime}, Volume: {point.Volume}");
                    }
                }
            }

            Console.WriteLine($"\nLow Cluster Vector Magnitude: {lowVectorMagnitude:F4}");
            Console.WriteLine("\n=== End of Retrieved Values ===\n");

            Console.WriteLine("⏳ Phase Four Progress: 0% - Starting TinyLlama connection...");

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    Console.WriteLine($"⏳ Phase Four Progress: {(attempt * 25)}% - Attempt {attempt} of {MAX_RETRIES}");

                    var payload = new
                    {
                        model = "tinyllama",
                        messages = new[]
                        {
                            new { role = "user", content = "Given the pattern of downward fluctuation found in Inuvo's stock prices in " + inuvLow + " there is a reoccurrence of Low values according to " + inuvLowML + ". According to the clusters found in " + inuvLowML + " there is a directional Magnitude found in " + lowVectorMagnitude + ", Extrapolate projection analysis, then reference indicators where at DateTime the lowest Low can be found" }
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

                    Console.WriteLine($"📡 Phase Four: Sending request to TinyLlama (Attempt {attempt})...");
                    var response = await _httpClient.PostAsync($"{_tinyLlamaUrl}/v1/chat/completions", content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"📥 Phase Four Raw Response: {responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                    }

                    var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent, _jsonOptions);

                    if (result?.Choices != null && result.Choices.Count > 0)
                    {
                        var message = result.Choices[0].Message;
                        Console.WriteLine($"⏳ Phase Four Progress: 75% - Processing response...");
                        Console.WriteLine($"Phase Four TinyLlama Response Content: {message.Content}");
                        Console.WriteLine($"Phase Four Response Details:");
                        Console.WriteLine($"- Model: {result.Model}");
                        Console.WriteLine($"- Tokens Used: {result.Usage.TotalTokens}");
                        Console.WriteLine($"- Finish Reason: {result.Choices[0].FinishReason}");

                        Jit_Memory_Object.AddProperty("PhaseFourLowAnalysis", message.Content);
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_TINYLLAMA_RESPONSE", message.Content);
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_TINYLLAMA_STATUS", "CONNECTED");
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_TINYLLAMA_USAGE", result.Usage);
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_COMPLETE", true);
                        Console.WriteLine("⏳ Phase Four Progress: 100% - Success");
                        Console.WriteLine("✅ SUCCESS Phase Four TinyLlama interaction complete");
                        break;
                    }
                    else
                    {
                        throw new Exception("Invalid response format from TinyLlama");
                    }
                }
                catch (JsonException jex)
                {
                    Console.WriteLine($"❌ Phase Four JSON Error (Attempt {attempt}/{MAX_RETRIES}): {jex.Message}");
                    Console.WriteLine($"Path: {jex.Path}, LineNumber: {jex.LineNumber}, BytePositionInLine: {jex.BytePositionInLine}");

                    if (attempt == MAX_RETRIES)
                    {
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_TINYLLAMA_STATUS", "FAILED");
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_COMPLETE", false);
                        Console.WriteLine("❌ ERROR: Phase Four - All retry attempts exhausted");
                    }
                    else
                    {
                        Console.WriteLine($"⏳ Phase Four: Retrying in {RETRY_DELAY_MS}ms...");
                        await Task.Delay(RETRY_DELAY_MS * attempt);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Phase Four Error (Attempt {attempt}/{MAX_RETRIES}): {ex.Message}");

                    if (attempt == MAX_RETRIES)
                    {
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_TINYLLAMA_STATUS", "FAILED");
                        Jit_Memory_Object.AddProperty("PHASE_FOUR_COMPLETE", false);
                        Console.WriteLine("❌ ERROR: Phase Four - All retry attempts exhausted");
                    }
                    else
                    {
                        Console.WriteLine($"⏳ Phase Four: Retrying in {RETRY_DELAY_MS}ms...");
                        await Task.Delay(RETRY_DELAY_MS * attempt);
                    }
                }
            }

            timer.Stop();
            Console.WriteLine($"Phase Four execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }
    }

    public class PhaseFive : IPhase
    {
        private string _tinyLlamaUrl;
        private readonly HttpClient _httpClient;
        private const int MAX_RETRIES = 3;
        private const int RETRY_DELAY_MS = 1000;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public PhaseFive()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
            _tinyLlamaUrl = AppConfig.TINYLLAMA_URL;
        }

        public async Task ExecuteAsync()
        {
            var timer = Stopwatch.StartNew();
            Console.WriteLine("Initializing Phase Five - Final Analysis");

            try
            {
                // Check if both Phase Three and Four completed successfully
                var phaseThreeStatus = Jit_Memory_Object.GetProperty("PHASE_THREE_COMPLETE");
                var phaseFourStatus = Jit_Memory_Object.GetProperty("PHASE_FOUR_COMPLETE");

                if (phaseThreeStatus != null && phaseFourStatus != null &&
                    (bool)phaseThreeStatus && (bool)phaseFourStatus)
                {
                    // Retrieve the analyses from PhaseThree and PhaseFour
                    var highAnalysis = Jit_Memory_Object.GetProperty("PhaseThreeHighAnalysis");
                    var lowAnalysis = Jit_Memory_Object.GetProperty("PhaseFourLowAnalysis");

                    Console.WriteLine("\n=== Phase Five Initial Analysis Results ===");
                    Console.WriteLine($"High Analysis: {highAnalysis}");
                    Console.WriteLine($"Low Analysis: {lowAnalysis}");
                    Console.WriteLine("=== End of Initial Analysis Results ===\n");

                    Console.WriteLine("⏳ Phase Five Progress: 0% - Starting TinyLlama connection...");

                    for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
                    {
                        try
                        {
                            Console.WriteLine($"⏳ Phase Five Progress: {(attempt * 25)}% - Attempt {attempt} of {MAX_RETRIES}");

                            var payload = new
                            {
                                model = "tinyllama",
                                messages = new[]
                                {
                                    new { role = "user", content = $"Review the following high and low analyses for Inuvo stock: HIGH ANALYSIS: {highAnalysis} LOW ANALYSIS: {lowAnalysis}. Based on these analyses, provide a deduction for the highest upward fluctuation, including the exact time it occurs. Additionally, postulate the approximate time and expected high value for the next significant upward movement." }
                                },
                                stream = false,
                                options = new
                                {
                                    temperature = 0.7,
                                    max_tokens = 150
                                }
                            };

                            var content = new StringContent(
                                 JsonSerializer.Serialize(payload, _jsonOptions),
                                 Encoding.UTF8,
                                 "application/json");

                            Console.WriteLine($"📡 Phase Five: Sending request to TinyLlama (Attempt {attempt})...");
                            var response = await _httpClient.PostAsync($"{_tinyLlamaUrl}/v1/chat/completions", content);

                            var responseContent = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"📥 Phase Five Raw Response: {responseContent}");

                            if (!response.IsSuccessStatusCode)
                            {
                                throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                            }

                            var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent, _jsonOptions);

                            if (result?.Choices != null && result.Choices.Count > 0)
                            {
                                var message = result.Choices[0].Message;
                                Console.WriteLine($"⏳ Phase Five Progress: 75% - Processing response...");
                                Console.WriteLine($"Phase Five TinyLlama Response Content: {message.Content}");
                                Console.WriteLine($"Phase Five Response Details:");
                                Console.WriteLine($"- Model: {result.Model}");
                                Console.WriteLine($"- Tokens Used: {result.Usage.TotalTokens}");
                                Console.WriteLine($"- Finish Reason: {result.Choices[0].FinishReason}");

                                Jit_Memory_Object.AddProperty("PhaseFiveCombinedAnalysis", message.Content);
                                Jit_Memory_Object.AddProperty("PHASE_FIVE_TINYLLAMA_RESPONSE", message.Content);
                                Jit_Memory_Object.AddProperty("PHASE_FIVE_TINYLLAMA_STATUS", "CONNECTED");
                                Jit_Memory_Object.AddProperty("PHASE_FIVE_TINYLLAMA_USAGE", result.Usage);
                                Jit_Memory_Object.AddProperty("PHASE_FIVE_COMPLETE", true);
                                Console.WriteLine("⏳ Phase Five Progress: 100% - Success");
                                Console.WriteLine("✅ SUCCESS Phase Five TinyLlama interaction complete");

                                // Process and display final results
                                DisplayFinalResults();
                                break;
                            }
                            else
                            {
                                throw new Exception("Invalid response format from TinyLlama");
                            }
                        }
                        catch (JsonException jex)
                        {
                            Console.WriteLine($"❌ Phase Five JSON Error (Attempt {attempt}/{MAX_RETRIES}): {jex.Message}");
                            Console.WriteLine($"Path: {jex.Path}, LineNumber: {jex.LineNumber}, BytePositionInLine: {jex.BytePositionInLine}");

                            if (attempt == MAX_RETRIES)
                            {
                                Jit_Memory_Object.AddProperty("PHASE_FIVE_TINYLLAMA_STATUS", "FAILED");
                                Jit_Memory_Object.AddProperty("PHASE_FIVE_COMPLETE", false);
                                Console.WriteLine("❌ ERROR: Phase Five - All retry attempts exhausted");
                            }
                            else
                            {
                                Console.WriteLine($"⏳ Phase Five: Retrying in {RETRY_DELAY_MS}ms...");
                                await Task.Delay(RETRY_DELAY_MS * attempt);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Phase Five Error (Attempt {attempt}/{MAX_RETRIES}): {ex.Message}");

                            if (attempt == MAX_RETRIES)
                            {
                                Jit_Memory_Object.AddProperty("PHASE_FIVE_TINYLLAMA_STATUS", "FAILED");
                                Jit_Memory_Object.AddProperty("PHASE_FIVE_COMPLETE", false);
                                Console.WriteLine("❌ ERROR: Phase Five - All retry attempts exhausted");
                            }
                            else
                            {
                                Console.WriteLine($"⏳ Phase Five: Retrying in {RETRY_DELAY_MS}ms...");
                                await Task.Delay(RETRY_DELAY_MS * attempt);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("❌ ERROR: Phase Five cancelled - Dependencies not met");
                    Jit_Memory_Object.AddProperty("PHASE_FIVE_COMPLETE", false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Phase Five: {ex.Message}");
                Jit_Memory_Object.AddProperty("PHASE_FIVE_COMPLETE", false);
            }

            timer.Stop();
            Console.WriteLine($"Phase Five execution time: {timer.Elapsed.TotalSeconds:F3} seconds");
        }

        public void DisplayFinalResults()
        {
            Console.WriteLine("\n=== Final Analysis Results ===");

            var highMagnitude = Jit_Memory_Object.GetProperty("HIGH_CLUSTER_VECTOR_MAGNITUDE");
            var lowMagnitude = Jit_Memory_Object.GetProperty("LOW_CLUSTER_VECTOR_MAGNITUDE");

            Console.WriteLine("\nVector Magnitudes:");
            Console.WriteLine($"High Cluster: {highMagnitude:F4}");
            Console.WriteLine($"Low Cluster: {lowMagnitude:F4}");

            var phaseThreeResponse = Jit_Memory_Object.GetProperty("PHASE_THREE_TINYLLAMA_RESPONSE");
            var phaseFourResponse = Jit_Memory_Object.GetProperty("PHASE_FOUR_TINYLLAMA_RESPONSE");
            var phaseFiveResponse = Jit_Memory_Object.GetProperty("PHASE_FIVE_TINYLLAMA_RESPONSE");

            Console.WriteLine("\nAI Analysis Results:");
            Console.WriteLine("High Analysis:");
            Console.WriteLine(phaseThreeResponse);
            Console.WriteLine("\nLow Analysis:");
            Console.WriteLine(phaseFourResponse);
            Console.WriteLine("\nFinal Prediction:");
            Console.WriteLine(phaseFiveResponse);

            var highData = Jit_Memory_Object.GetProperty("INUV_High") as dynamic[];
            var lowData = Jit_Memory_Object.GetProperty("INUV_Low") as dynamic[];

            if (highData != null && lowData != null)
            {
                Console.WriteLine("\nPrice Movement Visualization:");
                RenderPriceGraph(highData, lowData);
            }
            else
            {
                Console.WriteLine("No data available for visualization");
            }
        }

        private void RenderPriceGraph(dynamic[] highData, dynamic[] lowData)
        {
            const int GRAPH_HEIGHT = 20;
            const int GRAPH_WIDTH = 100;

            // Convert and combine high and low data points
            var allPoints = new List<(DateTime time, decimal price, string type)>();

            foreach (var point in highData)
            {
                allPoints.Add((
                    DateTime.Parse(point.DateTime.ToString()),
                    Convert.ToDecimal(point.High),
                    "H"
                ));
            }

            foreach (var point in lowData)
            {
                allPoints.Add((
                    DateTime.Parse(point.DateTime.ToString()),
                    Convert.ToDecimal(point.Low),
                    "L"
                ));
            }

            // Sort by time
            allPoints = allPoints.OrderBy(p => p.time).ToList();

            // Calculate ranges for the graph
            decimal maxValue = allPoints.Max(p => p.price);
            decimal minValue = allPoints.Min(p => p.price);
            decimal valueRange = maxValue - minValue;

            // Print the price scale and plot points
            for (int i = GRAPH_HEIGHT - 1; i >= 0; i--)
            {
                decimal rowValue = minValue + (valueRange * i / (GRAPH_HEIGHT - 1));
                Console.Write($"{rowValue,8:F4} |");

                foreach (var point in allPoints)
                {
                    decimal normalizedDiff = Math.Abs(point.price - rowValue);
                    decimal threshold = valueRange / GRAPH_HEIGHT;

                    if (normalizedDiff < threshold)
                    {
                        Console.Write(point.type);
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }
                Console.WriteLine();
            }

            // Print time axis
            Console.Write("         |");
            Console.WriteLine(new string('-', allPoints.Count));

            // Print time scale markers
            Console.Write("         |");
            for (int i = 0; i < allPoints.Count; i++)
            {
                Console.Write(i % 5 == 0 ? "+" : "-");
            }
            Console.WriteLine();

            // Print time labels
            var timeLabels = allPoints
                .Where((p, i) => i % 5 == 0)
                .Select(p => p.time.ToString("HH:mm").PadRight(5));
            Console.Write("Time     |");
            Console.WriteLine(string.Join("", timeLabels));

            // Print statistical summary
            PrintStatisticalSummary(allPoints);
        }
        private static void PrintStatisticalSummary(List<(DateTime time, decimal price, string type)> allPoints)
        {
            // Separate points by type and convert to decimal lists
            var highPoints = allPoints
                .Where(p => p.type == "H")
                .Select(p => p.price)
                .ToList();

            var lowPoints = allPoints
                .Where(p => p.type == "L")
                .Select(p => p.price)
                .ToList();

            Console.WriteLine("\n=== Statistical Summary ===");

            Console.WriteLine($"\nHigh Values:");
            Console.WriteLine($"  Maximum: ${highPoints.Max():F4}");
            Console.WriteLine($"  Minimum: ${highPoints.Min():F4}");
            Console.WriteLine($"  Average: ${highPoints.Average():F4}");

            Console.WriteLine($"\nLow Values:");
            Console.WriteLine($"  Maximum: ${lowPoints.Max():F4}");
            Console.WriteLine($"  Minimum: ${lowPoints.Min():F4}");
            Console.WriteLine($"  Average: ${lowPoints.Average():F4}");

            // Calculate price movement metrics
            var priceRange = highPoints.Max() - lowPoints.Min();
            var volatility = priceRange / lowPoints.Min() * 100;

            Console.WriteLine($"\nPrice Movement Metrics:");
            Console.WriteLine($"  Total Range: ${priceRange:F4}");
            Console.WriteLine($"  Volatility: {volatility:F2}%");

            // Time-based analysis
            var timeRange = allPoints.Max(p => p.time) - allPoints.Min(p => p.time);

            // Find top 3 highest points
            var topHighPoints = allPoints
                .Where(p => p.type == "H")
                .OrderByDescending(p => p.price)
                .Take(3)
                .Select(p => $"{p.time:HH:mm} (${p.price:F4})")
                .ToList();

            Console.WriteLine($"\nTime Analysis:");
            Console.WriteLine($"  Time Range: {timeRange.TotalHours:F1} hours");
            Console.WriteLine($"  Top 3 High Times: {string.Join(", ", topHighPoints)}");
        }
    }
}