using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NextTech.Domain.Entities;

public class Story
{
    public Story(int id, string type, string title, string? url, string by, int score, DateTime time)
    {
        Id = id;
        Title = title;
        Url = url;
        By = by;
        Score = score;
        Time = time;
        @Type = type;
    }

    public int Id { get; private set; }
    public string Title { get; private set; }
    public string? Url { get; private set; }
    public string By { get; private set; }
    public int Score { get; private set; }

    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime Time { get; private set; }
    public string @Type { get; set; }

}