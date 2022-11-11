using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Application;
using Titanium;
using xml_js_Parser.Classes;
using static System.Console;
using static Application.Program;
using static xml_js_Parser.Classes.Methods;
using static Titanium.Consol;
using static Titanium.TypesFuncs;
using System.Linq;
using System.Reflection;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Color = System.Drawing.Color;
using Table = xml_js_Parser.Classes.Table;

// Bettercomments colors:
// 696969 grey
//:2cb02b green
//!e4e40a yellow
//\9e0d09 red
//%583b16 dark-brown
//TODO: 2693b7 blue-turquoise

namespace Application
{
	static class Program
	{
		//public static Dictionary<Regex, TableRow> Dictionary = new();
		//public static List<Regex> IgnoreList = new();
		//public static List<Regex> SkipList = new();
		public const string DictionaryPath = @"Файлы программы\Словарь.txt";
		public const string ignore = "!ignore";
		public const string skip = "!skip";
		public static List<(string, bool isCode)> SkipList = new();
		public static Table.Block Dictionary = new Table.Block();


		public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
		public static Version DicVer = new Version(1, 3); //: Dictionary version (версия словаря); Должно использоваться в Updater

		static void Main(string[] args)
		{
			SetConsolePallete(DarkBlue: Color.FromArgb(9, 29, 69), DarkCyan: Color.FromArgb(9, 61, 69), Silver: Color.FromArgb(20, 20, 20));
			Process.Start("Updater.exe");

			while (true)
			{
				RClr();
				Clear();
				ReWrite(new[] { "xml-js Parser ", $"v{Version} ", "by ", "Тит", "ов Ив", "ан" }, new[] { c.lime, c.cyan, c.Default, c.lime, c.green, c.lime });
				try //TODO: автоматическй выбор тип парсинга, в зависимости от расширения файла
				{
					//ReWrite("Выберите тип парсинга: \n");
#if DEBUG
					Parsing(ParsingType.docx);
#else
					Parsing((ParsingType)Menu( ((ParsingType[])Enum.GetValues(typeof(ParsingType))).ToArray(
								Type => new Option(GetTypeName(Type), Type.ToString())
								)));
#endif
					WaitKey("выбрать другой файл");
				}
				catch (Exception e)
				{
					e.Write("перезапустить приложение");
				}
			}
		}

		enum ParsingType
		{
			docx,
			xsl
		}

		private static string GetTypeName(ParsingType type) => type switch
		{
			ParsingType.docx => "ТЗ (.docx)",
			ParsingType.xsl => ".xsl",
		//	ParsingType.xml => ".xml (не рекомендуется)",
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};

		private static void Parsing(ParsingType? parsingType)
		{
			ReWrite("\nЧтение словаря... ", c.purple, ClearLine: true);
			Table.Block FileDictionary = GetDictionary(DictionaryPath); //: Чтение словаря

			if (FileDictionary.RowsCount == 0) ReWrite("Словарь пуст", c.red);
			else ReWrite($"Словарь с {FileDictionary.RowsCount} определениями успешно прочитан", c.green, ClearLine: true);
			Dictionary.Append(FileDictionary);

			//TableMode = quSwitch(true, c.cyan, c.yellow, c.silver, "Таблица", "Словарь");
			TreeNode<Data> tree = null;
			var supportedExtensions = new[] { "docx" };
			string filePath = GetFilepath(supportedExtensions, () => ReWrite($"\nСкопируйте файл ({supportedExtensions.ToReadableString(Separator: ", .", " или .")}) сюда: ", ClearLine: true));
		}

