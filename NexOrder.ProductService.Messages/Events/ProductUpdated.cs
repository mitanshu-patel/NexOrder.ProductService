namespace NexOrder.ProductService.Messages.Events
{
    public record ProductUpdated(int Id, string Name, string Description, decimal Price);
}
