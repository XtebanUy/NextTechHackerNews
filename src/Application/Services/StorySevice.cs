using NextTech.Application.Common.Interfaces;
using NextTech.Application.Common.Models;
using NextTech.Application.Story;
using NextTech.Application.Interfaces;

namespace NextTech.Application.Services;

public class StoryService : IStoryService
{
    private readonly IRepositoryService _respository;

    public StoryService(IRepositoryService respository)
    {
        _respository = respository;
    }
    public async Task<PaginatedList<StoryDto>> GetStories(StoryPaginatedFilter storyPaginatedFilter)
    {
        var stories = await _respository.GetStoriesFromApi();
        var storiesFiltered = stories.AsQueryable();

        if (storyPaginatedFilter.Title != null)
        {
            storiesFiltered = storiesFiltered.Where(s => s.Title.Contains(storyPaginatedFilter.Title));
        }

        if (storyPaginatedFilter.MinScore != null)
        {
            storiesFiltered = storiesFiltered.Where(s => s.Score >= storyPaginatedFilter.MinScore);
        }

        if (storyPaginatedFilter.MaxScore != null)
        {
            storiesFiltered = storiesFiltered.Where(s => s.Score <= storyPaginatedFilter.MaxScore);
        }

        if (storyPaginatedFilter.MinTime != null)
        {
            storiesFiltered = storiesFiltered.Where(s => s.Time >= storyPaginatedFilter.MinTime);
        }

        if (storyPaginatedFilter.MaxTime != null)
        {
            storiesFiltered = storiesFiltered.Where(s => s.Time <= storyPaginatedFilter.MaxTime);
        }
        
        return PaginatedList<StoryDto>.Create(storiesFiltered.Select(s => new StoryDto(s.Id, s.Title, s.Url, s.By, s.Score, s.Time)), storyPaginatedFilter.PageSize, storyPaginatedFilter.PageNumber);
    }
}