namespace OrderHub.Web.Pages.Orders
{
    public interface IOrderDraftStore
    {
        Task<OrderDraft?> LoadAsync(int orderId, CancellationToken ct);
    }
    public sealed record OrderDraft(int SchoolId, string SchoolName, string ParentEmail,
                                    IReadOnlyList<DraftLine> Lines);
    public sealed record DraftLine(int Id, string Sku, string? Embroidery, decimal UnitPrice, int Quantity);
}
