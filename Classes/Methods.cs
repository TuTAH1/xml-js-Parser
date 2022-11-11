using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Titanium.Consol;
using System.Xml.Linq;
using Application;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using Titanium;
using static xml_js_Parser.Classes.Table;

using DocTable =  DocumentFormat.OpenXml.Wordprocessing.Table;
using DocTableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using DocTableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;

namespace xml_js_Parser.Classes
{
	public static class Methods
	{
		public static bool DeleteGroups(this TreeNode<Data> Tree, bool IsRootNode = true)
		{
			var childCount = Tree.Childs.Count;
			for (var i = 0; i < childCount;)
			{
				var child = Tree.Childs[i];
				if (!child.DeleteGroups(false)) i++;
				else childCount--;
			}

			if (!IsRootNode && Tree.Value?.IsGroup != false)
			{
				Tree.Delete();
				return true;
			}

			return false;
		
		}

		public static string Name(string name) => @"{http://idecs.atc.ru/pgufg/ws/fgapc/}" + name;

		public class XMLData
		{
			public string Type;
			public string? NameType;
			public string? ValueType;
			public bool Recursive;
			public bool AskIfNotFound;

			public XMLData(string Type, string? NameType = null, string? ValueType = null, bool Recursive = false, bool askIfNotFound = false)
			{
				this.Type = Type;
				this.AskIfNotFound = askIfNotFound;
				this.NameType = NameType;
				this.ValueType = ValueType;
				this.Recursive = Recursive;
			}
		}

		internal class XmlFile
		{
			public XDocument Doc;
			public FileInfo Info;

			private XmlFile(XDocument Doc, FileInfo Info)
			{
				this.Doc = Doc;
				this.Info = Info;
			}

			public static XmlFile Get()
			{
				var cursorPosition = GetCurPos();
				var filepath = GetFilepath("xml", 
					() => ReWrite(new[] { "\nНапишите путь ", ".XML", " файла для парсинга (или ", "перетащите", " его) и нажмите", " Enter: " }, new[] { c.Default, c.lime, c.Default, c.lime, c.Default, c.yellow }));
				try //:										Парсинг
				{
					ReWrite("\nИдёт парсинг xml... ", c.purple);
					XDocument xml = XDocument.Load(filepath);
					ReWrite("\nПарсинг успешно завершён", c.green);
					return new XmlFile(xml, new FileInfo(filepath));
				}
				catch (Exception e)
				{
					ReWrite($"\nОшибка: {e.Message}", c.red, ClearLine: true);
					ReWrite(" (повторите попытку)", c.Default);
					SetCurPos(cursorPosition);
					return Get();
				}
			
			}

			public static XmlFile Get(string filepath)
			{
				var cursorPosition = GetCurPos();
				try //:										Парсинг
				{
					ReWrite("\nИдёт парсинг xml... ", c.purple);
					XDocument xml = XDocument.Load(filepath);
					ReWrite("\nПарсинг успешно завершён", c.green);
					return new XmlFile(xml, new FileInfo(filepath));
				}
				catch (Exception e)
				{
					ReWrite($"\nОшибка: {e.Message}", c.red, ClearLine: true);
					ReWrite(" (повторите попытку)", c.Default);
					SetCurPos(cursorPosition);
					return null;
				}
			
			}
		}

		public class Data //TODO: придумать нормальное имя
		{ //\ Нужно бы избавиться от зависимостей и заменить его на TableRow при методе DocxParse
			public string? Code;
			public string? Text;
			public bool? Optional;
			public string? Value; //\ НЕ ИСПОЛЬЗУЕТСЯ
			public bool IsGroup;

			public XElement xml; //\ Не используется при Docx parse

			public Data(XElement Xml, string? code = null, string text = null, bool? optional = null, string? Value = null, bool isGroup = false)
			{
				this.Code = code;
				this.Value = Value;
				xml = Xml;
				Text = text;
				Optional = optional;
				IsGroup = isGroup;
			}

