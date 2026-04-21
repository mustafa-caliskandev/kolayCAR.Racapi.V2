using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using GemBox.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using Tuple = System.Tuple;

namespace KolayCAR.Broker.Infrastructure.Helpers
{

    public class ExcelHelper : IDataReader
    {
        private SpreadsheetDocument _document;
        private OpenXmlReader _reader;
        private OpenXmlElement _currentRow;
        private readonly string[] _headers;
        private readonly IDictionary<int, string> _sharedStrings;

        public ExcelHelper(string path, int sheetIndex = 0, bool firstRowAsHeader = true)
        {
            _document = SpreadsheetDocument.Open(path, false);
            _sharedStrings = GetSharedStrings(_document);

            var worksheetPart = _document.WorkbookPart.GetPartById(GetSheetByIndex(sheetIndex).Id.Value);
            _reader = OpenXmlReader.Create(worksheetPart);
            SkipRows(GetEmptyRowsCount(worksheetPart));
            _headers = firstRowAsHeader ? GetFirstRowAsHeaders() : GetRangeHeaders(worksheetPart);
        }

        public ExcelHelper(Stream stream, int sheetIndex = 0, bool firstRowAsHeader = true)
        {
            _document = SpreadsheetDocument.Open(stream, false);
            _sharedStrings = GetSharedStrings(_document);

            var worksheetPart = _document.WorkbookPart.GetPartById(GetSheetByIndex(sheetIndex).Id.Value);
            _reader = OpenXmlReader.Create(worksheetPart);
            SkipRows(GetEmptyRowsCount(worksheetPart));
            _headers = firstRowAsHeader ? GetFirstRowAsHeaders() : GetRangeHeaders(worksheetPart);
        }

        public ExcelHelper(string path, string sheetName, bool firstRowAsHeader = true)
        {
            _document = SpreadsheetDocument.Open(path, false);
            _sharedStrings = GetSharedStrings(_document);

            var worksheetPart = _document.WorkbookPart.GetPartById(GetSheetByName(sheetName).Id.Value);
            _reader = OpenXmlReader.Create(worksheetPart);
            SkipRows(GetEmptyRowsCount(worksheetPart));
            _headers = firstRowAsHeader ? GetFirstRowAsHeaders() : GetRangeHeaders(worksheetPart);
        }

        public ExcelHelper(Stream stream, string sheetName, bool firstRowAsHeader = true)
        {
            _document = SpreadsheetDocument.Open(stream, false);
            _sharedStrings = GetSharedStrings(_document);

            var worksheetPart = _document.WorkbookPart.GetPartById(GetSheetByName(sheetName).Id.Value);
            _reader = OpenXmlReader.Create(worksheetPart);
            SkipRows(GetEmptyRowsCount(worksheetPart));
            _headers = firstRowAsHeader ? GetFirstRowAsHeaders() : GetRangeHeaders(worksheetPart);
        }

        #region methods

