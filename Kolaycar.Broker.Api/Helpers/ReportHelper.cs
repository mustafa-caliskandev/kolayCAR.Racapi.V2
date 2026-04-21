using KolayCAR.Broker.Domain.Models.StoredPorcedureModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;

namespace KolayCAR.Broker.API.Helpers
{
    public static class ReportHelper
    {
        public static void ExportToExcel(List<ReservationReportModel> data, string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("rapor");
                worksheet.Cells.LoadFromCollection(data, true);

                AddTittles(worksheet);

                for (int row = 2; row <= data.Count + 1; row++)
                {
                    worksheet.Cells[row, 4].Value = data[row - 2].ReservationDate.ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cells[row, 6].Value = data[row - 2].PickupDate.ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cells[row, 7].Value = data[row - 2].ReturnDate.ToString("dd.MM.yyyy HH:mm");
                }

                AddStyleExcel(worksheet, data);
                FormatNumberColumnsAsThousands(worksheet, 13, 15);

                //for (int row = 0; row < data.Count; row++)
                //{
                //    worksheet.Cells[row, 1].Value = data[row - 1].VendorName;
                //    worksheet.Cells[row, 2].Value = data[row - 1].ReservationNumber;
                //}

                FileInfo excelFile = new FileInfo(filePath);
                package.SaveAs(excelFile);
            }
        }

        public static void AddTittles(ExcelWorksheet worksheet)
        {
            worksheet.Cells[1, 1].Value = "Tedarikçi";
            worksheet.Cells[1, 2].Value = "Rezervasyon No";
            worksheet.Cells[1, 3].Value = "Tedarikçi Rezervasyon No";
            worksheet.Cells[1, 4].Value = "Rez Tarihi";
            worksheet.Cells[1, 5].Value = "Yolcu";
            worksheet.Cells[1, 6].Value = "Alış Tarihi";
            worksheet.Cells[1, 7].Value = "Bırakış Tarihi";
            worksheet.Cells[1, 8].Value = "Alış Lokasyonu";
            worksheet.Cells[1, 9].Value = "Bırakış Lokasyonu";
            worksheet.Cells[1, 10].Value = "Tedarikçi Fatura";
            worksheet.Cells[1, 11].Value = "Broker Fatura";
            worksheet.Cells[1, 12].Value = "Rez Durumu";
            worksheet.Cells[1, 13].Value = "Rezervasyon Ücreti";
            worksheet.Cells[1, 14].Value = "Tedarikçi Hakediş";
            worksheet.Cells[1, 15].Value = "Broker Hakediş";
        }

        public static void ChangeFontSize(ExcelWorksheet worksheet, int startRow, int startColumn, int endRow, int endColumn, float fontSize)
        {
            // Belirtilen hücre aralığında yazı boyutunu değiştirin
            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = startColumn; col <= endColumn; col++)
                {
                    var cell = worksheet.Cells[row, col];
                    cell.Style.Font.Size = fontSize; // Yazı boyutunu ayarla
                }
            }
        }

        public static void AddStyleExcel(ExcelWorksheet worksheet, List<ReservationReportModel> data)
        {
            using (var range = worksheet.Cells[1, 1, 1, 15])
            {
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#E6B8AF"));
                range.Style.Font.Color.SetColor(System.Drawing.Color.Black); // Metin rengini beyaz olarak ayarladık
                range.Style.Font.Bold = true; // Kalın metin
                range.Style.WrapText = true; // Metni kaydır
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin; // Alt çizgi ekle
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin; // Alt çizgi ekle
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin; // Alt çizgi ekle
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin; // Alt çizgi ekle
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            using (var range = worksheet.Cells[2, 1, data.Count + 1, 15])
            {
                range.Style.Font.Size = 10f;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin; // Alt çizgi ekle
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin; // Alt çizgi ekle
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin; // Alt çizgi ekle
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin; // Alt çizgi ekle
            }

            worksheet.Cells.AutoFitColumns();
        }

        public static void FormatNumberColumnsAsThousands(ExcelWorksheet worksheet, int startColumn, int endColumn)
        {
            // Belirtilen başlangıç ve bitiş sütunları arasındaki sütunlarda sayı formatını 1000'lik '.' olarak ayarlayın
            for (int col = startColumn; col <= endColumn; col++)
            {
                var column = worksheet.Column(col);
                column.Style.Numberformat.Format = "#,##0.00"; // Sayı formatı olarak 1000'lik '.'
            }
        }
    }

    public class ReporDateHelper
    {
        public DateTime endDate;
        public DateTime beginDate;
        public DateTime today;
        public DateTime now;

        public static ReporDateHelper GetDateWeekly()
        {
            var queryDate = new ReporDateHelper();
            queryDate.today = DateTime.Today;

            while (queryDate.today.DayOfWeek != DayOfWeek.Monday)
            {
                queryDate.today = queryDate.today.AddDays(-1);
            }

            queryDate.endDate = queryDate.today.Date.AddHours(9);
            queryDate.beginDate = queryDate.endDate.AddDays(-7);

            return queryDate;
        }

        public static ReporDateHelper GetDateMonthly()
        {
            var queryDate = new ReporDateHelper();
            queryDate.now = DateTime.Now;
            queryDate.endDate = new DateTime(queryDate.now.Year, queryDate.now.Month, 1);
            queryDate.beginDate = queryDate.endDate.AddMonths(-1);

            return queryDate;
        }
    }
}
