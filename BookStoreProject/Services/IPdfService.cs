namespace BookStoreProject.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateInvoice(int orderId);
    }
}