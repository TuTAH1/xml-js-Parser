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
		public static Table.Block Data = new Table.Block();
		public static string GetCodeFromDictionary(string name) //BUG: !=Text not working
		{
			var dic = Data;
			if (dic == null || name == null) return null;

			var dicStr = dic.rows.FirstOrDefault(row => row.Text == name)?? new Block.TableRow(null,name,null); //: Найти совпадение по имени, иначе создать TableRow
			dicStr.Code ??= Data.AskCode(name);
			return dicStr.Code;
		}

		public static string AskCode(this Table.Block Dictionary, string ruName)
		{
			ReWrite(new[] { $"\nНе найдено имя переменной для ",ruName,". Напишите его: " }, new[] { Consol.c.red,Consol.c.cyan, Consol.c.white });
			
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
					ReWrite(new[] { "\nНе удалось сохранить словарь: ", e.Message + ". ", "Повторить попытку?" }, new[] { Consol.c.red, Consol.c.gray, Consol.c.Default });
					repeat = quSwitch();
				}
			} while (repeat);

			return Code;
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
