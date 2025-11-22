namespace MyCRM.Domain.Entities;

public class Todo
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public bool IsDone { get; set; }
}