namespace NextTech.Application.Story;
public class StoryDto
{
    public StoryDto(int id, string title, string? url, string by, int score, DateTime time)
    {
        Id = id;
        Title = title;
        Url = url;
        By = by;
        Score = score;
        Time = time;
    }

    public int Id { get; private set; }
    public string Title { get; private set; }
    public string? Url { get; private set; }
    public string By { get; private set; }
    public int Score { get; private set; }
    public DateTime Time { get; private set; }

}