        public static DataTable ReadExcelFile(string fileUrl)
        {
            try
            {
                //string path = Server.MapPath("~/App_Data/MyExcelFile.xlsx");
                //string path = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileUrl + ";Extended Properties=\"Excel 12.0 Xml;HDR=YES;\"";
                string connString = "";
                if (fileUrl.EndsWith(".xlsx"))
                {
                    //Excel 2007, above
                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileUrl + ";Extended Properties=\"Excel 12.0 Xml;HDR=YES;\"";
                }
                else if (fileUrl.EndsWith(".xls"))
                {
                    //Excel 2003
                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileUrl + ";Extended Properties=\"Excel 8.0;HDR=YES;\"";
                }
                OleDbConnection conn = new OleDbConnection(connString);
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                if (dt == null)
                {
                    return new DataTable();
                }

                return dt;

                string[] excelSheets = new string[dt.Rows.Count];
                int t = 0;
                // Add the sheet name to the string array.
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets[t] = row["TABLE_NAME"].ToString();
                    t++;
                }
                OleDbConnection excelConnection1 = new OleDbConnection(connString);
                string query = string.Format("Select * from [{0}]", excelSheets[0]);
                using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                {
                    DataSet ds = new DataSet();
                    dataAdapter.Fill(ds, "ExcelTable");
                    DataTable dtable = ds.Tables["ExcelTable"];
                    string excelData = dtable.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static DataTable ReadExcel(string path)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

            var workbook = ExcelFile.Load(path);

            // Select the first worksheet from the file.
            var worksheet = workbook.Worksheets[0];

            // Create DataTable from an Excel worksheet.
            var dataTable = worksheet.CreateDataTable(new CreateDataTableOptions()
            {
                ColumnHeaders = true,
                StartRow = 0,
                NumberOfColumns = 17,
                NumberOfRows = worksheet.Rows.Count,
                Resolution = ColumnTypeResolution.AutoPreferStringCurrentCulture
            });

            return dataTable;
        }

        private void SkipRows(int count)
        {
            for (var i = 0; i < count; i++)
                SkipRow();
        }

        private void SkipRow()
        {
            while (_reader.Read())
                if (_reader.ElementType == typeof(Row) && _reader.IsEndElement)
                    break;
        }

        private static int GetEmptyRowsCount(OpenXmlPart worksheetPart)
        {
            var emptyRowsCount = 0;
            using (var reader = OpenXmlReader.Create(worksheetPart))
            {
                while (reader.Read())
                {
                    if (reader.ElementType == typeof(Row))
                    {
                        var row = reader.LoadCurrentElement();
                        if (!string.IsNullOrEmpty(row.InnerText))
                            break;

                        emptyRowsCount++;
                    }
                }
            }
            return emptyRowsCount;
        }

        private IEnumerable<Sheet> GetSheets()
        {
            return _document.WorkbookPart.Workbook
                .GetFirstChild<Sheets>()
                .Elements<Sheet>();
        }

        private Sheet GetSheetByIndex(int sheetIndex)
        {
            var sheets = GetSheets().ToArray();
            if (sheetIndex < 0 || sheetIndex >= sheets.Count())
                throw new ApplicationException(Error.NotFoundSheetIndex(sheetIndex));

            return sheets.ElementAt(sheetIndex);
        }

        private Sheet GetSheetByName(string sheetName)
        {
            var sheet = GetSheets().FirstOrDefault(x => x.Name == sheetName);
            if (sheet == null)
                throw new ApplicationException(Error.NotFoundSheetName(sheetName));

            return sheet;
        }

        private string[] GetFirstRowAsHeaders()
        {
            var result = new string[] { };
            if (Read())
            {
                result = AdjustRow(_currentRow, -1)
                    .Select(GetCellValue)
                    .ToArray();
            }
            _currentRow = null;
            return result;
        }

        private static string[] GetRangeHeaders(OpenXmlPart worksheetPart)
        {
            var count = 0;
            using (var reader = OpenXmlReader.Create(worksheetPart))
            {
                while (reader.Read())
                {
                    if (reader.ElementType == typeof(Row))
                    {
                        count = reader.LoadCurrentElement().Elements<Cell>().Count();
                        break;
                    }
                }
            }
            return Enumerable.Range(0, count).Select(x => "col" + x).ToArray();
        }

        private static IDictionary<int, string> GetSharedStrings(SpreadsheetDocument document)
        {
            return document.WorkbookPart.SharedStringTablePart.SharedStringTable
                .Select((x, i) => Tuple.Create(i, x.InnerText))
                .ToDictionary(x => x.Item1, x => x.Item2);
        }

        private string GetCellValue(CellType cell)
        {
            if (cell == null || cell.CellValue == null)
                return null;

            var value = cell.CellValue.InnerXml;
            if (value == null)
                return null;

            int index;
            if (int.TryParse(value, out index) && cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                return _sharedStrings[index];

            return value;
        }

        private static IEnumerable<Cell> AdjustRow(OpenXmlElement row, int capacity)
        {
            if (row == null)
                return new Cell[] { };

            var cells = row.Elements<Cell>().ToArray();
            if (capacity == -1)
                capacity = cells.Count();

            var list = cells
                .OrderBy(x => ExcelUtil.GetColumnIndexByName(x.CellReference.Value))
                .Take(capacity)
                .ToList();

            while (list.Count() < capacity)
                list.Add(new Cell());

            return list;
        }

        #endregion

        #region IDataReader Members

        public void Close()
        {
            Dispose();
        }

        public int Depth
        {
            get { return 0; }
        }

        public DataTable GetSchemaTable()
        {
            return null;
        }

        public bool IsClosed
        {
            get { return _document == null; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            while (_reader.Read())
            {
                if (_reader.ElementType == typeof(Row))
                {
                    _currentRow = _reader.LoadCurrentElement();
                    // skip empty rows
                    if (string.IsNullOrEmpty(_currentRow.InnerText))
                        continue;
                    break;
                }
            }
            return _currentRow != null && !_reader.EOF;
        }

        public int RecordsAffected
        {
            /*
             * RecordsAffected is only applicable to batch statements
             * that include inserts/updates/deletes. The sample always
             * returns -1.
             */
            get { return -1; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader.Dispose();
                _reader = null;
            }

            if (_document != null)
            {
                _document.Dispose();
                _document = null;
            }
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { return _headers.Length; }
        }

        public bool GetBoolean(int i)
        {
            return SafeConverter.Convert<bool>(GetValue(i));
        }

        public byte GetByte(int i)
        {
            return SafeConverter.Convert<byte>(GetValue(i));
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return SafeConverter.Convert<char>(GetValue(i));
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            return null;
        }

        public string GetDataTypeName(int i)
        {
            return typeof(string).Name;
        }

        public DateTime GetDateTime(int i)
        {
            return DateTime.FromOADate(GetDouble(i));
        }

        public decimal GetDecimal(int i)
        {
            var value = GetValue(i);
            if (value != null)
            {
                decimal num;
                if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out num))
                    return num;
            }
            return SafeConverter.Convert<decimal>(value);
        }

        public double GetDouble(int i)
        {
            var value = GetValue(i);
            if (value != null)
            {
                double num;
                if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out num))
                    return num;
            }
            return SafeConverter.Convert<double>(value);
        }

