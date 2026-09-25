using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Models;

//Use this class when additional measurement files are present in a measurement
public class MeasurementArtifact
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MeasurementId { get; set; }
    public Measurement Measurement { get; set; } = null!;

    // Identifies the meaning of this additional file.
    // The valid values are defined by the individual measurement type.
    [Required]
    [MaxLength(100)]
    public string Kind { get; set; } = "";

    [Required]
    [MaxLength(2000)]
    public string Path { get; set; } = "";
}