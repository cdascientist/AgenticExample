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

            var factory = new PhaseFactory();
            var orchestrator = new PhaseOrchestrator();

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

    public class TinyLlamaResponse
    {
        public string model_id { get; set; }
        public string created_at { get; set; }
        public string response { get; set; }
        public bool done { get; set; }
        public Context context { get; set; }
        public int total_duration { get; set; }
        public int load_duration { get; set; }
        public int prompt_eval_duration { get; set; }
        public int eval_duration { get; set; }
        public float eval_count { get; set; }
    }

    public class Context
    {
        public List<string> history { get; set; }
        public string system { get; set; }
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
                prompt = $@"[{name}|{role}]: Analyze the following time series data for Cluster {cluster.ClusterIndex + 1}:
                          Average High: ${cluster.AverageHigh:F2}
                          Time Range: {string.Join(", ", cluster.Times.Select(t => t.ToString("HH:mm:ss")))}
                          Based on this data, what is the most probable time period for this average high to occur?",
                stream = false
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{ngrokUrl}/api/generate", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = JsonConvert.DeserializeObject<TinyLlamaResponse>(await response.Content.ReadAsStringAsync(cancellationToken));
            return result.response;
        }
    }

    public class PhaseOrchestrator
    {
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
                    System.Diagnostics.Debug.WriteLine($"  Times: {string.Join(", ", group.Times.Select(t => t.ToString("HH:mm:ss")))}");
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

        public PhaseTwo(string apiKey) : base(apiKey)
        {
            agent = new AutoGenAgent("Agent1", "Research Assistant", Program.LLAMA_URL);
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
                    foreach (var cluster in clusterResults.Clusters)
                    {
                        var prediction = await agent.PredictHighProbabilityPeriod(cluster, cancellationToken);
                        System.Diagnostics.Debug.WriteLine($"Cluster {cluster.ClusterIndex + 1} Probability Analysis:");
                        System.Diagnostics.Debug.WriteLine($"LLM Response: {prediction}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Phase Two Complete: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Phase Two Error: {ex.Message}");
                return false;
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
        public PhaseFour(string apiKey) : base(apiKey) { }

        public override async Task<ClusterResults> ExecuteAsync()
        {
            await Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Starting Phase Four: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                Thread.Sleep(800);
                System.Diagnostics.Debug.WriteLine($"Phase Four Complete: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            });
            return new ClusterResults { Clusters = new List<ClusterGroup>() };
        }
    }

    public class PhaseFactory
    {
        public IPhase CreatePhase(string phaseType, string apiKey)
        {
            return phaseType switch
            {
                "PhaseOne" => new PhaseOne(apiKey),
                "PhaseTwo" => new PhaseTwo(apiKey),
                "PhaseThree" => new PhaseThree(apiKey),
                "PhaseFour" => new PhaseFour(apiKey),
                _ => throw new ArgumentException("Invalid phase type")
            };
        }
    }
}