        public float GetFloat(int i)
        {
            var value = GetValue(i);
            if (value != null)
            {
                float num;
                if (float.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out num))
                    return num;
            }
            return SafeConverter.Convert<float>(value);
        }

        public Type GetFieldType(int i)
        {
            return typeof(string);
        }

        public Guid GetGuid(int i)
        {
            return SafeConverter.Convert<Guid>(GetValue(i));
        }

        public short GetInt16(int i)
        {
            return SafeConverter.Convert<short>(GetValue(i));
        }

        public int GetInt32(int i)
        {
            return SafeConverter.Convert<int>(GetValue(i));
        }

        public long GetInt64(int i)
        {
            return SafeConverter.Convert<long>(GetValue(i));
        }

        public string GetName(int i)
        {
            return _headers[i];
        }

        public int GetOrdinal(string name)
        {
            for (var i = 0; i < _headers.Length; i++)
                if (string.Equals(_headers[i], name, StringComparison.InvariantCultureIgnoreCase))
                    return i;

            return -1;
        }

        public string GetString(int i)
        {
            return SafeConverter.Convert<string>(GetValue(i));
        }

        public object GetValue(int i)
        {
            var cell = AdjustRow(_currentRow, _headers.Length).ElementAtOrDefault(i);
            return GetCellValue(cell);
        }

        public int GetValues(object[] values)
        {
            var num = values.Length < _headers.Length ? values.Length : _headers.Length;
            var row = AdjustRow(_currentRow, num)
                .Select(GetCellValue)
                .ToArray();

            for (var i = 0; i < num; i++)
                values[i] = row[i];

            return num;
        }

        public bool IsDBNull(int i)
        {
            return Convert.IsDBNull(GetValue(i));
        }

        public object this[string name]
        {
            get { return this[GetOrdinal(name)]; }
        }

        public object this[int i]
        {
            get { return GetValue(i); }
        }

        #endregion
    }

    internal static class Error
    {
        public static string NotFoundSheetIndex(int index)
        {
            return string.Format("The sheet with index {0} not found.", index);
        }

        public static string NotFoundSheetName(string name)
        {
            return string.Format("The sheet with name {0} not found.", name);
        }
    }

    public static class ExcelUtil
    {
        public static int GetColumnIndexByName(string colName)
        {
            var name = GetStartingLettersOnly(colName);

            int number = 0, pow = 1;
            for (var i = name.Length - 1; i >= 0; i--)
            {
                number += (name[i] - 'A' + 1) * pow;
                pow *= 26;
            }
            return number - 1;
        }

        private static string GetStartingLettersOnly(string colName)
        {
            var result = string.Empty;
            foreach (var ch in colName)
            {
                if (char.IsLetter(ch))
                    result += ch;
                else
                    break;
            }
            return result;
        }
    }

    public static class SafeConverter
    {
        public static object Convert(object value, Type desiredType, object defaultValue = null)
        {
            return CoerceValue(desiredType, value, defaultValue);
        }

        public static T Convert<T>(object value)
        {
            return (T)CoerceValue(typeof(T), value, default(T));
        }

        private static object CoerceValue(Type desiredType, object value, object defaultValue)
        {
            if (value == null)
                return defaultValue;

            var valueType = value.GetType();
            if (desiredType.IsAssignableFrom(valueType))
                return value;

            if (desiredType.IsGenericType)
            {
                if (desiredType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    if (valueType == typeof(string) && string.IsNullOrEmpty(value.ToString()))
                        return null;
            }

            desiredType = GetNullableUnderlyingType(desiredType);

            try
            {
                if (desiredType == typeof(string))
                    return value.ToString();

                if (desiredType == typeof(bool) && Equals(value, "0"))
                    return false;

                if (desiredType == typeof(bool) && Equals(value, "1"))
                    return true;

                return System.Convert.ChangeType(value, desiredType);
            }
            catch
            {
                var cnv = TypeDescriptor.GetConverter(desiredType);
                if (cnv.CanConvertFrom(valueType))
                    return cnv.ConvertFrom(value);

                return value;
            }
        }

        private static Type GetNullableUnderlyingType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Nullable.GetUnderlyingType(type);

            return type;
        }
    }
}
