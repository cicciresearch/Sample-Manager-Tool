using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Api;

public class CreateMeasurementRequest<TSpecific>
    where TSpecific : class
{
    [Required]
    public CommonMeasurementRequest Common { get; set; } = null!;

    [Required]
    public TSpecific Specific { get; set; } = null!;
}