using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Models;
using System;


public class InvoiceController : Controller
{
    private readonly DB _db;
    private readonly InvoiceService _invoiceService;

    public InvoiceController(DB db, InvoiceService invoiceService)
    {
        _db = db;
        _invoiceService = invoiceService;
    }

    [HttpGet("/invoice/{paymentId}")]
    public IActionResult Download(int paymentId)
    {
        // check user
        var userId = User.GetUserId();
        var user = _db.Users.Find(userId);
        if(user == null)
            return NotFound("This payment is not belongs to you");

        var payment = _db.Payments
            .Include(p => p.User)
            .Include(p => p.Purchases)
                .ThenInclude(pu => pu.Game)
            .FirstOrDefault(p => p.Id == paymentId && p.Status == PaymentStatus.Completed && p.UserId == userId);

        if (payment == null)
            return NotFound();

        var invoice = new InvoiceDto
        {
            InvoiceNumber = $"INV-{payment.CreatedAt:yyyyMMdd}-{payment.Id}",
            IssuedAt = payment.CreatedAt,

            CustomerName = payment.User.Username,
            CustomerEmail = payment.User.Email,

            Items = payment.Purchases.Select((p, index) => new InvoiceItemDto
            {
                No = index + 1,
                ItemName = p.Game.Title,
                Price = p.Game.Price,
                Discount = p.Game.Price - p.Game.DiscountPrice ?? 0
            }).ToList()
        };

        invoice.Subtotal = invoice.Items.Sum(i => i.Price);
        invoice.Discount = invoice.Items.Sum(i => i.Discount);
        invoice.Total = invoice.Subtotal - invoice.Discount;

        var pdf = _invoiceService.GetInvoice(invoice);

        using var stream = new MemoryStream();
        pdf.Save(stream);

        //return File(
        //    stream.ToArray(),
        //    "application/pdf",
        //    $"{invoice.InvoiceNumber}.pdf"
        //);

        return File(
            stream.ToArray(),
            "application/pdf"
        );
    }
}
