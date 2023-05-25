using Microsoft.AspNetCore.Mvc;
using NextTech.Application.Common.Models;
using NextTech.Application.Common.Interfaces;
using NextTech.Application.Story;
using FluentValidation;
using FluentValidation.Results;

namespace NextTech.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoryController : ControllerBase
{
    private readonly IStoryService _storyService;
    IValidator<StoryPaginatedFilter> _validator;

    public StoryController(IStoryService storyService, IValidator<StoryPaginatedFilter> validator)
    {
        _storyService = storyService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<StoryDto>>> Get([FromQuery] StoryPaginatedFilter filter)
    {
        ValidationResult result = await _validator.ValidateAsync(filter);

        if (!result.IsValid)
        {
            return BadRequest(result.ToDictionary());
        }
        return Ok(await _storyService.GetStories(filter));
    }
}