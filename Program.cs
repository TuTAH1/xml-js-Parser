using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AngleSharp.Attributes;
using AngleSharp.Common;
using AngleSharp.Dom;
using Application;
using Titanium;
using xml_js_Parser.Classes;
using static System.Console;
using static Application.Program;
using static xml_js_Parser.Classes.Methods;
using static Titanium.Consol;
using static Titanium.TypesFuncs;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Color = System.Drawing.Color;
using Table = xml_js_Parser.Classes.Table;

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
		public static List<string> SkipList = new();
		public static Table Table = null;
		static void Main(string[] args) {
			SetConsolePallete(DarkBlue:Color.FromArgb(9, 29, 69), DarkCyan:Color.FromArgb(9, 61, 69), Silver: Color.FromArgb(20,20,20));
			RClr();
			Clear();
			ReWrite(new []{"xml-js Parser ", "v1.3 ", "by ","Тит","ов Ив","ан"}, new []{c.lime, c.cyan, c.Default, c.lime,c.green,c.lime});
			try
			{
				string xmlObject = Name("object"); //:<object> node name

				ReWrite("\nЧтение словаря... ", c.purple, ClearLine: true);
				Table Dictionary = GetDictionary(DictionaryPath); //: Чтение словаря

				if (Dictionary.RowsCount == 0) ReWrite("Словарь пуст", c.red);
				else                       ReWrite($"Словарь с {Dictionary.RowsCount} определениями успешно прочитан", c.green, ClearLine: true);

				var xmlFile = XmlFile.Get();  //: Чтение xml файла
				var xml = xmlFile.Doc;
				var docxFilePath = xmlFile.Info.FullName.Slice(0, ".", true, true) + ".docx";
				if (!File.Exists(docxFilePath)) 
					docxFilePath = GetFilepath("docx", () => ReWrite("\nСкопируйте word-файл сюда: ", ClearLine: true));

				Table = Dictionary + new Table(GetFileDocx(docxFilePath)); //: Чтение таблицы из .txt файла и добавление её к словарю

				//TableMode = quSwitch(true, c.cyan, c.yellow, c.silver, "Таблица", "Словарь");

				ReWrite("\nСоздание дерева... ", c.purple, ClearLine: true);
				var rootNode = xml.Root.Elements(xmlObject).FirstOrDefault();
				var tree = new TreeNode<Data>(new Data(rootNode));
				if (rootNode == null) throw new ArgumentNullException(nameof(rootNode), "Корневой object не найден");
				var objBranches = tree
					.CreateChilds(new[]
					{
						new XMLData("object","code"), //:Steps
						new XMLData("object","code","name",askIfNotFound:true), //:Groups
						new XMLData("object", "code", "value", true), //:Objects
						//new TypesData("attrs"),
						//new TypesData("entry", "key", "value")
					}); //: Чтение всего xml и добавление из него элементов в дерево
				Table = null;

				if (tree.Empty) throw new ArgumentException("дерево пусто");
				else ReWrite("\nДерево успешно прочитано", c.green, ClearLine: true);

				tree.DeleteGroups(); //: удаление безымянных груп (типа Gp1, APG1 и тд)

				var js = GenerateJsCode(tree);
				saveFile(js);


				void saveFile(string Text)
				{
					var fileName = xmlFile.Info.Name.Slice(0,".", LastEnd:true)+".js";
					ReWrite("\nВведите название файла: ");
					fileName = ReadT(InputString: fileName).String();
					var filePath = xmlFile.Info.DirectoryName.Add("\\") + fileName;

					if (File.Exists(filePath))
					{
						ReWrite(new []{$"\nФайл  уже существует. ", "Перезаписать? "}, new []{c.Default,c.red});
						if (!quSwitch(null,false))
						{
							saveFile(Text); //: Если нет, ввести другое имя
							return;
						}
					}
					ReWrite("\nИдёт запись файла...", c.purple);
					File.WriteAllText(filePath,Text);
					ReWrite(new []{"\nФайл записан", " по пути ", filePath}, new []{c.lime, c.Default, c.blue});
				}

				WaitKey("выбрать другой файл");
			}
			catch (Exception e)
			{
				e.Write("перезапустить приложение");
			}

			Main(null);
		}

		public static string AskUser(this Table Table, string Code)
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
				Table.Add(Code, ruName, optional);
			}

			do
			{
				try
				{
					File.AppendAllText(DictionaryPath,  
						ruName==null?
							$"\n!={Code}" :
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

		private static string GenerateJsCode(TreeNode<Data> tree)
		{
			{
				string js =
					@"var response = JSON.parse(claimData);
var result = {""Заявление"": {}};
if(response.Order) result.Заявление = ResponseOrder(response.Order);
JSON.stringify(result);


function Docs(value, title)
{
    var result = {customNameLabel: {label:title}};
    if(Array.isArray(value))
    {
        for(var i = 0; i < value.length; i++)
        {
            result[i] = {};
            result[i].prop1 = {customNameLabel: {label: ""Тип"", value: value[i].AppliedDocument.Type}};
            result[i].prop2 = {customNameLabel: {label: ""URL"", value: value[i].AppliedDocument.URL}};
            result[i].prop3 = {customNameLabel: {label: ""Название документа"", value: value[i].AppliedDocument.Name}};
        }
    }
    else
    {
        result.prop1 = {customNameLabel: {label: ""Тип"", value: value.AppliedDocument.Type}};
        result.prop2 = {customNameLabel: {label: ""URL"", value: value.AppliedDocument.URL}};
        result.prop3 = {customNameLabel: {label: ""Название документа"", value: value.AppliedDocument.Name}};
    }
    return result;
}

function ResponseOrder()
{
    var result = {};
    result.prop1 = {customNameLabel: {label: ""Дата заявления"", value: response.statementDate}};
    result.prop2 = {customNameLabel: {label: ""Тип"", value: response.type}};
    result.prop3 = Order(response.Order, """;

				ReWrite("\nВведите тип услуги: ");
				js += ReadT(InputString: "Согласование документации").String() +
				      @""");
	return result;
}

function Order(value, title)
{
    var result = {customNameLabel: {label:title}};";

				//! function Order(value, title)
				ReWrite("\nИдёт генерация js кода...", c.purple);
				List<string> funcs = new List<string>();
				int k = 0;
				foreach (TreeNode<Data> branch in tree)
				{
					k++;
					var text = branch.Value.Text;
					var codeName = branch.Value.Code;

					js += "\n";
					if(branch.Value.Optional == true)
					{
						js += @$"	if(value.{codeName}) ";
					} else if (branch.Value.Optional!=false) ReWrite(new []{"\nНе найдена обязательность поля ", codeName}, new []{c.red,c.cyan});

					js += @$" result.prop{k} = ";
					if (branch.Empty)
						js += @$"{{customNameLabel: {{label: ""{text}"", value: value.{codeName}}}}};";
					else
					{
						//! Остальные функции
						js += @$"{codeName}(value.{codeName}, ""{text}"");";
						string func = "";
						int i = 1;
						foreach (TreeNode<Data> leave in branch)
						{
							text = leave.Value.Text;
							codeName = leave.Value.Code;
							func += "\n\t";
							if (leave.Value.Optional == true)
							{
								func += @$"if(value.{codeName}) ";
							}else if (leave.Value.Optional!=false)  ReWrite(new []{"Не найдена обязательность поля ", codeName}, new []{c.red,c.cyan});
							func += $@"result.prop{i++} = {{customNameLabel: {{label: ""{text}"", value: {codeName}}}}};";
						}

						funcs.Add(Wrap(func, codeName));
					}
				}

				js += @"
}";

				js += funcs.ToArray().ToStringT("\n");
				ReWrite("\nГенерация js кода завершена", c.green);
				return js;
			}

			string Wrap(string func, string code)
			{
				return $@"

function {code}(value,title)
{{
	var result = {{customNameLabel: {{label:title}}}};
" + func + "\n\n\treturn result;\n}";
			}
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