			public Data(string? code = null, string text = null, bool? optional = null, string? Value = null, bool isGroup = false)
			{
				this.Code = code;
				this.Value = Value;
				Text = text;
				Optional = optional;
				IsGroup = isGroup;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filepath"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static List<string[]> GetFileDocx(string filepath)
		{
			ReWrite("\nИдёт чтение .doc файла... ", c.purple);
			List<string[]> Data = new();
			using WordprocessingDocument doc =
				WordprocessingDocument.Open(filepath, false);
			var tables = doc.MainDocumentPart.Document.Body.Elements<DocTable>();
			ReWrite("Файл открыт", c.green); ReWrite("\nИдёт поиск нужной таблицы... ",c.purple);
			DocTable specTable = null;
			int[] Ns = null;
			int NsMax = 0;
			foreach (DocTable table in tables)
			{
				var Header = table.Elements<TableRow>().ElementAt(0);
				Ns = Header.GetMainColumnsIndexes();
				if (Ns == null) continue;
				specTable = table;
				break;
			}
			if(specTable==null) throw new ArgumentException("Подходящей таблицы не найдено в .docx файле");
			ReWrite("Нужная таблица найдена",c.green);

			NsMax = Ns.Max();

			foreach (var row in specTable.Elements<TableRow>().ToArray()[1..])
			{
				var cells = row.Elements<TableCell>().ToArray();
				if (cells.Length >=NsMax)
					Data.Add(new []{cells[Ns[0]].InnerText, cells[Ns[1]].InnerText, cells[Ns[2]].InnerText });
			}

			return Data;
		}

		// internal class FormTableRow
		// {
		// 	public string RuName;
		// 	public string XmlName;
		// 	public string FieldType; //TODO: typechange
		// 	public string Restrictions; //TODO: typechange
		// 	public string FillingType; //TODO: typechange
		// 	public string DefaultValue;
		// 	public bool 
		//
		// }

		public static int[] GetMainColumnsIndexes(this TableRow tr)
		{
			var tcs = tr.Elements<TableCell>().ToList();
			var res = new int[4].FillAndGet(-1);
			for (int i = 0; i < tcs.Count; i++)
			{
				if (tcs[i].InnerText.Contains(Table.CodeColumnName, StringComparison.OrdinalIgnoreCase)) res[0] = i;
				else if (tcs[i].InnerText.Contains(Table.TextColumnName, StringComparison.OrdinalIgnoreCase)) res[1] = i;
				else if (tcs[i].InnerText.Contains(Table.IsOptionalColumnName, StringComparison.OrdinalIgnoreCase)) res[2] = i;
				else if (tcs[i].InnerText.Contains(Table.FormatControl, StringComparison.OrdinalIgnoreCase)) res[3] = i;
			}

			if (res.All(x => x != -1)) return res;
			{
				for (int i = 0; i < 3; i++)
				{
					if(res[i]!=-1) 
						ReWrite(new []{"В таблице найден только столбец ", i switch
					{
						0 => "названий",
						1 => "кодов",
						2 => "обязательности"
					} + " полей\n"}, new []{c.red, c.cyan});
				}

				return null;
			}

		}

		public static TreeNode<Data> CreateTree(DocTable Table)
		{
			
			int textColumnN = 2, codeColumnN = 3, optionalColumnN = 8, formatControlN = 9; //:Номера столбцов по умолчанию
			var tableRows = Table.Elements<DocTableRow>().ToArray();
			var headerCells = tableRows[0].Elements<DocTableCell>().ToArray(Cell => Cell.InnerText);
			for (var j = 0; j < headerCells.Length; j++)
			{
				var header = headerCells[j];
				if (header.Contains(TextColumnName, StringComparison.OrdinalIgnoreCase)) textColumnN = j;
				else if (header.Contains(CodeColumnName, StringComparison.OrdinalIgnoreCase)) codeColumnN = j;
				else if (header.Contains(IsOptionalColumnName, StringComparison.OrdinalIgnoreCase)) optionalColumnN = j;
				else if (header.Contains(FormatControl, StringComparison.OrdinalIgnoreCase)) formatControlN = j;
			}
			
			if (tableRows.Length <1) throw new ArgumentException("Таблица пуста");

			TreeNode<Data> root = new TreeNode<Data>();
			bool skip = false;

			for (var i = 1; i < tableRows.Length; i++) //! Добавление значения столбцов
			{
				var row = tableRows[i];
				var cells = row.Elements<DocTableCell>().ToArray(Cell => Cell.InnerText);
				switch (cells.Length)
				{
					//! Строка с названием блока или шага
					case 1:
					{
						if (!(cells[0].Contains("Блок") || cells[0].Contains("Шаг") || cells[0].IsNullOrEmpty()))
						{
							ReWrite(new[] { "\nСтрока ", i.ToString(), " была пропущена", ", так как похожа на комментарий" }, new[] { c.gray, c.cyan, c.yellow, c.gray });
						}
						else
						{
							if (cells[0].ToLower().ContainsAny("блок", "шаг")) //:Добавление блока (названия)
							{
								string name = cells[0].Slice(new Regex(@"[Б|б]лок *\d+\.? *|[Ш|ш]аг *\d+\.? *"), ".", LastEnd: false);
								if (Program.SkipList.Contains((name, false)))
									skip = true;
								else
								{
									skip = false;
									var el = GetCodeFromDictionary(name);
									root.Add(new Data(code, name));
								}
							}
							else
							//	Blocks.Add(new Block(cells[0].Slice(new Regex(@"Шаг ?\d ?\.?"), ".", true)));
							//else
								ReWrite(new[] { "Обнаружена строка, похожая на блок описания, но ключевых слов не найдено:\n", cells[0] }, new[] { c.red, c.gray });
						}
									
					} break;
					//! Строка с данными
					case > 1 when (!skip):
						if (root.Empty) 
							throw new InvalidOperationException("Не найдено описание блока"); //: Когда таблица запарсилась раньше строки "Блок #. Название блока. <...>"
						if (new Regex(@"[а-я|А-Я]*").IsMatchT(cells[codeColumnN])||cells[codeColumnN].IsNullOrWhiteSpace()) continue; //: Когда в поле код пишется что-то вроде "не передаётся"
						root[^1].Add(CreateData(cells[textColumnN], cells[codeColumnN], cells[optionalColumnN], cells[formatControlN])); //: Добавление данных в дерево
						break;
				}
			}

			return root;
		}

		static Data CreateData(string text, string code, string OptionalityText, string FormatControlText, Block Source = null)
		{
			return new Data(
				code,
				text,
				OptionalityText.Contains("-") || FormatControlText.ContainsAny(new[] { "Поле отображается", "Поле видно" })
			);
		}

		// public static string GetFileText(string filepath, string FileTypeName = "")
		// {
		// 	string filedata;
		// 	if (!File.Exists(filepath))
		// 	{
		// 		ReWrite(new []{$"\nОшибка: файл {FileTypeName} не найден", "по пути", Environment.CurrentDirectory.Add("\\")+ filepath}, new [] { c.red ,c.Default, c.blue});
		// 		return null;
		// 	}
		// 	try
		// 	{
		// 		filedata = File.ReadAllText(filepath);
		// 	}
		// 	catch (Exception e)
		// 	{
		// 		ReWrite(new []{$"\nОшибка при чтении {FileTypeName?? "файла"}: ",e.Message}, new []{c.red, c.Default});
		// 		return null;
		// 	}
		//
		// 	return filedata;
		// }

		public static Table.Block GetDictionary(string filepath) //BUG: не добавляется IpId
		{
			string filedata;
			if (!File.Exists(filepath))
			{
				ReWrite(new []{"\nОшибка: файл словаря не найден", "по пути", Environment.CurrentDirectory.Add("\\")+ filepath}, new [] { c.red ,c.Default, c.blue});
				return null;
			}
			try
			{
				filedata = File.ReadAllText(filepath);
			}
			catch (Exception e)
			{
				ReWrite(new []{"\nОшибка при чтении словаря: ",e.Message}, new []{c.red, c.Default});
				return null;
			}

			bool dataChanged = false;
			List<string> lines = filedata.RemoveAll("\r").Split("\n").ToList();
			var result = new Table.Block();
			//var resultWithRegex = new Dictionary<Regex, string>();
			for (var i = 0; i < lines.Count; i++)
			{
				if (lines[i].StartsWith("//")) continue; //: комментарий

				string[] pair = lines[i].Split("=", StringSplitOptions.RemoveEmptyEntries);
				if (pair.Length < 3)
				{
					if (pair.Length==2&&pair[0] == "!")
					{
						var skipWords = pair[1].Split(',');
						foreach (var word in skipWords) //: Можно сделать экранирование путём проверки на "\" в конце word
						{
							Program.SkipList.Add(word);
						}
					} else
					if(!lines[i].IsNullOrEmpty()) 
						ReWrite(new []{"\nОшибка",$" чтения строки словаря {i}: ",lines[i]}, new []{c.red,c.gray,c.silver});
					continue;
				}
			
				if (pair.Length != 3) ReWrite($"\nОшибка при чтении словаря в {i + 1}-й строке ({lines[i]}): должно быть 3 слова, а обнаружено {pair.Length}.");
				else if (result.GetByCode(pair[0])!=null) {lines.RemoveAt(i); dataChanged = true;}
				else result.Add(pair[0],pair[1],pair[2]=="1");
			}

			if (!dataChanged) return result;
		
			try
			{ File.OpenWrite(filepath); }
			catch (Exception e)
			{ ReWrite("Не удалось удалить дубликаты из Словаря", c.red);}

			return result;
		}

		/*internal static Regex GetRegex(string S, out int Freedom)
		{
			bool anyStart = S.StartsWith("^");
			bool anyEnd = S.EndsWith("$");
			Strict = anyStart||anyEnd
			return new Regex($"{ (anyStart? "" : "^")}{S}{(anyEnd? "" : "$")}");
		}*/
	}
}

#region Мусорка

// public static List<string[]> GetFileDocx(string filepath)
// {
// 	ReWrite("\nИдёт чтение .doc файла... ", c.purple);
// 	List<string[]> Data = new();
// 	using WordprocessingDocument doc =
// 		WordprocessingDocument.Open(filepath, false);
// 	var tables = doc.MainDocumentPart.Document.Body.Elements<DocTable>();
// 	ReWrite("Файл открыт", c.green); ReWrite("\nИдёт поиск нужной таблицы... ",c.purple);
// 	DocTable specTable = null;
// 	int[] Ns = null;
// 	int NsMax = 0;
// 	foreach (DocTable table in tables)
// 	{
// 		var Header = table.Elements<TableRow>().ElementAt(0);
// 		Ns = Header.GetMainColumnsIndexes();
// 		if (Ns == null) continue;
// 		specTable = table;
// 		break;
// 	}
// 	if(specTable==null) throw new ArgumentException("Подходящей таблицы не найдено в .docx файле");
// 	ReWrite("Нужная таблица найдена",c.green);
//
// 	NsMax = Ns.Max();
//
// 	foreach (var row in specTable.Elements<TableRow>().ToArray()[1..])
// 	{
// 		var cells = row.Elements<TableCell>().ToArray();
// 		if (cells.Length >=NsMax)
// 			Data.Add(new []{cells[Ns[0]].InnerText, cells[Ns[1]].InnerText, cells[Ns[2]].InnerText });
// 	}
//
// 	return Data;
// }

#endregion
