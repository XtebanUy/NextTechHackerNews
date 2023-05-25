using FluentValidation;

namespace NextTech.Application.Story;

public class StoryPaginatedFilterValidator : AbstractValidator<StoryPaginatedFilter>
{
    public StoryPaginatedFilterValidator()
    {
        RuleFor(x => x.MinScore)
            .GreaterThanOrEqualTo(0).WithMessage("MinScore must be greater or equal than 0.");

        RuleFor(x => x.MaxScore)
            .GreaterThanOrEqualTo(0).WithMessage("MinScore must be greater or equal than 0.");

        RuleFor(x => new {x.MaxScore, x.MinScore})
            .Must(x => x.MaxScore.HasValue && x.MinScore.HasValue ? x.MaxScore >= x.MinScore : true).WithMessage(x => "MaxScore must be greater or equal than MinScore");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize at least greater than or equal to 1.");
    }
}
