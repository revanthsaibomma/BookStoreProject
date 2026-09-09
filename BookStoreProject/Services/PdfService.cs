using BookStoreProject.DAL;
using BookStoreProject.ViewModels;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection.Metadata;

namespace BookStoreProject.Services
{
    public class PdfService : IPdfService
    {
        private readonly ApplicationDbContext _context;

        public PdfService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateInvoice(int orderId)
        {
            var order = await _context.Orders
                .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Book)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
                throw new Exception("Order not found.");

            var invoice = new InvoiceVM
            {
                OrderId = order.Id,
                CustomerName = order.Name,
                Address = order.Address,
                PhoneNumber = order.PhoneNumber,
                OrderDate = order.CreateDate,
                TotalAmount = (decimal)order.OrderDetails.Sum(x => x.Quantity * x.UnitPrice),

                Items = order.OrderDetails.Select(x => new InvoiceItemVM
                {
                    BookName = x.Book.BookName,
                    Quantity = x.Quantity,
                    Price = (decimal)x.UnitPrice
                }).ToList()
            };

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Column(column =>
                    {
                        column.Item().Text("BOOK STORE")
                            .FontSize(24)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        column.Item().Text($"Invoice #{invoice.OrderId}")
                            .FontSize(16);

                        column.Item().Text($"Date: {invoice.OrderDate:dd MMM yyyy}");
                    });

                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        // Customer Information
                        column.Item().Text("Customer Information")
                            .Bold()
                            .FontSize(16);

                        column.Item().Text($"Name: {invoice.CustomerName}");
                        column.Item().Text($"Address: {invoice.Address}");
                        column.Item().Text($"Phone: {invoice.PhoneNumber}");

                        column.Item().PaddingVertical(15);

                        // Table Header
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Book").Bold();
                                header.Cell().Text("Qty").Bold();
                                header.Cell().Text("Price").Bold();
                                header.Cell().Text("Total").Bold();
                            });

                            foreach (var item in invoice.Items)
                            {
                                table.Cell().Text(item.BookName);

                                table.Cell().Text(item.Quantity.ToString());

                                table.Cell().Text($"${item.Price}");

                                table.Cell().Text($"${item.Price * item.Quantity}");
                            }
                        });

                        column.Item().PaddingTop(20);

                        column.Item().AlignRight().Text(text =>
                        {
                            text.Span("Grand Total: ").Bold();

                            text.Span($"${invoice.TotalAmount}");
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Thank you for shopping with Book Store! ❤")
                            .FontSize(12);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}