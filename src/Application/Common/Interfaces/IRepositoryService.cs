using NextTech.Application.Common.Models;
using NextTech.Domain.Entities;

namespace NextTech.Application.Interfaces;

public interface IRepositoryService
{
    Task<IList<NextTech.Domain.Entities.Story>> GetStoriesFromApi(); 
}