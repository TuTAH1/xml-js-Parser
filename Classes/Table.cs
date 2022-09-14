using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Titanium.Consol;
using Titanium;
using Application;
using static xml_js_Parser.Classes.Methods;

namespace xml_js_Parser.Classes
{
	public class Table : IEnumerable 
	{
		public List<TableRow> rows;
		public int RowsCount => rows.Count;
		public IEnumerator GetEnumerator() => rows.GetEnumerator();
		public static string TextColumnName = "Наименование поля"
			, CodeColumnName = "Название переменной в XML"
			, IsOptionalColumnName = "Обязательность заполнения"
			;

		public Table()
		{
			rows = new List<TableRow>();
		}

		public Table(List<string[]> Data)
		{
			rows = new List<TableRow>(Data.Count);
			foreach (var data in Data)
			{
				if (data.Length != 3) 
					throw new ArgumentException($"Data length should be 3. Data value: {data.ToStringT(",")}");
				rows.Add(new TableRow(data[0],data[1],data[2].Contains("-")));
			}
		}

		public Table(string tableText)
		{

			int textColumnN = 2, codeColumnN = 3, optionalColumnN = 8, N = 11; //:Номера столбцов
			var RowsText = tableText.Split('\n');
			rows = new List<TableRow>(RowsText.Length);

			List<string> unfinishedRow = new List<string>();
			for (var i = 0; i < RowsText.Length; i++)
			{
				var rowText = RowsText[i];
				var values = rowText.Remove("\r").Split('\t');
				if (i == 0)
				{
					for (var j = 0; j < values.Length; j++)
					{
						var header = values[j];
						if (header.Contains(TextColumnName)) textColumnN = j;
						else if (header.Contains(CodeColumnName)) codeColumnN = j;
						else if (header.Contains(IsOptionalColumnName)) optionalColumnN = j;
					}

					N = values.Length;

					continue;
				}

				if (values.Length < N)
				{
					//ReWrite(new []{"\nСтрока ", i.ToString(), " была пропущена: Она содержит ", values.Length.ToString(), " столбцов, вместо положенных ", N.ToString()}, new []{c.gray, c.cyan, c.red, c.cyan,c.gray,c.cyan});
					if (!unfinishedRow.Any())
					{
						if(values.Length == 1)
						{
							if(!(values[0].Contains("Блок")||values[0].Contains("Шаг")||values[0].IsNullOrEmpty()))
								ReWrite(new []{"\nСтрока ", i.ToString(), " была пропущена",", так как похожа на комментарий"}, new []{c.gray, c.cyan, c.yellow, c.gray});
						}
						else if (values.Length > 1) //: Первая строка с объедененной ячейкой или многостроковым значчением
						{
							unfinishedRow.AddRange(values);
						}
					} 
					else
					{ //: Продолжение первой строки с объедененной ячейкой
						var isMergedCell = values[0] == "";
						if (isMergedCell) values = values[1..];
						if(!values[0].IsNullOrEmpty()) 
							unfinishedRow[^1] = unfinishedRow[^1] + "\n" + values[0];
						unfinishedRow.AddRange(values[1..]);

						if (unfinishedRow.Count == N)
						{
							rows.Add(new TableRow(unfinishedRow[codeColumnN], unfinishedRow[textColumnN], this, unfinishedRow[optionalColumnN].Contains("-")));
							unfinishedRow = new List<string>();
							//unfinishedRow = new List<string>();
						} else if (unfinishedRow.Count > N)
						{
							ReWrite(new[] { "\nНе удалось восстановить строку ",i.ToString(),": в ней содержится ", unfinishedRow.Count.ToString(), " столбцов вместо ", N.ToString() }, new []{c.red,c.cyan,c.gray,c.cyan,c.gray,c.green});
							unfinishedRow = new List<string>();
						}
							
					}

					continue;
				} 
				unfinishedRow = new List<string>();
				rows.Add(new TableRow(values[codeColumnN],values[textColumnN], this, values[optionalColumnN].Contains("-")));
			}
		}

		public void Add(string code, string text, bool optional = true) => rows.Add(new TableRow(code,text,this, optional));

		public void Add(TableRow tr)
		{
			tr.SourceTable = this;
			rows.Add(tr);
		}

		private void AddRange(IEnumerable<TableRow> trs)
		{
			foreach (var tr in trs)
			{
				Add(tr);
			}
		}
		public static Table operator +(Table t1, Table t2)
		{
			Table t3 = new Table();
			t3.AddRange(t1.rows);
			t3.AddRange(t2.rows);
			return t3;
		}

		public void Append(Table t) => AddRange(t.rows);

		public Table.TableRow GetByCode(string code)
		{
			if (this == null || code == null) return null;

			return this.rows.FirstOrDefault(row => row.Code == code);
		}

		public class TableRow
		{
			public string Text;
			public string Code;
			public bool Optional;
			public Table SourceTable;

			internal TableRow(string code, string text, Table sourceTable, bool optional = true)
			{
				Text = text;
				Code = code;
				SourceTable = sourceTable;
				Optional = optional;
			}

			public TableRow(string code, string text, bool optional = true)
			{
				Text = text;
				Code = code;
				SourceTable = null;
				Optional = optional;
			} 
		}
	}
}
