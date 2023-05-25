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
    private readonly IFlurlClient _flurlClient;
    private ConcurrentDictionary<int, Story> _cachedStories = new ();
    private List<int> _newStoriesIds = new List<int>();
    private TaskCompletionSource<List<Story>>? _runningQuery = null;
    private readonly SemaphoreSlim _lock= new SemaphoreSlim(1, 1);

   
    public RepositoryService(IFlurlClientFactory flurlClientFac, IOptions<HackerNewsApiOptions> options)
    {
        _options = options.Value;
        _flurlClient = flurlClientFac.Get(_options.ServiceBaseUrl);
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
            var newStoriesIds = await _flurlClient.Request("newstories.json").GetJsonAsync<List<int>>();
            var removedStories = _newStoriesIds.Where(s => !newStoriesIds.Contains(s));
            var newStoriesIdsToCache = newStoriesIds.Where(s => !_newStoriesIds.Contains(s));

            foreach(var s in removedStories)
            {
                _cachedStories.TryRemove(s,  out Story? removedValue);
            }
        
            ParallelOptions paralellOptions = new() {
                MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism
            };

            await Parallel.ForEachAsync(newStoriesIdsToCache, paralellOptions, async (storyId, ct) => {
                var story = await _flurlClient.Request("item", $"{storyId}.json").GetJsonAsync<Story>();
                _cachedStories[storyId] = story;
            });

            var updates = await _flurlClient.Request("updates.json").GetJsonAsync<Updates>();
            var itemsToUpdate = updates.Items.Join(newStoriesIds, updatedItem => updatedItem, newStoryItem => newStoryItem, (updatedItem, newStoryItem) => updatedItem);
            await Parallel.ForEachAsync(itemsToUpdate, paralellOptions, async (storyId, ct) => {
                var story = await _flurlClient.Request("item", $"{storyId}.json").GetJsonAsync<Story>();
                _cachedStories[storyId] = story;
            });

            _newStoriesIds = newStoriesIds;

            await _lock.WaitAsync();
            try
            {
                _runningQuery.SetResult(_cachedStories.Values.OrderBy(s => s.Id).ToList());
                _runningQuery = null;
            }
            finally
            {
                _lock.Release();
            }
        }
        
        return await result;

    }
}