	private static void Parsing(ParsingType parsingType)
		{
			
			ReWrite("\nЧтение словаря... ", c.purple, ClearLine: true);
			Table.Block FileDictionary = GetDictionary(DictionaryPath); //: Чтение словаря

			if (FileDictionary.RowsCount == 0) ReWrite("Словарь пуст", c.red);
			else ReWrite($"Словарь с {FileDictionary.RowsCount} определениями успешно прочитан", c.green, ClearLine: true);
			Dictionary.Append(FileDictionary);

			//TableMode = quSwitch(true, c.cyan, c.yellow, c.silver, "Таблица", "Словарь");
			TreeNode<Data> tree = null;
			string filePath = null;

			switch (parsingType)
			{
				case ParsingType.docx:
				{
					ReWrite("\nСоздание дерева... ", c.purple, ClearLine: true);

					var docxFilePath =
#if DEBUG
						@"C:\Users\iati\Desktop\jsация\10. Предоставление права пользования участками недр местного значения\ЧТЗ Предоставление права пользования участками недр местного значения .docx";				
#else
						GetFilepath("docx", () => ReWrite("\nСкопируйте word-файл сюда: ", ClearLine: true));
#endif
					filePath = docxFilePath.Slice(0, ".");
					tree = CreateTree(GetDocxTable(docxFilePath));

				} break;
				case ParsingType.xsl:
				{
					throw new NotImplementedException("xsl парсинг ещё не реализован");
				} break;
				default:
					throw new ArgumentOutOfRangeException(nameof(parsingType), parsingType, null);
			}

			

			var js = CodeGenerator.GenerateJsCode(tree);
			saveFile(js);


			void saveFile(string Text)
			{
				var fileName = filePath.Slice("\\",int.MaxValue, LastStart:true, LastEnd: true) + ".js";
				ReWrite("\nВведите название файла: ");
				fileName = ReadT(InputString: fileName).String();
				filePath = filePath.Slice(0,"\\",false,true,true) + fileName;

				if (File.Exists(filePath))
				{
					ReWrite(new[] { $"\nФайл  уже существует. ", "Перезаписать? " }, new[] { c.Default, c.red });
					if (!quSwitch(null, false))
					{
						saveFile(Text); //: Если нет, ввести другое имя
						return;
					}
				}

				ReWrite("\nИдёт запись файла...", c.purple);
				File.WriteAllText(filePath, Text);
				ReWrite(new[] { "\nФайл записан", " по пути ", filePath }, new[] { c.lime, c.Default, c.blue });
			}
		}

		public static string AskCode(this Table.Block Dictionary, string ruName)
		{
			ReWrite(new[] { $"\nНе найдено имя переменной для ",ruName,". Напишите его: " }, new[] { c.red,c.cyan, c.white });
			
			var Code = ReadT(Placeholder: "! (удалить блок)").String();
			var curPos = GetCurPos();
			if (Code.StartsWith("!")) Code = null;
			bool optional = true;
			bool repeat = false;
			if (Code != null)
			{
				optional = !quSwitch("\nПоле обязательно? ");
				Dictionary.Add(Code, ruName, optional);
			}

			do
			{
				try
				{
					File.AppendAllText(DictionaryPath,  
						Code==null?
							$"\n!={ruName}" :
							$"\n{Code}={ruName}={(optional? "1" : "0")}");
				}
				catch (Exception e)
				{
					ReWrite(new[] { "\nНе удалось сохранить словарь: ", e.Message + ". ", "Повторить попытку?" }, new[] { c.red, c.gray, c.Default });
					repeat = quSwitch();
				}
			} while (repeat);

			return Code;
		}

		public static string AskName(this Table.Block Dictionary, string Code)
		{
			ReWrite(new[] { $"\nНе найдено определение для ",Code,". Напишите его: " }, new[] { c.red,c.cyan, c.white });
			
			var ruName = ReadT(Placeholder: "! (Разбить группу)").String();
			var curPos = GetCurPos();
			if (ruName.StartsWith("!")) ruName = null;
			bool optional = true;
			bool repeat = false;
			if (ruName != null)
			{
				optional = !quSwitch("\nПоле обязательно? ");
				Dictionary.Add(Code, ruName, optional);
			}

			do
			{
				try
				{
					File.AppendAllText(DictionaryPath,  
						ruName==null?
							$"\n{Code}=!" :
							$"\n{Code}={ruName}={(optional? "1" : "0")}");
				}
				catch (Exception e)
				{
					ReWrite(new[] { "\nНе удалось сохранить словарь: ", e.Message + ". ", "Повторить попытку?" }, new[] { c.red, c.gray, c.Default });
					repeat = quSwitch();
				}
			} while (repeat);

			return ruName;
		}
	}
}
#region Мусорка

/*foreach (var stepNode in rootNode.Elements(xmlObject)) 
{
	foreach (var group in stepNode.Elements(xmlObject))
	{
		var groupName = group.Element(xmlCode).Value;
		if (IgnoreList.Contains(groupName)) continue;

		var groupTree = tree.Add(groupName);

		int i = 0;
		foreach (var obj in group.Elements(xmlObject))
		{
			i++;
			var objName = obj.Element(xmlCode)?.Value;
			var objValue = obj.Element(xmlValue)?.Value;
			if(objName == null || objValue == null) {ReWrite($"\nОшибка: Не найдено {(objValue == null? objValue == null? "имени и значения" : "имени" : "значения")} {i}-го объекта группы {groupName}", c.red); continue;}
			if (IgnoreList.Contains(objName)) continue;
			var objTree = groupTree.Add(objName,objValue);

			foreach (var entry in obj.Elements(xmlEntry))
			{
				var key = entry.Element(xmlKey)?.Value;
				var value = entry.Element(xmlValue)?.Value;
				if (key == null && value == null) {ReWrite($"\nОшибка: в {i}й entry объекта {groupName} нет {(key == null? value == null? "ключа и значения" : "ключа" : "значения")}", c.red); continue;}
				objTree.Add(key, value);
			}
		}
	}
}*/

