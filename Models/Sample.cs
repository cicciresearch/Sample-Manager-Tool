using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Models;

public class Sample
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = "";

    public Guid BatchId { get; set; }

    public Batch Batch { get; set; } = null!;

    [MaxLength(2000)]
    public string Notes { get; set; } = "";

    public ICollection<Device> Devices { get; set; } = new List<Device>();
}