using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Office.CustomUI;
using static Titanium.Consol;
using Titanium;
using DocTable =  DocumentFormat.OpenXml.Wordprocessing.Table;
using DocTableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using DocTableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using DocumentFormat.OpenXml.Spreadsheet;

namespace xml_js_Parser.Classes
{
	public class Table : IEnumerable
	{
		public List<Block> Blocks;
		public static string TextColumnName = "Наименование", CodeColumnName = "XML", IsOptionalColumnName = "Обязательность", FormatControl = "контроль";
		IEnumerator IEnumerable.GetEnumerator() => Blocks.GetEnumerator();

		public Table(Table table)
		{
			Blocks = table.Blocks;
		}

		public Table(DocTable table)
		{
			int textColumnN = 2, codeColumnN = 3, optionalColumnN = 8, formatControlN = 9, N = 11; //:Номера столбцов по умолчанию
			var tableRows = table.Elements<DocTableRow>() as DocTableRow[];
			Blocks = new List<Block>();
			
			for (var i = 0; i < tableRows.Length; i++)
			{
				var row = tableRows[i];
				var cells = (row.Elements<DocTableCell>() as DocTableCell[]).ToArray(Cell => Cell.InnerText);
				if (i == 0) //! Поиск номеров нужных столбцов
				{
					for (var j = 0; j < cells.Length; j++)
					{
						var header = cells[j];
						if (header.Contains(TextColumnName, StringComparison.OrdinalIgnoreCase)) textColumnN = j;
						else if (header.Contains(CodeColumnName, StringComparison.OrdinalIgnoreCase)) codeColumnN = j;
						else if (header.Contains(IsOptionalColumnName, StringComparison.OrdinalIgnoreCase)) optionalColumnN = j;
						else if (header.Contains(FormatControl, StringComparison.OrdinalIgnoreCase)) formatControlN = j;
					}

					N = cells.Length;
				}
				else //! Добавление значения столбцов
				{
					switch (cells.Length)
					{
						case 1:
						{
							if (!(cells[0].Contains("Блок") || cells[0].Contains("Шаг") || cells[0].IsNullOrEmpty()))
							{
								ReWrite(new[] { "\nСтрока ", i.ToString(), " была пропущена", ", так как похожа на комментарий" }, new[] { c.gray, c.cyan, c.yellow, c.gray });
							}
							else 
							{
								if (cells[0].Contains("Блок")) //:Добавление блока (названия)
									Blocks.Add(new Block(cells[0].Slice(".",".",LastEnd:false)));
								else if (!cells[0].Contains("Шаг"))
									ReWrite(new []{"Обнаружена строка, похожая на блок описания, но ключевых слов не найдено:\n", cells[0]}, new [] { c.red , c.gray});
							}
									
						} break;
						//: Первая строка с объедененной ячейкой или многостроковым значением
						case > 1:
							if (Blocks.Empty()) throw new InvalidOperationException("Не найдено описание блока");
							Blocks.Last().Add(new Block.TableRow(cells[textColumnN], cells[codeColumnN], cells[optionalColumnN], cells[formatControlN], Blocks.Last()));
							break;
					}
				}
			}

		}
		public class Block : IEnumerable
		{
			public string Name;
			public List<TableRow> rows;
			public int RowsCount => rows.Count;
			public IEnumerator GetEnumerator() => rows.GetEnumerator();

			public Block()
			{
				rows = new List<TableRow>();
			}
			public Block(string name)
			{
				rows = new List<TableRow>();
				Name = name;
			}

			public Block(List<string[]> Data, string Name = null)
			{
				rows = new List<TableRow>(Data.Count);
				foreach (var data in Data)
				{
					if (data.Length != 4)
						throw new ArgumentException($"Data length should be 4. Data value: {data.ToStringT(",")}");
					rows.Add(new TableRow(data));
				}

				this.Name = Name;
			}

			public void Add(string code, string text, bool optional = true) => rows.Add(new TableRow(code, text, this, optional));

			public void Add(TableRow tr)
			{
				tr.Source = this;
				rows.Add(tr);
			}

			private void AddRange(IEnumerable<TableRow> trs)
			{
				foreach (var tr in trs)
				{
					Add(tr);
				}
			}

			public static Block operator +(Block t1, Block t2)
			{
				Block t3 = new Block();
				t3.AddRange(t1.rows);
				t3.AddRange(t2.rows);
				return t3;
			}

			public static Block operator +(Block t1, Table t2)
			{
				Block t3 = new Block();
				t3.AddRange(t1.rows);
				foreach (Block block in t2)
				{
					t3.AddRange(block.rows);
				}
				return t3;
			}

			public static Table operator +(Table t1, Block t2)
			{
				var t3 = new Table(t1);
				t3.Blocks.Add(t2);
				return t3;
			}

			public void Append(Block t) => AddRange(t.rows);

			public Block.TableRow GetByCode(string code)
			{
				if (this == null || code == null) return null;

				return this.rows.FirstOrDefault(row => row.Code == code);
			}

			public class TableRow
			{
				public string Text;
				public string Code;
				public bool Optional;
				public Block Source;

				internal TableRow(string code, string text, Block Source, bool optional = true)
				{
					Text = text;
					Code = code;
					this.Source = Source;
					Optional = optional;
				}

