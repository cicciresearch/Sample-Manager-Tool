using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Api;

public class CreateUserRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";
}