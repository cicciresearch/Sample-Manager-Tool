namespace Cicci.SampleManager.Models;

public class DeviceStack
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public ICollection<StackLayer> Layers { get; set; } = new List<StackLayer>();
    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
}