				public TableRow(string code, string text, bool optional = true)
				{
					Text = text;
					Code = code;
					Source = null;
					Optional = optional;
				}

				/// <summary>
				/// 
				/// </summary>
				/// <param name="data">sequence with 4 elements, that contains
				///<list type="number">
				///<item>Text</item>
				///<item>Code</item>
				///<item>OptionalityText (+/-)</item>
				///<item>FormatControlText</item>
				///</list>
				/// </param>
				public TableRow(IEnumerable<string> data, Block Source = null)
				{
					if (data.Count() != 4)
						throw new ArgumentException($"Data length should be 4. Data value: {data.ToStringT(",")}");
					new TableRow(data.ElementAt(0), data.ElementAt(1), data.ElementAt(2), data.ElementAt(3), Source);
				}

				public TableRow(string text, string code, string OptionalityText, string FormatControlText, Block Source = null)
				{
					Text = text;
					Code = code;
					this.Source = Source;
					Optional =OptionalityText.Contains("-") || FormatControlText.ContainsAny(new[] { "Поле отображается", "Поле видно" });
				}
			}
		}
	}
}

#region Мусорка

		// public TableBlock(string TableText) => new TableBlock(TableText.Split('\n'));
			//
			// public TableBlock(IEnumerable<string> TableRows)
			// {
			//
			// 	int textColumnN = 2, codeColumnN = 3, optionalColumnN = 8, formatControlN = 9, N = 11; //:Номера столбцов по умолчанию
			// 	rows = new List<TableRow>(TableRows.Count());
			//
			// 	List<string> unfinishedRow = new List<string>();
			// 	for (var i = 0; i < TableRows.Count(); i++)
			// 	{
			// 		var rowText = TableRows.ElementAt(i);
			// 		var values = rowText.Remove("\r").Split('\t');
			// 		if (i == 0)//! Поиск номеров нужных столбцов
			// 		{
			// 			for (var j = 0; j < values.Length; j++)
			// 			{
			// 				var header = values[j];
			// 				if (header.Contains(TextColumnName, StringComparison.OrdinalIgnoreCase)) textColumnN = j;
			// 				else if (header.Contains(CodeColumnName, StringComparison.OrdinalIgnoreCase)) codeColumnN = j;
			// 				else if (header.Contains(IsOptionalColumnName, StringComparison.OrdinalIgnoreCase)) optionalColumnN = j;
			// 				else if (header.Contains(FormatControl, StringComparison.OrdinalIgnoreCase)) formatControlN = j;
			// 			}
			//
			// 			N = values.Length;
			// 		}
			// 		else //! Добавление значения столбцов
			// 		{
			//
			// 			if (values.Length < N)
			// 			{
			// 				//ReWrite(new []{"\nСтрока ", i.ToString(), " была пропущена: Она содержит ", values.Length.ToString(), " столбцов, вместо положенных ", N.ToString()}, new []{c.gray, c.cyan, c.red, c.cyan,c.gray,c.cyan});
			// 				if (!unfinishedRow.Any())
			// 				{
			// 					if (values.Length == 1)
			// 					{
			// 						if (!(values[0].Contains("Блок") || values[0].Contains("Шаг") || values[0].IsNullOrEmpty()))
			// 							ReWrite(new[] { "\nСтрока ", i.ToString(), " была пропущена", ", так как похожа на комментарий" }, new[] { c.gray, c.cyan, c.yellow, c.gray });
			// 					}
			// 					else if (values.Length > 1) //: Первая строка с объедененной ячейкой или многостроковым значчением
			// 					{
			// 						unfinishedRow.AddRange(values);
			// 					}
			// 				}
			// 				else
			// 				{
			// 					//: Продолжение первой строки с объедененной ячейкой
			// 					var isMergedCell = values[0] == "";
			// 					if (isMergedCell) values = values[1..];
			// 					if (!values[0].IsNullOrEmpty())
			// 						unfinishedRow[^1] = unfinishedRow[^1] + "\n" + values[0];
			// 					unfinishedRow.AddRange(values[1..]);
			//
			// 					if (unfinishedRow.Count == N)
			// 					{
			// 						rows.Add(new TableRow(new[] { unfinishedRow[codeColumnN], unfinishedRow[textColumnN], unfinishedRow[optionalColumnN] }, this));
			// 						unfinishedRow = new List<string>();
			// 						//unfinishedRow = new List<string>();
			// 					}
			// 					else if (unfinishedRow.Count > N)
			// 					{
			// 						ReWrite(new[] { "\nНе удалось восстановить строку ", i.ToString(), ": в ней содержится ", unfinishedRow.Count.ToString(), " столбцов вместо ", N.ToString() }, new[] { c.red, c.cyan, c.gray, c.cyan, c.gray, c.green });
			// 						unfinishedRow = new List<string>();
			// 					}
			//
			// 				}
			//
			// 				continue;
			// 			}
			//
			// 			unfinishedRow = new List<string>();
			// 			rows.Add(new TableRow(new[] { unfinishedRow[codeColumnN], unfinishedRow[textColumnN], unfinishedRow[optionalColumnN] }, this));
			// 		}
			// 	}
			// }


#endregion