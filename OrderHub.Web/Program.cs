using OrderHub.Ordering.Confirmations;
using OrderHub.Ordering.Infrastructure;
using OrderHub.Web.Pages.Orders;
using OrderHub.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IOrderDraftStore, InMemoryOrderDraftStore>();
builder.Services.AddSingleton<IConfirmationQueue, LoggingConfirmationQueue>(); 
builder.Services.AddOrderProcessing(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("OrderHub")!;
    options.PaymentBaseUrl = builder.Configuration["Payments:BaseUrl"]!;
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapGet("/", () => Results.Redirect("/Orders/ConfirmOrder/1"));

app.Run();