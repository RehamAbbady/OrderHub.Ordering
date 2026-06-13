using OrderHub.Web.Pages.Orders;

namespace OrderHub.Web.Services;

public sealed class InMemoryOrderDraftStore : IOrderDraftStore
{
    public Task<OrderDraft?> LoadAsync(int orderId, CancellationToken ct)
    {
        var draft = new OrderDraft(
            SchoolId: 1,
            SchoolName: "St Aldate's Primary",
            ParentEmail: "parent@example.com",
            Lines: new List<DraftLine>
            {
                new(1, "BLZ-NAVY-28", "AB", 24.50m, 1),
                new(2, "TIE-HOUSE-RED", null, 8.00m, 2),
                new(3, "JMP-GREY-30", "ABCD", 18.75m, 1)
            });

        return Task.FromResult<OrderDraft?>(draft);
    }
}