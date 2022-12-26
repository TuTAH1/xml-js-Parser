using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanium;
using static Titanium.Consol;
using static xml_js_Parser.Classes.Methods;

namespace xml_js_Parser.Classes
{
	internal class CodeGenerator
	{
		public static string GenerateJsCode(TreeNode<Data> Tree, string ТипУслуги = null)
		{
			ReWrite("\nВведите тип услуги: ");
			string js = GenerateBasic(ТипУслуги?? ReadT(InputString: "Согласование документации").String());
			js += GenerateFuncs(Tree);
			js += GenerateDocs();
			return js;
		}

		private static string GenerateBasic(string ТипУслуги) =>
			@"var response = JSON.parse(claimData);
var result = {""Заявление"": {}};
if(response.Order) result.Заявление = ResponseOrder(response.Order);
JSON.stringify(result);

function ResponseOrder()
{
    var result = {};
    result.prop1 = {customNameLabel: {label: ""Дата заявления"", value: response.statementDate}};
    result.prop2 = {customNameLabel: {label: ""Тип"", value: response.type}};
    result.prop3 = Order(response.Order, "" "+ТипУслуги+@" "");

	return result;
}";

		private static string GenerateDocs() =>
			@"

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
}";

		private static string GenerateFuncs(TreeNode<Data> tree)
		{
			{
				string js =
					@"

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
					} else if (branch.Value.Optional!=false) 
						ReWrite(new []{"\nНе найдена обязательность поля ", codeName}, new []{c.red,c.cyan});

					js += @$"	result.prop{k} = ";
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
							var leaveText = leave.Value.Text;
							var leaveCodeName = leave.Value.Code;
							func += "\n\t";
							if (leave.Value.Optional == true)
							{
								func += @$"if(value.{leaveCodeName}) ";
							}else if (leave.Value.Optional!=false)  ReWrite(new []{"Не найдена обязательность поля ", leaveCodeName}, new []{c.red,c.cyan});
							func += $@"result.prop{i++} = {{customNameLabel: {{label: ""{leaveText}"", value: {leaveCodeName}}}}};";
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
		}

		/*private static string GenerateFuncs(Table Table)
		{
			{
				string js =
					@"

function Order(value, title)
{
    var result = {customNameLabel: {label:title}};";

				//! function Order(value, title)
				ReWrite("\nИдёт генерация js кода...", c.purple);
				List<string> funcs = new List<string>();
				int k = 0;
				foreach (Table.Block block in Table)
				{
					k++;
					var text = block.Name;
					var codeName = block.Value.Code;

					js += "\n";
					if(block.Value.Optional == true)
					{
						js += @$"	if(value.{codeName}) ";
					} else if (block.Value.Optional!=false) ReWrite(new []{"\nНе найдена обязательность поля ", codeName}, new []{c.red,c.cyan});

					js += @$" result.prop{k} = ";
					if (block.Empty)
						js += @$"{{customNameLabel: {{label: ""{text}"", value: value.{codeName}}}}};";
					else
					{
						//! Остальные функции
						js += @$"{codeName}(value.{codeName}, ""{text}"");";
						string func = "";
						int i = 1;
						foreach (TreeNode<Data> leave in block)
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
		}
		*/

		static string Wrap(string func, string code)
		{
			return $@"

function {code}(value,title)
{{
	var result = {{customNameLabel: {{label:title}}}};
" + func + "\n\n\treturn result;\n}";
		}
	}
}
