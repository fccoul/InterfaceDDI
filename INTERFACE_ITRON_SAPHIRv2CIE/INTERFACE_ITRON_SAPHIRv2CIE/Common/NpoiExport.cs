using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.HSSF.Util;
using NPOI.POIFS.FileSystem;
using NPOI.HPSF;
using System.Drawing;

public class NpoiExport : IDisposable
{
    const int MaximumNumberOfRowsPerSheet = 65500;
    const int MaximumSheetNameLength = 25;
    //protected Workbook Workbook { get; set; }
    public Workbook Workbook { get; set; }

    public NpoiExport()
    {
        this.Workbook = new HSSFWorkbook();
    }

    protected string EscapeSheetName(string sheetName)
    {
        var escapedSheetName = sheetName
                                    .Replace("/", "-")
                                    .Replace("\\", " ")
                                    .Replace("?", string.Empty)
                                    .Replace("*", string.Empty)
                                    .Replace("[", string.Empty)
                                    .Replace("]", string.Empty)
                                    .Replace(":", string.Empty);

        if (escapedSheetName.Length > MaximumSheetNameLength)
            escapedSheetName = escapedSheetName.Substring(0, MaximumSheetNameLength);

        return escapedSheetName;
    }

    protected Sheet CreateExportDataTableSheetAndHeaderRow(DataTable exportData, string sheetName, CellStyle headerRowStyle)
    {
        var sheet = this.Workbook.CreateSheet(EscapeSheetName(sheetName));

        // Create the header row
        var row = sheet.CreateRow(0);

        for (var colIndex = 0; colIndex < exportData.Columns.Count; colIndex++)
        {
            var cell = row.CreateCell(colIndex);
            cell.SetCellValue(exportData.Columns[colIndex].ColumnName);

            if (headerRowStyle != null)
                cell.CellStyle = headerRowStyle;
        }

        return sheet;
    }

    public void ExportDataTableToWorkbook(DataTable exportData, string sheetName)
    {
        // Create the header row cell style
        var headerLabelCellStyle = this.Workbook.CreateCellStyle();
        headerLabelCellStyle.BorderBottom = CellBorderType.THIN;
        var headerLabelFont = this.Workbook.CreateFont();
        headerLabelFont.Boldweight = (short)FontBoldWeight.BOLD;
        headerLabelCellStyle.SetFont(headerLabelFont);

        //headerLabelFont.Color = (short)FontColor.RED;
        headerLabelFont.Color = (short) Color.Orange.ToArgb();
        headerLabelCellStyle.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.GREY_50_PERCENT.COLOR_NORMAL;

        var sheet = CreateExportDataTableSheetAndHeaderRow(exportData, sheetName, headerLabelCellStyle);
        var currentNPOIRowIndex = 1;
        var sheetCount = 1;

        for (var rowIndex = 0; rowIndex < exportData.Rows.Count; rowIndex++)
        {
            if (currentNPOIRowIndex >= MaximumNumberOfRowsPerSheet)
            {
                sheetCount++;
                currentNPOIRowIndex = 1;

                sheet = CreateExportDataTableSheetAndHeaderRow(exportData,
                                                               sheetName + " - " + sheetCount,
                                                               headerLabelCellStyle);
            }

            var row = sheet.CreateRow(currentNPOIRowIndex++);

            for (var colIndex = 0; colIndex < exportData.Columns.Count; colIndex++)
            {
                var cell = row.CreateCell(colIndex);
                cell.SetCellValue(exportData.Rows[rowIndex][colIndex].ToString());
            }
        }
    }

    public byte[] GetBytes()
    {
        using (var buffer = new MemoryStream())
        {
            this.Workbook.Write(buffer);
            return buffer.GetBuffer();
        }
    }

    public void Dispose()
    {
        if (this.Workbook != null)
            this.Workbook.Dispose();
    }
}