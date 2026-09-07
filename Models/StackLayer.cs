using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Models;

public class StackLayer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeviceStackId { get; set; }
    public DeviceStack DeviceStack { get; set; } = null!;

    public int Position { get; set; }

    [Required]
    [MaxLength(200)]
    public string Material { get; set; } = "";
}