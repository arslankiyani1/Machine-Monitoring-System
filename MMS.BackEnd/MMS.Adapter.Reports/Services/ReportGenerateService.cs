using CsvHelper;
using CsvHelper.Configuration;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using MMS.Application.Enum;
using MMS.Application.Ports.In.CustomerReportSetting;
using MMS.Application.Utils;
using System.Globalization;

public class ReportGenerateService : IReportGenerateService
{
    public async Task<byte[]> GenerateAsync(string format, Dictionary<Guid, ReportData> reportDatas, IEnumerable<ReportType> reportTypes, string reportName)
    {
        IEnumerable<string> reportTypeStrings = reportTypes.Select(r => r.ToString());
        return format.ToUpper() switch
        {
            "PDF" => await GeneratePdfAsync(reportDatas, reportTypeStrings, reportName),
            "CSV" => await GenerateCsvAsync(reportDatas, reportTypeStrings, reportName),
            _ => throw new ArgumentException($"Unsupported report format: {format}")
        };
    }

    private async Task<byte[]> GeneratePdfAsync(Dictionary<Guid, ReportData> reportDatas, IEnumerable<string> reportTypes, string reportName)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new PdfWriter(memoryStream);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        // Modern color palette
        var primaryColor = new iText.Kernel.Colors.DeviceRgb(102, 126, 234);
        var accentColor = new iText.Kernel.Colors.DeviceRgb(118, 75, 162);
        var lightGray = new iText.Kernel.Colors.DeviceRgb(248, 250, 252);
        var darkGray = new iText.Kernel.Colors.DeviceRgb(30, 41, 59);
        var headerBg = new iText.Kernel.Colors.DeviceRgb(241, 245, 249);
        var successColor = new iText.Kernel.Colors.DeviceRgb(34, 197, 94);
        var warningColor = new iText.Kernel.Colors.DeviceRgb(234, 179, 8);
        var errorColor = new iText.Kernel.Colors.DeviceRgb(239, 68, 68);
        var borderColor = new iText.Kernel.Colors.DeviceRgb(226, 232, 240);

        document.SetMargins(40, 40, 40, 40);