/*
					int i = 0;
					foreach (var obj in group.Elements(xmlObject))
					{
						i++;
						var objName = obj.Element(xmlCode)?.Value;
						var objValue = obj.Element(xmlValue)?.Value;

						if (objName == null || objValue == null)
						{
							var critical = objName == null && objValue == null;
							ReWrite($"\nНе найдено {(objName == null? objValue == null? "имени и значения" : "имени" : "значения")} {i}-го объекта группы {groupName}", critical? c.red : c.gray);
							if(critical) continue;
						}
						if (IgnoreList.Contains(objName)) continue;
						var objTree = groupTree.Add(objName,objValue);

						foreach (var subObj in obj.Elements(xmlEntry))
						{
							var subObjName = obj.Element(xmlCode)?.Value;
							var subObjValue = obj.Element(xmlValue)?.Value;

							if (subObjName == null || subObjValue == null)
							{
								var critical = subObjName == null && subObjValue == null;
								ReWrite($"\nНе найдено {(subObjName == null? subObjValue == null? "имени и значения" : "имени" : "значения")} {i}-го субъекта объекта {groupName}", critical?c.red : c.gray);
								if(critical) continue;
							}
							if (IgnoreList.Contains(objName)) continue;
							var subObjTree = groupTree.Add(objName,objValue);

							foreach (var entry in obj.Elements(xmlEntry)) //! Проверить
							{
								var key = entry.Element(xmlKey)?.Value;
								var value = entry.Element(xmlValue)?.Value;
								if (key == null || value == null)
								{
									var critical = key == null && value == null;
									ReWrite($"\nВ {i}й entry объекта {groupName} нет {(key == null? value == null? "ключа и значения" : "ключа" : "значения")}", critical?c.red : c.yellow, c.gray);
									if(critical) continue;
								}
								subObj.Add(key, value);
							}
						}
					}*/
/*	/// <summary>
	/// Создаёт потомков указанноо Tree на основе его Value.xml.Elements(Type), и помещает его в TargetTree (или в Tree, если не указано)
	/// </summary>
	/// <param name="Tree">Дерево-источник</param>
	/// <param name="Type"></param>
	/// <param name="NameType"></param>
	/// <param name="ValueType"></param>
	/// <param name="TargetTree">Дерево, куда помещаются найденные потомки (по умолчанию, Tree)</param>
	/// <returns>Tree.Childs</returns>
	public static IEnumerable<TreeNode<Data>> CreateChilds(this TreeNode<Data> Tree, string Type, string? NameType = null, string? ValueType = null, TreeNode<Data>? TargetTree = null, bool Recursive = false)
	{
		//if (Node == null) {ReWrite($"\nОшибка: элемент {Tree.Name} не существует", c.red); return;}
		var TreeName = Tree.Value.Name;
		var Nodes = Tree.Value.xml.Elements(Name(Type));
		if(!Nodes.Any()) {ReWrite($"\nЭлемент {TreeName} не содержит потомков", c.gray); return new []{Tree};}
		
		int i = 0;
		foreach (var obj in Nodes)
		{
			i++;
			var objName = NameType == null? null : obj.Element(Name(NameType))?.Value;
			var objValue = ValueType == null? null : obj.Element(Name(ValueType))?.Value;

			if (objName == null || objValue == null)
			{
				var critical = objName == null && objValue == null;
				ReWrite($"\nНе найдено {(objName == null? objValue == null? "имени и значения" : "имени" : "значения")} {i}-го объекта группы {TreeName}", critical? c.red : c.gray);
				if(critical) continue;
			}
			if (IgnoreList.IsMatchAny(objName)) continue;

			var leave = new TreeNode<Data>(new Data(obj, objName, objValue));
			if (SkipList.IsMatchAny(objName))
			{
				leave.CreateChilds(NameType ?? ValueType, NameType, ValueType, Tree); //: Универсальнее было бы ставить метку на удаление (узла без потомков)
				Tree.Add(leave.Childs);
			}
			else
				Tree.Add(leave);
		}

		//if (Recursive) return Nodes.ToList().ForEach(x => x.Elements(Type).CreateTree(Tr))
		return Tree.Childs;
	}*/

#endregion