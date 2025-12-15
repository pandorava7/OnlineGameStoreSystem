using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Snippets.Font;

public class InvoiceService
{
    public PdfDocument GetInvoice(InvoiceDto invoice)
    {
        var document = new Document();

        BuildDocument(document, invoice);



        var pdfRenderer = new PdfDocumentRenderer();
        pdfRenderer.Document = document;

        pdfRenderer.RenderDocument();

        return pdfRenderer.PdfDocument;
    }

    private void BuildDocument(Document document, InvoiceDto invoice)
    {
        // 只有加这个才能正常运作，迫不得已的选择
        if (Capabilities.Build.IsCoreBuild)
            GlobalFontSettings.FontResolver = new FailsafeFontResolver();

        var section = document.AddSection();

        var title = section.AddParagraph("INVOICE");
        title.Format.Font.Size = 18;
        title.Format.Font.Bold = true;
        title.Format.SpaceAfter = 10;

        var paragraph = section.AddParagraph();
        paragraph.AddText("Online Game Store");
        paragraph.AddLineBreak();
        paragraph.AddText("Website: www.onlinegamestore.com");
        paragraph.AddLineBreak();
        paragraph.AddText("Email: contact@onlinegamestore.com");
        paragraph.AddLineBreak();
        paragraph.AddText("Phone: 012-34567890");
        paragraph.Format.SpaceAfter = 20;

        paragraph = section.AddParagraph();
        paragraph.AddText("BILLED TO");
        paragraph.AddLineBreak();
        paragraph.AddText(invoice.CustomerName);
        paragraph.AddLineBreak();
        paragraph.AddText($"Email: {invoice.CustomerEmail}");
        paragraph.Format.SpaceAfter = 20;

        paragraph = section.AddParagraph();
        paragraph.AddText($"Invoice No: {invoice.InvoiceNumber}");
        paragraph.AddLineBreak();
        paragraph.AddText(invoice.IssuedAt.ToString("yyyy/MM/dd HH:mm:ss"));
        paragraph.AddLineBreak();
        paragraph.Format.SpaceAfter = 20;

        var table = document.LastSection.AddTable();
        table.Borders.Width = 0.5;

        table.AddColumn("1.2cm"); // No
        table.AddColumn("7.5cm"); // Item
        table.AddColumn("2cm");   // Qty
        table.AddColumn("3cm");   // Unit Price
        table.AddColumn("3cm");   // Total


        Row row = table.AddRow();
        row.HeadingFormat = true;
        row.Format.Font.Bold = true;
        row.Cells[0].AddParagraph("No");
        row.Cells[1].AddParagraph("Items");
        row.Cells[2].AddParagraph("Price");
        row.Cells[3].AddParagraph("Discount");
        row.Cells[4].AddParagraph("TOTAL");
        row.Cells[2].Format.Alignment = ParagraphAlignment.Center;
        row.Cells[3].Format.Alignment = ParagraphAlignment.Right;
        row.Cells[4].Format.Alignment = ParagraphAlignment.Right;

        foreach (var item in invoice.Items)
        {
            row = table.AddRow();
            row.Cells[0].AddParagraph(item.No.ToString());
            row.Cells[1].AddParagraph(item.ItemName);
            row.Cells[2].AddParagraph($"RM {item.Price:N2}");
            row.Cells[3].AddParagraph($"RM {item.Discount:N2}");
            row.Cells[4].AddParagraph($"RM {item.Total:N2}");

            row.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            row.Cells[3].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[4].Format.Alignment = ParagraphAlignment.Right;
        }

        paragraph = section.AddParagraph();
        paragraph.Format.Alignment = ParagraphAlignment.Right;
        paragraph.AddText($"Subtotal: RM {invoice.Subtotal:N2}");
        paragraph.AddLineBreak();
        paragraph.AddText($"Discount: RM {invoice.Discount:N2}");
        paragraph.AddLineBreak();
        paragraph.Format.Font.Bold = true;
        paragraph.AddText($"Total: RM {invoice.Total:N2}");
        paragraph.Format.SpaceBefore = 10;

        paragraph = section.Footers.Primary.AddParagraph();
        paragraph.AddText("Online Game Store Street 42 - 56789 New York - USA");
        paragraph.Format.Alignment = ParagraphAlignment.Center;
    }
}