        // Decorative header bar
        var headerBar = new Table(UnitValue.CreatePercentArray(1)).UseAllAvailableWidth();
        headerBar.AddCell(new Cell()
            .Add(new Paragraph(""))
            .SetHeight(8)
            .SetBackgroundColor(primaryColor)
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
        document.Add(headerBar);

        // Large title
        document.Add(new Paragraph(reportName)
            .SetFontSize(32)
            .SetFontColor(darkGray)
            .SetMarginTop(20)
            .SetMarginBottom(5));

        // Subtitle
        document.Add(new Paragraph("Performance Analysis Report")
            .SetFontSize(14)
            .SetFontColor(new iText.Kernel.Colors.DeviceRgb(100, 116, 139))
            .SetMarginBottom(5));

        // Generation timestamp
        document.Add(new Paragraph($"Generated: {DateTime.UtcNow:MMMM dd, yyyy 'at' HH:mm} UTC")
            .SetFontSize(11)
            .SetFontColor(new iText.Kernel.Colors.DeviceRgb(148, 163, 184))
            .SetMarginBottom(25));

        // Separator line
        var separator = new Table(UnitValue.CreatePercentArray(1)).UseAllAvailableWidth();
        separator.AddCell(new Cell()
            .Add(new Paragraph(""))
            .SetHeight(1)
            .SetBackgroundColor(borderColor)
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
        document.Add(separator.SetMarginBottom(25));

        int machineCount = 0;
        foreach (var (machineId, data) in reportDatas)
        {
            machineCount++;

            if (machineCount > 1)
            {
                document.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
            }

            // Machine header
            document.Add(new Paragraph($"Machine #{machineCount}")
                .SetFontSize(22)
                .SetFontColor(primaryColor)
                .SetMarginBottom(15));

            // Machine ID badge
            document.Add(new Paragraph($"ID: {machineId}")
                .SetFontSize(10)
                .SetFontColor(new iText.Kernel.Colors.DeviceRgb(100, 116, 139))
                .SetBackgroundColor(lightGray)
                .SetPadding(6)
                .SetMarginBottom(20)
                .SetBorderRadius(new iText.Layout.Properties.BorderRadius(4)));

            // Section label
            document.Add(new Paragraph("Machine Details")
                .SetFontSize(16)
                .SetFontColor(darkGray)
                .SetMarginBottom(10));

            // Machine details table
            var machineDetailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 35, 65 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(25);

            // Styled header cells
            machineDetailsTable.AddHeaderCell(new Cell()
                .Add(new Paragraph("Detail").SetFontSize(11).SetFontColor(darkGray))
                .SetBackgroundColor(headerBg)
                .SetPadding(12)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBorderBottom(new iText.Layout.Borders.SolidBorder(borderColor, 1)));

            machineDetailsTable.AddHeaderCell(new Cell()
                .Add(new Paragraph("Value").SetFontSize(11).SetFontColor(darkGray))
                .SetBackgroundColor(headerBg)
                .SetPadding(12)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBorderBottom(new iText.Layout.Borders.SolidBorder(borderColor, 1)));

            // Detail rows with alternating colors
            string[] details = { "Serial Number", "Machine Name", "Machine Model" };
            for (int i = 0; i < details.Length; i++)
            {
                var bgColor = i % 2 == 0 ? iText.Kernel.Colors.ColorConstants.WHITE : lightGray;
                var key = details[i].Replace(" ", "");
                var value = data.MachineDetails.ContainsKey(key) ? data.MachineDetails[key] : "N/A";

                machineDetailsTable.AddCell(new Cell()
                    .Add(new Paragraph(details[i]).SetFontSize(10))
                    .SetBackgroundColor(bgColor)
                    .SetPadding(10)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                machineDetailsTable.AddCell(new Cell()
                    .Add(new Paragraph(value).SetFontSize(10))
                    .SetBackgroundColor(bgColor)
                    .SetPadding(10)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            }
            document.Add(machineDetailsTable);

            // Summary section
            document.Add(new Paragraph("Performance Summary")
                .SetFontSize(16)
                .SetFontColor(darkGray)
                .SetMarginTop(10)
                .SetMarginBottom(10));

            var summaryTable = new Table(UnitValue.CreatePercentArray(Enumerable.Repeat(1f, reportTypes.Count() + 1).ToArray()))
                .UseAllAvailableWidth()
                .SetMarginBottom(25);

            // Colored header
            summaryTable.AddHeaderCell(new Cell()
                .Add(new Paragraph("Metric").SetFontSize(11).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                .SetBackgroundColor(primaryColor)
                .SetPadding(12)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            foreach (var type in reportTypes)
            {
                summaryTable.AddHeaderCell(new Cell()
                    .Add(new Paragraph(type).SetFontSize(11).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                    .SetBackgroundColor(primaryColor)
                    .SetPadding(12)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            }

            // Summary row with color-coded values
            summaryTable.AddCell(new Cell()
                .Add(new Paragraph("Average").SetFontSize(10))
                .SetBackgroundColor(lightGray)
                .SetPadding(10)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            foreach (var type in reportTypes)
            {
                var value = data.Summary.ContainsKey(type) ? data.Summary[type] : 0;
                var valueColor = value >= 80 ? successColor : value >= 50 ? warningColor : errorColor;

                summaryTable.AddCell(new Cell()
                    .Add(new Paragraph(value.ToString("F2")).SetFontSize(11).SetFontColor(valueColor))
                    .SetBackgroundColor(lightGray)
                    .SetPadding(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            }
            document.Add(summaryTable);

            // Daily performance section
            document.Add(new Paragraph("Daily Performance Breakdown")
                .SetFontSize(16)
                .SetFontColor(darkGray)
                .SetMarginTop(10)
                .SetMarginBottom(10));

            var dailyTable = new Table(UnitValue.CreatePercentArray(Enumerable.Repeat(1f, reportTypes.Count() + 1).ToArray()))
                .UseAllAvailableWidth();

            // Header with accent color
            dailyTable.AddHeaderCell(new Cell()
                .Add(new Paragraph("Date").SetFontSize(11).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                .SetBackgroundColor(accentColor)
                .SetPadding(12)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            foreach (var type in reportTypes)
            {
                dailyTable.AddHeaderCell(new Cell()
                    .Add(new Paragraph(type).SetFontSize(11).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                    .SetBackgroundColor(accentColor)
                    .SetPadding(12)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            }

            // Data rows with alternating colors
            int rowIndex = 0;
            foreach (var row in data.DailyRows)
            {
                var rowBg = rowIndex % 2 == 0 ? iText.Kernel.Colors.ColorConstants.WHITE : lightGray;

                dailyTable.AddCell(new Cell()
                    .Add(new Paragraph(((DateTime)row["Date"]).ToString("MMM dd, yyyy")).SetFontSize(10))
                    .SetBackgroundColor(rowBg)
                    .SetPadding(10)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                foreach (var type in reportTypes)
                {
                    var value = row.ContainsKey(type) ? Convert.ToDouble(row[type]).ToString("F2") : "N/A";
                    dailyTable.AddCell(new Cell()
                        .Add(new Paragraph(value).SetFontSize(10))
                        .SetBackgroundColor(rowBg)
                        .SetPadding(10)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                }
                rowIndex++;
            }
            document.Add(dailyTable);
        }

        // Professional footer
        document.Add(new Paragraph($"End of Report • {machineCount} Machine(s) Analyzed • MMS - Machine Monitoring System")
            .SetFontSize(9)
            .SetFontColor(new iText.Kernel.Colors.DeviceRgb(148, 163, 184))
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginTop(40));

        document.Close();
        return memoryStream.ToArray();
    }

    private async Task<byte[]> GenerateCsvAsync(Dictionary<Guid, ReportData> reportDatas, IEnumerable<string> reportTypes, string reportName)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        csv.WriteField($"Report: {reportName}");
        csv.NextRecord();
        foreach (var (machineId, data) in reportDatas)
        {
            csv.WriteField($"Machine ID: {machineId}");
            csv.NextRecord();

            csv.WriteField("Machine Details");
            csv.NextRecord();
            csv.WriteField("Detail");
            csv.WriteField("Value");
            csv.NextRecord();
            csv.WriteField("Serial Number");
            csv.WriteField(data.MachineDetails.ContainsKey("SerialNumber") ? data.MachineDetails["SerialNumber"] : "N/A");
            csv.NextRecord();
            csv.WriteField("Machine Name");
            csv.WriteField(data.MachineDetails.ContainsKey("MachineName") ? data.MachineDetails["MachineName"] : "N/A");
            csv.NextRecord();
            csv.WriteField("Machine Model");
            csv.WriteField(data.MachineDetails.ContainsKey("MachineModel") ? data.MachineDetails["MachineModel"] : "N/A");
            csv.NextRecord();

            csv.WriteField("Summary");
            foreach (var type in reportTypes)
                csv.WriteField(type);
            csv.NextRecord();
            csv.WriteField("");
            foreach (var type in reportTypes)
                csv.WriteField(data.Summary.ContainsKey(type) ? data.Summary[type].ToString("F2") : "N/A");
            csv.NextRecord();

            csv.WriteField("Daily Data");
            csv.NextRecord();
            csv.WriteField("Date");
            foreach (var type in reportTypes)
                csv.WriteField(type);
            csv.NextRecord();
            foreach (var row in data.DailyRows)
            {
                csv.WriteField(((DateTime)row["Date"]).ToString("yyyy-MM-dd"));
                foreach (var type in reportTypes)
                    csv.WriteField(row.ContainsKey(type) ? Convert.ToDouble(row[type]).ToString("F2") : "N/A");
                csv.NextRecord();
            }
            csv.NextRecord();
        }

        await writer.FlushAsync();
        return memoryStream.ToArray();
    }
}