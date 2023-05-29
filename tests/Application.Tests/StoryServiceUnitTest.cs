using Moq;
using NextTech.Application.Interfaces;
using NextTech.Application.Services;
using NextTech.Domain.Entities;

namespace Application.Tests;

public class StoryServiceUnitTest
{
    private List<Story> GetInitialStories()
    {
        
        return Enumerable.Range(0, 500).Select(i => new Story(i, "story", $"title {i}", $"http://url.com/{i}", GetBy(i), GetScore(i), GetTime(i))).ToList();
        static int GetScore(int i) => i % 7;
        static string GetBy(int i) => $"by{i%3}";
        static DateTime GetTime(int i) => DateTime.Parse($"2023-05-0{(i%3)+1}");
    }

    [Fact]
    public async Task SearchWithoutFiltersFirstPageTest()
    {
        var testStories = GetInitialStories();
        var repositoryMock = new Mock<IRepositoryService>();
        repositoryMock.Setup(l => l.GetStoriesFromApi()).ReturnsAsync(testStories);
        var storyService = new StoryService(repositoryMock.Object);
        var stories = await storyService.GetStories(new NextTech.Application.Story.StoryPaginatedFilter{
            PageNumber = 1,
            PageSize = 10
        });
        

        Assert.Equal(500, stories.TotalCount);
        Assert.Equal(10, stories.Items.Count);
        Assert.Equal(1, stories.PageNumber);
        Assert.Equal(50, stories.TotalPages);
        Assert.Equal(true, stories.HasNextPage);
        Assert.Equal(false, stories.HasPreviousPage);
    }

    [Fact]
    public async Task SearchWithoutFiltersMiddlePageTest()
    {
        var testStories = GetInitialStories();
        var repositoryMock = new Mock<IRepositoryService>();
        repositoryMock.Setup(l => l.GetStoriesFromApi()).ReturnsAsync(testStories);
        var storyService = new StoryService(repositoryMock.Object);
        var stories = await storyService.GetStories(new NextTech.Application.Story.StoryPaginatedFilter{
            PageNumber = 25,
            PageSize = 10
        });
        

        Assert.Equal(500, stories.TotalCount);
        Assert.Equal(10, stories.Items.Count);
        Assert.Equal(25, stories.PageNumber);
        Assert.Equal(50, stories.TotalPages);
        Assert.Equal(true, stories.HasNextPage);
        Assert.Equal(true, stories.HasPreviousPage);
    }

    [Fact]
    public async Task SearchWithoutFiltersLastPageTest()
    {
        var testStories = GetInitialStories();
        var repositoryMock = new Mock<IRepositoryService>();
        repositoryMock.Setup(l => l.GetStoriesFromApi()).ReturnsAsync(testStories);
        var storyService = new StoryService(repositoryMock.Object);

        var stories = await storyService.GetStories(new NextTech.Application.Story.StoryPaginatedFilter{
            PageNumber = 50,
            PageSize = 10
        });


        Assert.Equal(500, stories.TotalCount);
        Assert.Equal(10, stories.Items.Count);
        Assert.Equal(50, stories.PageNumber);
        Assert.Equal(50, stories.TotalPages);
        Assert.Equal(false, stories.HasNextPage);
        Assert.Equal(true, stories.HasPreviousPage);
    }

    [Fact]
    public async Task TitleSearchTest()
    {
        var repositoryMock = new Mock<IRepositoryService>();
        repositoryMock.Setup(l => l.GetStoriesFromApi()).ReturnsAsync(GetInitialStories());
        var storyService = new StoryService(repositoryMock.Object);
        var stories = await storyService.GetStories(new NextTech.Application.Story.StoryPaginatedFilter{
            Title = "title 1",
            PageNumber = 1,
            PageSize = 10
        });
        
        //title 1: 1, title 1x: 10, title 1xx: 100, total 100
        Assert.Equal(111, stories.TotalCount);
        Assert.Equal(10, stories.Items.Count);
        Assert.Equal(1, stories.PageNumber);
        Assert.Equal(12, stories.TotalPages);
        Assert.Equal(true, stories.HasNextPage);
        Assert.Equal(false, stories.HasPreviousPage);
    }

    [Fact]
    public async Task ScoreSearchTest()
    {
        var repositoryMock = new Mock<IRepositoryService>();
        repositoryMock.Setup(l => l.GetStoriesFromApi()).ReturnsAsync(GetInitialStories());
        var storyService = new StoryService(repositoryMock.Object);
        var stories = await storyService.GetStories(new NextTech.Application.Story.StoryPaginatedFilter{
            MinScore = 1,
            MaxScore = 2,
            PageNumber = 1,
            PageSize = 10
        });
        
        //title 1: 1, title 1x: 10, title 1xx: 100, total 100
        Assert.Equal(144, stories.TotalCount);
        Assert.Equal(10, stories.Items.Count);
        Assert.Equal(1, stories.PageNumber);
        Assert.Equal(15, stories.TotalPages);
        Assert.Equal(true, stories.HasNextPage);
        Assert.Equal(false, stories.HasPreviousPage);
    }

    [Fact]
    public async Task TimeSearchTest()
    {
        var repositoryMock = new Mock<IRepositoryService>();
        var initial = GetInitialStories();
        repositoryMock.Setup(l => l.GetStoriesFromApi()).ReturnsAsync(GetInitialStories());
        var storyService = new StoryService(repositoryMock.Object);
        var stories = await storyService.GetStories(new NextTech.Application.Story.StoryPaginatedFilter{
            MinTime = DateTime.Parse("2023-05-01"),
            MaxTime = DateTime.Parse("2023-05-02"),
            PageNumber = 1,
            PageSize = 10
        });
        
        //500 / 3 = 166 500 / 3 = 2  the dates 2023-05-01 and 2023-05-02 has 1 more 166 + 1 = 167 
        Assert.Equal(334, stories.TotalCount);
        Assert.Equal(10, stories.Items.Count);
        Assert.Equal(1, stories.PageNumber);
        Assert.Equal(34, stories.TotalPages);
        Assert.Equal(true, stories.HasNextPage);
        Assert.Equal(false, stories.HasPreviousPage);
    }
}