namespace az_rag_search_services.Domain.Entities;

public class Note
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Content { get; set; }
    public float[] VectorEmbedding { get; set; } = Array.Empty<float>();
    
    public Note(string content)
    {
        Content = content;
    }
}