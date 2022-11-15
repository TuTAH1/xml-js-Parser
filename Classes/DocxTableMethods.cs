using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Titanium.Consol;

using DocTable =  DocumentFormat.OpenXml.Wordprocessing.Table;
using DocTableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using DocTableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;

namespace xml_js_Parser.Classes
{
	internal class DocxTableMethods
	{
		public static DocTable OpenFile(string filepath) => OpenFile(filepath, out _);

		public static DocTable OpenFile(string filepath, out int[] MainColumnNumbers)
		{
			ReWrite("\nИдёт чтение .docx файла... ", c.purple);
			using WordprocessingDocument doc =
				WordprocessingDocument.Open(filepath, false);
			var tables = doc.MainDocumentPart.Document.Body.Elements<DocTable>();
			ReWrite("Файл открыт", c.green); ReWrite("\nИдёт поиск нужной таблицы... ",c.purple);
			DocTable specTable = null;
			MainColumnNumbers = null;
			int NsMax = 0;
			foreach (DocTable table in tables)
			{
				var Header = table.Elements<DocTableRow>().ElementAt(0);
				MainColumnNumbers = Header.GetMainColumnsIndexes();
				if (MainColumnNumbers == null) continue;
				specTable = table;
				break;
			}
			if(specTable==null) throw new ArgumentException("Подходящей таблицы не найдено в .docx файле");
			ReWrite("Нужная таблица найдена",c.green);
			return specTable;
		}
	}
}