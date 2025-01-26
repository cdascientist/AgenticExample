using System.Text.Json;
using Accord.MachineLearning;
using System.Linq;
using Newtonsoft.Json;
using System.Text;

namespace AgenticExample
{
    internal class Program
    {
        private const string API_KEY = "2D98SFTA8JW1TVHD";
        private const string BASE_URL = "https://www.alphavantage.co/query";
        public static string LLAMA_URL = "https://da31-34-74-128-6.ngrok-free.app";

        static async Task Main(string[] args)
        {
            var totalTimer = System.Diagnostics.Stopwatch.StartNew();
            System.Diagnostics.Debug.WriteLine($"Application Start: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");

            var orchestrator = new PhaseOrchestrator();
            var factory = new PhaseFactory(orchestrator);

            var phaseOne = factory.CreatePhase("PhaseOne", API_KEY);
            var phaseTwo = factory.CreatePhase("PhaseTwo", API_KEY);
            var phaseThree = factory.CreatePhase("PhaseThree", API_KEY);
            var phaseFour = factory.CreatePhase("PhaseFour", API_KEY);

            var phaseOneResults = await orchestrator.ExecutePhase(phaseOne);

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var (phase2Result, phase3Result) = await orchestrator.ExecuteParallelPhases(phaseTwo, phaseThree, phaseOneResults, cts.Token);

            if (phase2Result && phase3Result)
            {
                await orchestrator.ExecutePhase(phaseFour);
            }

            totalTimer.Stop();
            var totalSeconds = totalTimer.ElapsedMilliseconds / 1000.0;
            System.Diagnostics.Debug.WriteLine($"Total Application Execution Time: {totalTimer.ElapsedMilliseconds}ms ({totalSeconds:F3} seconds)");
        }
    }

    public class AnalysisResult
    {
        public List<TradingPeriod> TopPeriods { get; set; }
        public string Analysis { get; set; }
        public Dictionary<string, double> Confidence { get; set; }
    }

    public class TradingPeriod
    {
        public int ClusterIndex { get; set; }
        public double Price { get; set; }
        public string PeakTime { get; set; }
        public double ActivityPercentage { get; set; }
    }

    public class TinyLlamaResponse
    {
        public string model_id { get; set; }
        public string created_at { get; set; }
        public string response { get; set; }
        public bool done { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> context { get; set; }
        public long total_duration { get; set; }
        public long load_duration { get; set; }
        public long prompt_eval_duration { get; set; }
        public long eval_duration { get; set; }
        public float eval_count { get; set; }
    }

    public class AutoGenAgent
    {
        private readonly string name;
        private readonly string role;
        private readonly HttpClient client;
        private readonly string ngrokUrl;

        public AutoGenAgent(string name, string role, string ngrokUrl)
        {
            this.name = name;
            this.role = role;
            this.ngrokUrl = ngrokUrl;
            client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            System.Diagnostics.Debug.WriteLine($"Created AutoGen Agent: {name} ({role})");
        }

        public async Task<string> PredictHighProbabilityPeriod(ClusterGroup cluster, CancellationToken cancellationToken)
        {
            var payload = new
            {
                model = "tinyllama",
                prompt = $@"[{name}|{role}]: Analyze these intraday trading patterns:
                          Period {cluster.ClusterIndex + 1}: ${cluster.AverageHigh:F2}
                          Time Range: {string.Join(", ", cluster.Times.Select(t => t.ToString("hh:mm tt")))}

                          Based strictly on this time series data:
                          1. What time periods show the highest consistency?
                          2. What are the key price-time relationships?
                          3. Identify specific high-probability time windows
                          4. Statistical correlation between time and price activity

                          Provide a concise, data-driven analysis with no external factors or assumptions.",
                stream = false
            };

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{ngrokUrl}/api/generate", content, linkedCts.Token);
                response.EnsureSuccessStatusCode();

                var jsonSettings = new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"JSON Error: {args.ErrorContext.Error.Message}");
                        args.ErrorContext.Handled = true;
                    }
                };

                var result = JsonConvert.DeserializeObject<TinyLlamaResponse>(
                    await response.Content.ReadAsStringAsync(linkedCts.Token),
                    jsonSettings);

