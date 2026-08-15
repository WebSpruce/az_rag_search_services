using az_rag_search_services.Domain.Entities;

namespace az_rag_search_services.Application.Common.Interfaces;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(string id);
    Task AddAsync(Note note);
    Task<IEnumerable<Note>> SearchByVectorAsync(float[] embedding, int limit);
}