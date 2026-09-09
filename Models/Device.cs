using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Models;

public class Device
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Pixel { get; set; } = "";

    public Guid SampleId { get; set; }

    public Sample Sample { get; set; } = null!;

    [MaxLength(2000)]
    public string Notes { get; set; } = "";

    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
}