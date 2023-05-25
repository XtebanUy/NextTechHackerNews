namespace NextTech.Application.Story;

public record StoryPaginatedFilter
{
    public string? Title { get; init; }
    public int? MinScore { get; set; }
    public int? MaxScore { get; set; }
    public DateTime? MinTime {get; set; }
    public DateTime? MaxTime {get; set; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}