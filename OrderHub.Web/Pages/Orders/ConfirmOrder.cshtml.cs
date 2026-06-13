using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderHub.Ordering.Orders;

namespace OrderHub.Web.Pages.Orders;

public sealed class ConfirmOrderModel : PageModel
{
    private readonly IOrderDraftStore _drafts;   
    private readonly IOrderProcessor _processor; 

    public ConfirmOrderModel(IOrderDraftStore drafts, IOrderProcessor processor)
    {
        _drafts = drafts;
        _processor = processor;
    }

    public string SchoolName { get; private set; } = "";

    [BindProperty] public List<LineInput> Lines { get; set; } = new();

    public IReadOnlyDictionary<int, DraftLine> LineViews { get; private set; }
        = new Dictionary<int, DraftLine>();

    public decimal Subtotal => Lines
        .Where(l => LineViews.ContainsKey(l.Id))
        .Sum(l => LineViews[l.Id].UnitPrice * l.Quantity);

    public async Task<IActionResult> OnGetAsync(int orderId, CancellationToken ct)
    {
        var draft = await _drafts.LoadAsync(orderId, ct);
        if (draft is null) return NotFound();

        SchoolName = draft.SchoolName;
        LineViews = draft.Lines.ToDictionary(l => l.Id);
        Lines = draft.Lines.Select(l => new LineInput(l.Id, l.Quantity)).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int orderId, CancellationToken ct)
    {
        var draft = await _drafts.LoadAsync(orderId, ct);
        if (draft is null) return NotFound();

        SchoolName = draft.SchoolName;
        LineViews = draft.Lines.ToDictionary(l => l.Id);


        for (var i = 0; i < Lines.Count; i++)
        {
            if (!LineViews.ContainsKey(Lines[i].Id))
                ModelState.AddModelError("", "An order line is no longer valid; reload the page.");
            else if (Lines[i].Quantity <= 0)
                ModelState.AddModelError($"Lines[{i}].Quantity", "Quantity must be at least 1.");
        }
        if (!ModelState.IsValid) return Page();

        var request = new OrderRequest(
            draft.SchoolId,
            Lines.Select(l => new OrderLine(LineViews[l.Id].Sku, l.Quantity, LineViews[l.Id].Embroidery))
                 .ToList(),
            draft.ParentEmail);

        OrderResult result;
        try
        {
            result = await _processor.ProcessAsync(request, ct);
        }
        catch
        {
            ModelState.AddModelError("", "Order processing is not available in this demo. The page and live subtotal run without a backend.");
            return Page();
        }

        if (result.Succeeded)
            return RedirectToPage("Confirmed", new { orderId });

        ModelState.AddModelError("", result.Reason switch
        {
            OrderFailureReason.OutOfStock => $"Out of stock: {result.Detail}.",
            OrderFailureReason.PaymentDeclined => "Payment was declined. No charge was made.",
            OrderFailureReason.ProductNotFound => $"Product not found: {result.Detail}.",
            _ => "We couldn't confirm this order. Please try again."
        });
        return Page();
    }

    public sealed record LineInput(int Id, int Quantity)
    {
        public LineInput() : this(0, 0) { }   
    }
}