                return result?.response ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Agent Error: {ex.Message}");
                return string.Empty;
            }
        }
    }

    public class PhaseOrchestrator
    {
        private Dictionary<string, IPhase> phases = new Dictionary<string, IPhase>();

        public IPhase GetPhase(string phaseName)
        {
            if (phases.TryGetValue(phaseName, out var phase))
            {
                return phase;
            }
            return null;
        }

        public void RegisterPhase(string phaseName, IPhase phase)
        {
            phases[phaseName] = phase;
        }

        public async Task<ClusterResults> ExecutePhase(IPhase phase)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var result = await phase.ExecuteAsync();
            timer.Stop();
            var seconds = timer.ElapsedMilliseconds / 1000.0;
            System.Diagnostics.Debug.WriteLine($"{phase.GetType().Name} Execution Time: {timer.ElapsedMilliseconds}ms ({seconds:F3} seconds)");
            return result;
        }

        public async Task<(bool phase2Complete, bool phase3Complete)> ExecuteParallelPhases(
            IPhase phase1, IPhase phase2, ClusterResults clusterResults, CancellationToken cancellationToken)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();

            var phase2Task = ((PhaseTwo)phase1).ExecuteAsync(clusterResults, cancellationToken);
            var phase3Task = ((PhaseThree)phase2).ExecuteAsync(clusterResults);

            try
            {
                await Task.WhenAll(phase2Task, phase3Task);

                timer.Stop();
                var seconds = timer.ElapsedMilliseconds / 1000.0;
                System.Diagnostics.Debug.WriteLine($"Parallel Phase Execution Time: {timer.ElapsedMilliseconds}ms ({seconds:F3} seconds)");

                return (phase2Task.Result, phase3Task.Result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Parallel Phase Error: {ex.Message}");
                return (false, false);
            }
        }
    }

    public class ClusterResults
    {
        public List<ClusterGroup> Clusters { get; set; }
    }

    public class ClusterGroup
    {
        public int ClusterIndex { get; set; }
        public double AverageHigh { get; set; }
        public List<DateTime> Times { get; set; }
    }

    public interface IPhase
    {
        Task<ClusterResults> ExecuteAsync();
    }

    public abstract class BasePhase : IPhase
    {
        protected readonly string ApiKey;
        protected readonly string BaseUrl;
        protected readonly HttpClient Client;

        protected BasePhase(string apiKey)
        {
            ApiKey = apiKey;
            BaseUrl = "https://www.alphavantage.co/query";
            Client = new HttpClient();
        }

        public abstract Task<ClusterResults> ExecuteAsync();
    }

    public class PhaseOne : BasePhase
    {
        public PhaseOne(string apiKey) : base(apiKey) { }

        public override async Task<ClusterResults> ExecuteAsync()
        {
            System.Diagnostics.Debug.WriteLine($"Starting Phase One: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");

            var parameters = new Dictionary<string, string>
            {
                { "function", "TIME_SERIES_INTRADAY" },
                { "symbol", "INUV" },
                { "interval", "5min" },
                { "outputsize", "full" }
            };

            try
            {
                var url = $"{BaseUrl}?{string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"))}";
                var response = await Client.GetStringAsync($"{url}&apikey={ApiKey}");

                var jsonDoc = JsonDocument.Parse(response);
                var timeSeries = jsonDoc.RootElement.GetProperty("Time Series (5min)");

                var cutoffDate = DateTime.Now.AddDays(-15);
                var dataPoints = new List<(DateTime time, double high)>();

                foreach (var timePoint in timeSeries.EnumerateObject())
                {
                    var timestamp = DateTime.Parse(timePoint.Name);
                    if (timestamp >= cutoffDate)
                    {
                        var high = double.Parse(timePoint.Value.GetProperty("2. high").GetString());
                        dataPoints.Add((timestamp, high));
                    }
                }

                var observations = dataPoints.Select(p => new[] { p.high }).ToArray();
                var kmeans = new KMeans(k: 5);
                var clusters = kmeans.Learn(observations);
                var assignments = clusters.Decide(observations);

                var clusterGroups = dataPoints.Zip(assignments, (point, cluster) => (point.time, point.high, cluster))
                                            .GroupBy(x => x.cluster)
                                            .OrderBy(g => g.Key)
                                            .Select(g => new ClusterGroup
                                            {
                                                ClusterIndex = g.Key,
                                                AverageHigh = g.Average(x => x.high),
                                                Times = g.Select(x => x.time).ToList()
                                            })
                                            .ToList();

                var results = new ClusterResults { Clusters = clusterGroups };

                foreach (var group in clusterGroups)
                {
                    System.Diagnostics.Debug.WriteLine($"Cluster {group.ClusterIndex + 1}:");
                    System.Diagnostics.Debug.WriteLine($"  Average High: ${group.AverageHigh:F2}");
                    System.Diagnostics.Debug.WriteLine($"  Times: {string.Join(", ", group.Times.Select(t => t.ToString("hh:mm tt")))}");
                    System.Diagnostics.Debug.WriteLine("-------------------");
                }

                return results;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
                return new ClusterResults { Clusters = new List<ClusterGroup>() };
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine($"Phase One Complete: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            }
        }
    }

    public class PhaseTwo : BasePhase
    {
        private readonly AutoGenAgent agent;
        private readonly ProgressBar progressBar;
        private readonly PhaseOrchestrator orchestrator;

        public PhaseTwo(string apiKey, PhaseOrchestrator orchestrator) : base(apiKey)
        {
            this.orchestrator = orchestrator;
            agent = new AutoGenAgent("Agent1", "Research Assistant", Program.LLAMA_URL);
            progressBar = new ProgressBar(100);
        }

        public override async Task<ClusterResults> ExecuteAsync()
        {
            return new ClusterResults { Clusters = new List<ClusterGroup>() };
        }

        public async Task<bool> ExecuteAsync(ClusterResults clusterResults, CancellationToken cancellationToken)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Starting Phase Two: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");

                if (clusterResults?.Clusters != null)
                {
                    var allProbabilities = new List<(int clusterIndex, double avgHigh, TimeSpan hour, double frequency)>();
                    var analysisResult = new AnalysisResult
                    {
                        TopPeriods = new List<TradingPeriod>(),
                        Confidence = new Dictionary<string, double>()
                    };

                    foreach (var cluster in clusterResults.Clusters)
                    {
                        var timeGroups = cluster.Times
                            .GroupBy(t => new TimeSpan(t.Hour, 0, 0))
                            .Select(g => new
                            {
                                Hour = g.Key,
                                Frequency = (double)g.Count() / cluster.Times.Count,
                                ClusterIndex = cluster.ClusterIndex,
                                AvgHigh = cluster.AverageHigh
                            });

                        allProbabilities.AddRange(timeGroups.Select(g => (g.ClusterIndex, g.AvgHigh, g.Hour, g.Frequency)));

                        for (int i = 0; i <= 100; i++)
                        {
                            progressBar.Update(i);
                            await Task.Delay(20, cancellationToken);
                        }
                    }

                    System.Diagnostics.Debug.WriteLine("\nProbability Analysis Results:");
                    System.Diagnostics.Debug.WriteLine("----------------------------------------");

                    var orderedResults = allProbabilities
                        .OrderByDescending(r => r.avgHigh)
                        .ThenByDescending(r => r.frequency)
                        .GroupBy(r => r.clusterIndex)
                        .Take(3);

                    foreach (var clusterGroup in orderedResults)
                    {
                        var topPeriod = clusterGroup
                            .OrderByDescending(r => r.frequency)
                            .First();

                        var formattedTime = DateTime.Today.Add(topPeriod.hour).ToString("hh:mm tt");

                        analysisResult.TopPeriods.Add(new TradingPeriod
                        {
                            ClusterIndex = topPeriod.clusterIndex + 1,
                            Price = topPeriod.avgHigh,
                            PeakTime = formattedTime,
                            ActivityPercentage = topPeriod.frequency
                        });

                        analysisResult.Confidence[$"Cluster{topPeriod.clusterIndex + 1}"] =
                            Math.Round(topPeriod.frequency * (topPeriod.avgHigh / allProbabilities.Max(x => x.avgHigh)) * 100, 2);

                        System.Diagnostics.Debug.WriteLine($"Cluster {topPeriod.clusterIndex + 1}");
                        System.Diagnostics.Debug.WriteLine($"   Price: ${topPeriod.avgHigh:F2}");
                        System.Diagnostics.Debug.WriteLine($"   Peak Time: {formattedTime}");
                        System.Diagnostics.Debug.WriteLine($"   Activity: {topPeriod.frequency:P1}");
                        System.Diagnostics.Debug.WriteLine($"   Confidence: {analysisResult.Confidence[$"Cluster{topPeriod.clusterIndex + 1}"]:F1}%");
                        System.Diagnostics.Debug.WriteLine("----------------------------------------");
                    }

                    var prompt = $@"[Agent1|Research Assistant]: Analyze these intraday trading patterns:
                        {string.Join("\n", analysisResult.TopPeriods.Select(p =>
                            $"Period {p.ClusterIndex}: ${p.Price:F2} at {p.PeakTime} ({p.ActivityPercentage:P1} activity)"))}

                        Based strictly on this intraday time series data:
                        1. What time periods show the highest consistency?
                        2. What are the key price-time relationships?
                        3. Identify specific high-probability time windows
                        4. Statistical correlation between time and price activity

                        Provide a concise, data-driven analysis with no external factors or assumptions.";

                    var analysis = await agent.PredictHighProbabilityPeriod(
                        new ClusterGroup { Times = new List<DateTime>() }, cancellationToken) ??
                        "Analysis unavailable - using statistical patterns only.";

                    analysisResult.Analysis = analysis;

                    System.Diagnostics.Debug.WriteLine("\nPattern Analysis:");
                    System.Diagnostics.Debug.WriteLine(analysis);

                    ((PhaseFour)orchestrator.GetPhase("PhaseFour")).SetAnalysisResult(analysisResult);
                }

                System.Diagnostics.Debug.WriteLine($"Phase Two Complete: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Phase Two Error: {ex.Message}");
                return false;
            }
            finally
            {
                progressBar.Dispose();
            }
        }
    }

    public class PhaseThree : BasePhase
    {
        private readonly AutoGenAgent agent;

        public PhaseThree(string apiKey) : base(apiKey)
        {
            agent = new AutoGenAgent("Agent2", "Data Analyst", Program.LLAMA_URL);
        }

        public override async Task<ClusterResults> ExecuteAsync()
        {
            return new ClusterResults { Clusters = new List<ClusterGroup>() };
        }

        public async Task<bool> ExecuteAsync(ClusterResults clusterResults)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Starting Phase Three: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                System.Diagnostics.Debug.WriteLine($"Phase Three Complete: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Phase Three Error: {ex.Message}");
                return false;
            }
        }
    }

    public class PhaseFour : BasePhase
    {
        private AnalysisResult analysisResult;

        public PhaseFour(string apiKey) : base(apiKey) { }

        public void SetAnalysisResult(AnalysisResult result)
        {
            analysisResult = result;
        }

        public override async Task<ClusterResults> ExecuteAsync()
        {
            await Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Starting Phase Four: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                if (analysisResult != null)
                {
                    System.Diagnostics.Debug.WriteLine("\nFinal Analysis Results:");
                    System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(analysisResult, Formatting.Indented));
                }
                Thread.Sleep(800);
                System.Diagnostics.Debug.WriteLine($"Phase Four Complete: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            });
            return new ClusterResults { Clusters = new List<ClusterGroup>() };
        }
    }

    public class ProgressBar : IDisposable
    {
        private readonly int totalProgress;
        private int currentProgress;
        private readonly int barWidth;
        private readonly string animation = "█";

        public ProgressBar(int total, int width = 30)
        {
            totalProgress = total;
            barWidth = width;
            currentProgress = 0;
        }

        public void Update(int progress)
        {
            currentProgress = progress;
            float percentage = (float)currentProgress / totalProgress;
            int filled = (int)(barWidth * percentage);

            System.Diagnostics.Debug.Write("\r[");
            System.Diagnostics.Debug.Write(string.Concat(Enumerable.Repeat(animation, filled)));
            System.Diagnostics.Debug.Write(string.Concat(Enumerable.Repeat(" ", barWidth - filled)));
            System.Diagnostics.Debug.Write($"] {percentage:P0}");
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.WriteLine("");
        }
    }

    public class PhaseFactory
    {
        private readonly PhaseOrchestrator orchestrator;

        public PhaseFactory(PhaseOrchestrator orchestrator)
        {
            this.orchestrator = orchestrator;
        }

        public IPhase CreatePhase(string phaseType, string apiKey)
        {
            IPhase phase = phaseType switch
            {
                "PhaseOne" => new PhaseOne(apiKey),
                "PhaseTwo" => new PhaseTwo(apiKey, orchestrator),
                "PhaseThree" => new PhaseThree(apiKey),
                "PhaseFour" => new PhaseFour(apiKey),
                _ => throw new ArgumentException("Invalid phase type")
            };

            orchestrator.RegisterPhase(phaseType, phase);
            return phase;
        }
    }
}