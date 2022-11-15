using Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
