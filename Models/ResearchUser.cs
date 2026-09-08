using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Models;

public class ResearchUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    public bool IsEnabled { get; set; } = true;

    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
}