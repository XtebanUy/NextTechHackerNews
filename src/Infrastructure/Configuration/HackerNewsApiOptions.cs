namespace NextTech.Infrastructure.Configuration;

public class HackerNewsApiOptions
{
    public const string HackerNewsApi = "HackerNewsApi"; 

    public string ServiceBaseUrl { get; set; }
    public int MaxDegreeOfParallelism { get; set; }
}