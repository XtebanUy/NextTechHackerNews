using NextTech.Application.Common.Models;
using NextTech.Application.Story;

namespace NextTech.Application.Common.Interfaces;

public interface IStoryService {
    public Task<PaginatedList<StoryDto>> GetStories(StoryPaginatedFilter filter);
}