using Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Titanium;
using static xml_js_Parser.Classes.Table;
using static Titanium.Consol;

namespace xml_js_Parser.Classes
{
	internal static class Dic
	{
		public const string DictionaryPath = @"Файлы программы\Словарь.txt";
		public const string ignore = "!ignore";
		public const string skip = "!skip";
		public static List<(string, bool isCode)> SkipList = new();
		public static Table.Block Data = new();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filepath">Default value: DictionaryPath</param>
		/// <returns></returns>
		public static Block ReadFile(string filepath = null) //BUG: не добавляется IpId
		{
			string filedata;
			filepath??= DictionaryPath;
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
			//var resultWithRegex = new Dictionary<Regex, string>();
			for (var i = 0; i < lines.Count; i++)
			{
				if (lines[i].StartsWith("//")) continue; //: комментарий
				if (lines[i].IsNullOrEmpty()) continue; //: Пустая строка
				string[] pair = lines[i].Split(new Func<char,bool>[] {(ch) => ch!='\\', ch => ch == '='}).ToArray(); //! New
				if (pair.Length < 3)
				{
					if (pair.Length==2&&pair[0] == "!")
					{
						var skipWords = pair[1].Split(',');
						foreach (var word in skipWords) //: Можно сделать экранирование путём проверки на "\" в конце word
						{
							SkipList.Add((word,false));
						}
					} 
					else
						ReWrite(new []{"\nОшибка",$" чтения строки словаря {i}: ",lines[i]}, new []{c.red,c.gray,c.silver});
					continue;
				}
			
				if (pair.Length != 3) ReWrite($"\nОшибка при чтении словаря в {i + 1}-й строке ({lines[i]}): должно быть 3 слова, а обнаружено {pair.Length}.");
				else if (Data.GetByCode(pair[0])!=null) {lines.RemoveAt(i); dataChanged = true;}
				else Data.Add(pair[0],pair[1],pair[2]=="1");
			}

			if (!dataChanged) return Data;
		
			try
			{ File.OpenWrite(filepath); }
			catch (Exception e)
			{ ReWrite(new []{"Не удалось удалить дубликаты из Словаря: ", e.Message}, new[] {c.red, c.gray});}

			return Data;
		}

		public static string GetCode(string Name, bool AskIfNotFound = true) => 
			((from r in Data.rows where r.Text == Name select r).FirstOrDefault()?? (AskIfNotFound? AskCode(Name) : null)).Code;
		public static Block.TableRow GetByName(string Name, bool AskIfNotFound = true) => 
			(from r in Data.rows where r.Text == Name select r).FirstOrDefault()?? AskCode(Name);

		public static Block.TableRow AskCode(string ruName)
		{
			ReWrite(new[] { $"\nНе найдено имя переменной для ",ruName,". Напишите его: " }, new[] { Consol.c.red,Consol.c.cyan, Consol.c.white });
			
			var Code = ReadT(Placeholder: "! (удалить блок)").String();
			var curPos = GetCurPos();
			if (Code.StartsWith("!")) Code = null;
			bool optional = true;
			bool repeat = false;
			Block.TableRow resData = null;

			if (Code == null)
			{
				SkipList.Add(new(ruName, false));
			}
			else
			{
				optional = !quSwitch("\nПоле обязательно? ");
				Data.Add(Code, ruName, optional);
				resData = Data.rows.Last();
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
					ReWrite(new[] { "\nНе удалось сохранить словарь: ", e.Message + ". ", "Повторить попытку?" }, new[] { Consol.c.red, Consol.c.gray, Consol.c.Default });
					repeat = quSwitch();
				}
			} while (repeat);

			return resData;
		}

		public static string AskName(this Table.Block Dictionary, string Code)
		{
			ReWrite(new[] { $"\nНе найдено определение для ",Code,". Напишите его: " }, new[] { Consol.c.red,Consol.c.cyan, Consol.c.white });
			
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
					ReWrite(new[] { "\nНе удалось сохранить словарь: ", e.Message + ". ", "Повторить попытку?" }, new[] { Consol.c.red, Consol.c.gray, Consol.c.Default });
					repeat = quSwitch();
				}
			} while (repeat);

			return ruName;
		}
	}
}
