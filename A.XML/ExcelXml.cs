using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace A.XML
{
    public static partial class ExcelXml
    {
        public static List<ExcelData<T>> ReadSpreadSheet<T>(Action<CustomExcel> action)
        {
            CustomExcel excel = new CustomExcel();
            action?.Invoke(excel);
            return ReadSpreadSheet<T>(excel) as List<ExcelData<T>>;
        }
        public static DataTable ReadSpreadSheet(Action<CustomExcel> action)
        {
            CustomExcel excel = new CustomExcel();
            action?.Invoke(excel);
            var dataTable = ReadSpreadSheet<DataTable>(excel) as DataTable;
            return dataTable;
        }
        public static DataSet ReadSpreadSheetAll(Action<CustomExcel> action)
        {
            CustomExcel excel = new CustomExcel();
            action?.Invoke(excel);
            var dataTable = ReadSpreadSheetExcelAll(excel);
            return dataTable;
        }
        internal static object ReadSpreadSheet<T>(CustomExcel excel)
        {
            if (string.IsNullOrEmpty(excel.Path) && excel.Stream == null) throw new Exception("path or stream not defined");

            using (SpreadsheetDocument doc = excel.Stream != null ? SpreadsheetDocument.Open(excel.Stream, false) : SpreadsheetDocument.Open(excel.Path, false))
            {
                try
                {
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheet theSheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                    if (!string.IsNullOrEmpty(excel.SheetName))
                    {
                        theSheet = workbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == excel.SheetName).FirstOrDefault();
                    }
                    //WorkbookPart wbPart = doc.WorkbookPart;
                    //var workSheet = wbPart.WorksheetParts.FirstOrDefault();

                    //foreach (ImagePart i in workSheet.DrawingsPart.ImageParts)
                    //{
                    //    var rId = workSheet.DrawingsPart.GetIdOfPart(i);

                    //    Stream stream = i.GetStream();
                    //    long length = stream.Length;
                    //    byte[] byteStream = new byte[length];
                    //    stream.Read(byteStream, 0, (int)length);

                    //    var imageAsString = Convert.ToBase64String(byteStream);
                    //    Console.WriteLine("The rId of this Image is '{0}'", rId);
                    //}
                    //var workSheet = workbookPart.WorksheetParts.First();
                    //foreach (ImagePart i in workSheet.DrawingsPart.ImageParts)
                    //{
                    //    var rId = workSheet.DrawingsPart.GetIdOfPart(i);

                    //    Stream stream = i.GetStream();
                    //    long length = stream.Length;
                    //    byte[] byteStream = new byte[length];
                    //    stream.Read(byteStream, 0, (int)length);

                    //    var imageAsString = Convert.ToBase64String(byteStream);
                    //    Console.WriteLine("The rId of this Image is '{0}'", rId);
                    //}
                    //string row = "4";
                    //string col = "7";
                    //       TwoCellAnchor cellHoldingPicture = workSheet.DrawingsPart.WorksheetDrawing.OfType<TwoCellAnchor>()
                    //.Where(c => c.FromMarker.RowId.Text == row && c.FromMarker.ColumnId.Text == col).FirstOrDefault();

                    //       var picture = cellHoldingPicture.OfType<DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture>().FirstOrDefault();
                    //       string rIdofPicture = picture.BlipFill.Blip.Embed;

                    //       Console.WriteLine("The rID of this Anchor's [{0},{1}] Picture is '{2}'", row, col, rIdofPicture);

                    //       ImagePart imageInThisCell = (ImagePart)workSheet.DrawingsPart.GetPartById(rIdofPicture);
                    if (typeof(T) == typeof(DataTable)) return ReadSheetDataTable(excel, workbookPart, theSheet);
                    return ReadSheetData<T>(excel, workbookPart, theSheet);
                }
                finally
                {
                    doc.Dispose();
                }
            }
        }
        internal static DataSet ReadSpreadSheetExcelAll(CustomExcel excel)
        {
            excel.Columns = null;
            excel.ColumnsAll = true;
            if (string.IsNullOrEmpty(excel.Path) && excel.Stream == null) throw new Exception("path or stream not defined");

            DataSet dataSet = new DataSet();

            using (SpreadsheetDocument doc = excel.Stream != null ? SpreadsheetDocument.Open(excel.Stream, false) : SpreadsheetDocument.Open(excel.Path, false))
            {
                try
                {
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    foreach (Sheet theSheet in workbookPart.Workbook.Descendants<Sheet>())
                    {
                        DataTable dt = ReadSheetDataTable(excel, workbookPart, theSheet);
                        dataSet.Tables.Add(dt);
                    }
                }
                finally
                {
                    doc.Dispose();
                }
            }
            return dataSet;
        }
        internal static List<ExcelData<T>> ReadSheetData<T>(CustomExcel excel, WorkbookPart workbookPart, Sheet theSheet)
        {
            WorksheetPart worksheetPart = (WorksheetPart)(workbookPart.GetPartById(theSheet.Id));
            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
            List<ExcelData<T>> ListaData = new List<ExcelData<T>>();

            var lista = new List<ExcelTabla>();
            bool firstRow = true;

            int cantidad = 1;
            CultureInfo culture = excel.cultureInfo != null ? excel.cultureInfo : new CultureInfo("en-US");
            foreach (Row row in sheetData.Elements<Row>())
            {
                if (excel.StartRow > row.RowIndex?.Value)
                {
                    continue;
                }
                int i = 0, y = Convert.ToInt32(row.RowIndex?.Value);
                var Data = new ExcelData<T>();
                T dr = Activator.CreateInstance<T>();
                var drType = dr.GetType();
                string mensaje = string.Empty;

                foreach (Cell c in row.Elements<Cell>())
                {
                    string text = GetCellValueString(c, workbookPart);
                    if (string.IsNullOrEmpty(text)) continue;

                    text = text.Trim();
                    string celda = c.CellReference.Value;
                    celda = celda.Replace(y.ToString(), "");
                    if (firstRow)
                    {
                        string nombre = text.ReplaceAll(" ", "");
                        if (!lista.Any(x => x.nombre.ToLower() == nombre.ToLower()))
                        {
                            bool existColumn = drType.GetProperties().Any(x => x.Name.ToLower() == nombre.ToLower());
                            if (!existColumn && !excel.ColumnsAll) continue;
                            lista.Add(new ExcelTabla()
                            {
                                id = cantidad,
                                celda = celda,
                                nombre = nombre,
                            });
                            cantidad++;
                        }
                        else throw new Exception($"El Excel tiene La Columna {text} repetida.");

                    }
                    else
                    {
                        var obj = lista.Where(x => x.celda == celda).Select(x => new { x.id, x.nombre }).FirstOrDefault();
                        if (obj != null)
                        {
                            try
                            {
                                var prop = drType.GetProperty(obj.nombre);
                                object value = ConvertType(prop.PropertyType, text, culture);
                                prop.SetValue(dr, value);

                            }
                            catch
                            {
                                mensaje += $"{obj.nombre} [{text}] No Valido | ";
                            }
                            i = obj.id;
                        }
                        if (cantidad < i + 1) break;
                    }
                }
                if (i > 0)
                {
                    Data.Data = dr;
                    Data.RowIndex = y;
                    if (!string.IsNullOrEmpty(mensaje)) Data.ErrorMessage = $"{mensaje.Trim()} Fila Excel {y}";
                    ListaData.Add(Data);
                }
                else
                {
                    firstRow = false;
                }
            }
            return ListaData;
        }

        internal static DataTable ReadSheetDataTable(CustomExcel excel, WorkbookPart workbookPart, Sheet theSheet)
        {
            WorksheetPart worksheetPart = (WorksheetPart)(workbookPart.GetPartById(theSheet.Id));
            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
            DataTable dt = new DataTable(theSheet.Name);

            if (!string.IsNullOrEmpty(excel.IndexName) && !dt.Columns.Contains(excel.IndexName)) dt.Columns.Add(new DataColumn(excel.IndexName, typeof(int)));
            if (!string.IsNullOrEmpty(excel.ValidateName) && !dt.Columns.Contains(excel.ValidateName)) dt.Columns.Add(excel.ValidateName, typeof(string));
            if (excel.Columns != null)
            {
                foreach (var item in excel.Columns)
                {
                    item.ColumnName = item.ColumnName.ReplaceAll(" ", "");
                    dt.Columns.Add(item);
                }
            }
            var columna = dt.Columns;
            var lista = new List<ExcelTabla>();
            bool firstRow = true;

            int cantidad = 1;
            CultureInfo culture = excel.cultureInfo != null ? excel.cultureInfo : new CultureInfo("en-US");
            foreach (Row row in sheetData.Elements<Row>())
            {
                if (excel.StartRow > row.RowIndex?.Value)
                {
                    continue;
                }
                int i = 0, y = Convert.ToInt32(row.RowIndex?.Value);
                DataRow dr = dt.NewRow();
                string mensaje = string.Empty;

                foreach (Cell c in row.Elements<Cell>())
                {
                    string text = GetCellValueString(c, workbookPart);
                    if (string.IsNullOrEmpty(text)) continue;

                    text = text.Trim();
                    string celda = c.CellReference.Value;
                    celda = celda.Replace(y.ToString(), "");
                    if (firstRow)
                    {
                        string nombre = text.ReplaceAll(" ", "");
                        if (!lista.Any(x => x.nombre.ToLower() == nombre.ToLower()))
                        {
                            bool existColumn = columna.Contains(nombre);
                            if (!existColumn && !excel.ColumnsAll) continue;
                            if (!existColumn) dt.Columns.Add(new DataColumn() { ColumnName = nombre, DataType = Type.GetType("System.String"), Caption = text });
                            lista.Add(new ExcelTabla()
                            {
                                id = cantidad,
                                celda = celda,
                                nombre = nombre,
                            });
                            cantidad++;
                        }
                        else throw new Exception($"El Excel tiene La Columna {text} repetida.");

                    }
                    else
                    {
                        var obj = lista.Where(x => x.celda == celda).Select(x => new { x.id, x.nombre }).FirstOrDefault();
                        if (obj != null)
                        {
                            try
                            {
                                var type = columna[obj.nombre].DataType;
                                object value = ConvertType(type, text, culture);
                                dr[obj.nombre] = value;
                            }
                            catch
                            {
                                mensaje += $"{columna[obj.nombre].Caption} [{text}] No Valido | ";
                            }
                            i = obj.id;
                        }
                        if (cantidad < i + 1) break;
                    }
                }
                if (i > 0)
                {
                    if (!string.IsNullOrEmpty(excel.IndexName)) dr[excel.IndexName] = y;
                    if (!string.IsNullOrEmpty(excel.ValidateName) && !string.IsNullOrEmpty(mensaje)) dr[excel.ValidateName] = $"{mensaje.Trim()} Fila Excel {y}";
                    dt.Rows.Add(dr);
                }
                else
                {
                    firstRow = false;
                    columna = dt.Columns;
                }
            }
            return dt;
        }
        internal static string GetCellValueString(Cell c, WorkbookPart workbookPart)
        {
            if (c.DataType != null && c.DataType == CellValues.SharedString)
            {
                var result = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(Convert.ToInt32(c.InnerText)).InnerText;
                return result;
            }
            else if (c.CellValue != null) return c.CellValue.Text;
            return c.InnerText;
        }
        internal static object ConvertType(Type returnType, object valueType, CultureInfo culture)
        {
            Type type = returnType;
            var underlyingType = Nullable.GetUnderlyingType(returnType);
            if (underlyingType != null)
            {
                type = underlyingType;
            }

            if (type == valueType.GetType()) return valueType;

            string text = valueType.ToString();
            object value = text;
            var typecode = Type.GetTypeCode(type);
            switch (typecode)
            {
                case TypeCode.DateTime:
                    {
                        if (text.Contains("-") || text.Contains("T") || text.Contains("+"))
                        {
                            if (text.Contains("+")) value = DateTimeOffset.Parse(text, culture).DateTime;
                            else value = Convert.ToDateTime(value, culture);
                        }
                        else
                        {
                            value = DateTime.FromOADate(Double.Parse(text));
                        }
                    }
                    break;
                case TypeCode.Decimal:
                    {
                        if (text.Contains("E"))
                        {
                            value = Convert.ToSingle(text, culture);
                        }
                        value = Convert.ToDecimal(value, culture);
                    }
                    break;
                case TypeCode.Int32:
                    {
                        value = Convert.ToInt32(value, culture);
                    }
                    break;
                case TypeCode.Int64:
                    {
                        value = Convert.ToInt64(value, culture);
                    }
                    break;
                case TypeCode.Single:
                    {
                        value = Convert.ToSingle(value, culture);
                    }
                    break;
                case TypeCode.Double:
                    {
                        value = Convert.ToDouble(value, culture);
                    }
                    break;
                case TypeCode.Boolean:
                    {
                        value = Convert.ToBoolean(value, culture);
                    }
                    break;
                case TypeCode.Char:
                    {
                        value = Convert.ToChar(value, culture);
                    }
                    break;
                case TypeCode.Byte:
                    {
                        value = Convert.ToByte(value, culture);
                    }
                    break;

            }
            return value;
        }
        /*
        public static CustomFileExcel ToCSV(IEnumerable query)
        {
            var columns = GetProperties(query.AsQueryable().ElementType);

            var sb = new StringBuilder();

            foreach (var item in query)
            {
                var row = new List<string>();

                foreach (var column in columns)
                {
                    row.Add($"{GetValue(item, column.Key)}".Trim());
                }

                sb.AppendLine(string.Join(",", row.ToArray()));
            }


            var result = new CustomFileExcel()
            {
                stream = new MemoryStream(UTF8Encoding.Default.GetBytes($"{string.Join(",", columns.Select(c => c.Key))}{System.Environment.NewLine}{sb.ToString()}")),
                contentType = "text/csv"
            };
            return result;
        }
      
        public static CustomFileExcel ToExcel(IEnumerable query)
        {
            var columns = GetProperties(query.AsQueryable().ElementType);
            var stream = new MemoryStream();

            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet();

                var workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                GenerateWorkbookStylesPartContent(workbookStylesPart);

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
                sheets.Append(sheet);

                workbookPart.Workbook.Save();

                var sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                var headerRow = new Row();

                foreach (var column in columns)
                {
                    headerRow.Append(new Cell()
                    {
                        CellValue = new CellValue(column.Key),
                        DataType = new EnumValue<CellValues>(CellValues.String)
                    });
                }

                sheetData.AppendChild(headerRow);

                foreach (var item in query)
                {
                    var row = new Row();

                    foreach (var column in columns)
                    {
                        var value = GetValue(item, column.Key);
                        var stringValue = $"{value}".Trim();

                        var cell = new Cell();

                        var underlyingType = column.Value.IsGenericType &&
                            column.Value.GetGenericTypeDefinition() == typeof(Nullable<>) ?
                            Nullable.GetUnderlyingType(column.Value) : column.Value;

                        var typeCode = Type.GetTypeCode(underlyingType);

                        if (typeCode == TypeCode.DateTime)
                        {
                            if (!string.IsNullOrWhiteSpace(stringValue))
                            {
                                cell.CellValue = new CellValue() { Text = ((DateTime)value).ToOADate().ToString(System.Globalization.CultureInfo.InvariantCulture) };
                                cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                                cell.StyleIndex = (UInt32Value)1U;
                            }
                        }
                        else if (typeCode == TypeCode.Boolean)
                        {
                            cell.CellValue = new CellValue(stringValue.ToLower());
                            cell.DataType = new EnumValue<CellValues>(CellValues.Boolean);
                        }
                        else if (IsNumeric(typeCode))
                        {
                            if (value != null)
                            {
                                stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);
                            }
                            cell.CellValue = new CellValue(stringValue);
                            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                        }
                        else
                        {
                            cell.CellValue = new CellValue(stringValue);
                            cell.DataType = new EnumValue<CellValues>(CellValues.String);
                        }

                        row.Append(cell);
                    }

                    sheetData.AppendChild(row);
                }


                workbookPart.Workbook.Save();
            }

            if (stream?.Length > 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var result = new CustomFileExcel()
            {
                stream = stream,
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            return result;
        }



        public static object GetValue(object target, string name)
        {
            return target.GetType().GetProperty(name).GetValue(target);
        }

        public static IEnumerable<KeyValuePair<string, Type>> GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && IsSimpleType(p.PropertyType)).Select(p => new KeyValuePair<string, Type>(p.Name, p.PropertyType));
        }

        public static bool IsSimpleType(Type type)
        {
            var underlyingType = type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>) ?
                Nullable.GetUnderlyingType(type) : type;

            if (underlyingType == typeof(System.Guid))
                return true;

            var typeCode = Type.GetTypeCode(underlyingType);

            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.DateTime:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.String:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsNumeric(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        private static void GenerateWorkbookStylesPartContent(WorkbookStylesPart workbookStylesPart1)
        {
            Stylesheet stylesheet1 = new Stylesheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac x16r2 xr" } };
            stylesheet1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            stylesheet1.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            stylesheet1.AddNamespaceDeclaration("x16r2", "http://schemas.microsoft.com/office/spreadsheetml/2015/02/main");
            stylesheet1.AddNamespaceDeclaration("xr", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");

            Fonts fonts1 = new Fonts() { Count = (UInt32Value)1U, KnownFonts = true };

            Font font1 = new Font();
            FontSize fontSize1 = new FontSize() { Val = 11D };
            Color color1 = new Color() { Theme = (UInt32Value)1U };
            FontName fontName1 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering1 = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme1 = new FontScheme() { Val = FontSchemeValues.Minor };

            font1.Append(fontSize1);
            font1.Append(color1);
            font1.Append(fontName1);
            font1.Append(fontFamilyNumbering1);
            font1.Append(fontScheme1);

            fonts1.Append(font1);

            Fills fills1 = new Fills() { Count = (UInt32Value)2U };

            Fill fill1 = new Fill();
            PatternFill patternFill1 = new PatternFill() { PatternType = PatternValues.None };

            fill1.Append(patternFill1);

            Fill fill2 = new Fill();
            PatternFill patternFill2 = new PatternFill() { PatternType = PatternValues.Gray125 };

            fill2.Append(patternFill2);

            fills1.Append(fill1);
            fills1.Append(fill2);

            Borders borders1 = new Borders() { Count = (UInt32Value)1U };

            Border border1 = new Border();
            LeftBorder leftBorder1 = new LeftBorder();
            RightBorder rightBorder1 = new RightBorder();
            TopBorder topBorder1 = new TopBorder();
            BottomBorder bottomBorder1 = new BottomBorder();
            DiagonalBorder diagonalBorder1 = new DiagonalBorder();

            border1.Append(leftBorder1);
            border1.Append(rightBorder1);
            border1.Append(topBorder1);
            border1.Append(bottomBorder1);
            border1.Append(diagonalBorder1);

            borders1.Append(border1);

            CellStyleFormats cellStyleFormats1 = new CellStyleFormats() { Count = (UInt32Value)1U };
            CellFormat cellFormat1 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U };

            cellStyleFormats1.Append(cellFormat1);

            CellFormats cellFormats1 = new CellFormats() { Count = (UInt32Value)2U };
            CellFormat cellFormat2 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U };
            CellFormat cellFormat3 = new CellFormat() { NumberFormatId = (UInt32Value)14U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true };

            cellFormats1.Append(cellFormat2);
            cellFormats1.Append(cellFormat3);

            CellStyles cellStyles1 = new CellStyles() { Count = (UInt32Value)1U };
            CellStyle cellStyle1 = new CellStyle() { Name = "Normal", FormatId = (UInt32Value)0U, BuiltinId = (UInt32Value)0U };

            cellStyles1.Append(cellStyle1);
            DifferentialFormats differentialFormats1 = new DifferentialFormats() { Count = (UInt32Value)0U };
            TableStyles tableStyles1 = new TableStyles() { Count = (UInt32Value)0U, DefaultTableStyle = "TableStyleMedium2", DefaultPivotStyle = "PivotStyleLight16" };

            StylesheetExtensionList stylesheetExtensionList1 = new StylesheetExtensionList();

            StylesheetExtension stylesheetExtension1 = new StylesheetExtension() { Uri = "{EB79DEF2-80B8-43e5-95BD-54CBDDF9020C}" };
            stylesheetExtension1.AddNamespaceDeclaration("x14", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main");

            StylesheetExtension stylesheetExtension2 = new StylesheetExtension() { Uri = "{9260A510-F301-46a8-8635-F512D64BE5F5}" };
            stylesheetExtension2.AddNamespaceDeclaration("x15", "http://schemas.microsoft.com/office/spreadsheetml/2010/11/main");

            OpenXmlUnknownElement openXmlUnknownElement4 = OpenXmlUnknownElement.CreateOpenXmlUnknownElement("<x15:timelineStyles defaultTimelineStyle=\"TimeSlicerStyleLight1\" xmlns:x15=\"http://schemas.microsoft.com/office/spreadsheetml/2010/11/main\" />");

            stylesheetExtension2.Append(openXmlUnknownElement4);

            stylesheetExtensionList1.Append(stylesheetExtension1);
            stylesheetExtensionList1.Append(stylesheetExtension2);

            stylesheet1.Append(fonts1);
            stylesheet1.Append(fills1);
            stylesheet1.Append(borders1);
            stylesheet1.Append(cellStyleFormats1);
            stylesheet1.Append(cellFormats1);
            stylesheet1.Append(cellStyles1);
            stylesheet1.Append(differentialFormats1);
            stylesheet1.Append(tableStyles1);
            stylesheet1.Append(stylesheetExtensionList1);

            workbookStylesPart1.Stylesheet = stylesheet1;
        }
        
        */
        internal static string ReplaceAll(this string value, string quitar, string nuevo)
        {
            if (!string.IsNullOrEmpty(value) && quitar != nuevo)
            {
                string result = value;
                try
                {
                    value = value.Trim();
                    if (!nuevo.Contains(quitar))
                    {
                        while (value.Contains(quitar))
                        {
                            value = value.Replace(quitar, nuevo);
                        }
                    }
                    else value.Replace(quitar, nuevo);
                    return value.Trim();
                }
                catch
                {
                    return result;
                }
            }
            else
            {
                return value;
            }
        }
        internal class ExcelTabla
        {
            public int id { get; set; }
            public string celda { get; set; }
            public string nombre { get; set; }
        }
    }
    public class CustomExcel
    {
        public IEnumerable<DataColumn> Columns { get; set; }
        public bool ColumnsAll { get; set; }
        public string Path { get; set; }
        public string SheetName { get; set; }
        public Stream Stream { get; set; }
        public string IndexName { get; set; } = "index";
        public string ValidateName { get; set; } = "validate";
        public int StartRow { get; set; }
        public CultureInfo cultureInfo { get; set; }
    }

    public class ExcelData<TModel>
    {
        public int RowIndex { get; set; }
        public string ErrorMessage { get; set; }
        public TModel Data { get; set; }
    }
}
