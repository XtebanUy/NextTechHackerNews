using System.Collections.Concurrent;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Options;
using NextTech.Application.Interfaces;
using NextTech.Application.Common.Models;
using NextTech.Domain.Entities;
using NextTech.Infrastructure.Configuration;
using NextTech.Infrastructure.Dtos;

namespace NextTech.Infrastructure.Services;

public class RepositoryService : IRepositoryService
{
    private readonly HackerNewsApiOptions _options;
    private readonly IFlurlClientFactory _flurlClientFac;
    private ConcurrentDictionary<int, Story> _cachedStories = new ();
    private List<int> _newStoriesIds = new List<int>();
    private TaskCompletionSource<List<Story>>? _runningQuery = null;
    private readonly SemaphoreSlim _lock= new SemaphoreSlim(1, 1);

   
    public RepositoryService(IFlurlClientFactory flurlClientFac, IOptions<HackerNewsApiOptions> options)
    {
        _options = options.Value;
        _flurlClientFac = flurlClientFac;
    }



    public async Task<IList<Story>> GetStoriesFromApi()
    {
        Task<List<Story>>? result = null;
        bool newRun = false;
        await _lock.WaitAsync();
        
        try
        {
            if (_runningQuery == null)
            {
                newRun = true;
                _runningQuery = new();
            }

            result = _runningQuery.Task;
        }
        finally
        {
            _lock.Release();
        }

        if (newRun)
        {
            var lockAquired = false;

            try
            {
                var newStoriesIds = await _flurlClientFac.Get(_options.ServiceBaseUrl).Request("newstories.json").GetJsonAsync<List<int>>();
                RemoveStories(newStoriesIds);

                ParallelOptions paralellOptions = new()
                {
                    MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism
                };

                await GetNewStories(newStoriesIds, paralellOptions);
                await Updatestories(newStoriesIds, paralellOptions);
                _newStoriesIds = newStoriesIds;
                await _lock.WaitAsync();
                lockAquired = true;
                _runningQuery.SetResult(_cachedStories.Values.OrderBy(s => s.Id).ToList());
            }
            catch (Exception e){
                _runningQuery.SetException(e);
            }
            finally
            {
                if (lockAquired)
                {
                    _runningQuery = null;
                    _lock.Release();
                }
            }
        }
        
        return await result;
    }

    private void RemoveStories(List<int> newStoriesIds)
    {
        var removedStories = _newStoriesIds.Where(s => !newStoriesIds.Contains(s));

        foreach (var s in removedStories)
        {
            _cachedStories.TryRemove(s, out Story? removedValue);
        }
    }

    private async Task Updatestories(List<int> newStoriesIds, ParallelOptions paralellOptions)
    {
        var updates = await _flurlClientFac.Get(_options.ServiceBaseUrl).Request("updates.json").GetJsonAsync<Updates>();
        var itemsToUpdate = updates.Items.Join(newStoriesIds, updatedItem => updatedItem, newStoryItem => newStoryItem, (updatedItem, newStoryItem) => updatedItem);
        await Parallel.ForEachAsync(itemsToUpdate, paralellOptions, async (storyId, ct) =>
        {
            var story = await _flurlClientFac.Get(_options.ServiceBaseUrl).Request("item", $"{storyId}.json").GetJsonAsync<Story>();
            if (story != null && story.Type == "story")
            {
                _cachedStories[storyId] = story;
            }
            else
            {
                _cachedStories.TryRemove(storyId, out Story? removedtory);
            } 
        });
    }

    private async Task GetNewStories(List<int> newStoriesIds, ParallelOptions paralellOptions)
    {
        var newStoriesIdsToCache = newStoriesIds.Where(s => !_newStoriesIds.Contains(s));
        await Parallel.ForEachAsync(newStoriesIdsToCache, paralellOptions, async (storyId, ct) =>
        {
            var story = await _flurlClientFac.Get(_options.ServiceBaseUrl).Request("item", $"{storyId}.json").GetJsonAsync<Story>();
            if (story != null && story.Type == "story")
            {
                _cachedStories[storyId] = story;
            }
            else
            {
                _cachedStories.TryRemove(storyId, out Story? removedtory);
            }
        });
    }
}