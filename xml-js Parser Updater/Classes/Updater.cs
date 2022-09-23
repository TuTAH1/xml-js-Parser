using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Ookii.Dialogs.WinForms;
using Titanium;
using xml_js_Parser_Updater;
using static Titanium.Forms;

namespace xml_js_Parser.Classes
{
	public static class Updater
	{

		public static async Task Update(FormUpdater ProgressForm, GitHub.UpdateMode UpdateMode = GitHub.UpdateMode.Update)
		{
			//var UpdaterForm = await Task.Run(() => new FormUpdater("Установка обновлений..."));
			var updateResult = new GitHub.UpdateResult();

			try
			{
				updateResult = await Task.Run(() => GitHub.checkSoftwareUpdates(UpdateMode, "github.com/TuTAH1/xml-js-Parser", "xml-js Parser.exe", () =>
				{
					bool result = MessageBox.Show("Найдена новая версия программы. Обновить? (Приложение ЗАКРОЕТСЯ для обновления)\n\n Описание обновления:\n" + updateResult.ReleaseDiscription, "Обновление найдено", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes;
					if (result) Task.Run(() => ProgressForm.ToLabel("Скачивание и распаковка обновления")).ConfigureAwait(false);

					return result;
				},
					true, new Regex("^Update"), true, true, KillRelatedProcesses:true)).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				e.ShowMessageBox("Ошибка при скачивании файла обновления");
			}

			if (updateResult.status != GitHub.Status.NoAction)
			{
				try
				{
					Task.Run(() => ProgressForm.ToLabel("Установка обновления")).ConfigureAwait(false);
				} catch (Exception){}
				try
				{
					if (updateResult.ReleaseDiscription.Contains("!Dic"))
					{
						var dicFolder = "Файлы программы\\";
						var tempDicFolder = "Temp\\Файлы программы\\";
						var dicPath = dicFolder + "Словарь.txt";
						if (File.Exists(dicPath))
							if (File.Exists(dicFolder + "Словарь.old.txt"))
								for (int i = 2; i < 5; i++)
								{
									if (File.Exists(dicFolder + $"Словарь.old{i}.txt")) i++;
									else
									{
										File.Move(dicPath, dicFolder + $"Словарь.old{i}.txt"); //: Rename
									}
								}
							else
								File.Move(dicPath, dicFolder + "Словарь.old.txt"); //: Rename
					}
				}
				catch (Exception e)
				{
					e.ShowMessageBox("Ошибка обновления словаря");
				}

				try
				{
					var procList = new List<Process>();
					var pathList = new List<string>();
					foreach (var proc in Process.GetProcesses())
					{
						try
						{
							string? procPath = proc?.MainModule?.FileName;
							if (procPath != Environment.ProcessPath
							    && procPath?.Slice(0, "\\", LastEnd: true) == Environment.CurrentDirectory
							    && proc?.MainModule?.FileVersionInfo.FileDescription == "xml-js Parser")
							{
								procList.Add(proc);
								pathList.Add(procPath);
							}
						}
						catch (Exception) {}
					
					}

					procList.ForEach(x => x.Kill()); //: Kill all processes in this folder

					IO.MoveAllTo("Temp", "", true, false, new List<Regex>(new []{new Regex(@".*\\Updater\..*")}));

					foreach (var x in pathList) 
						try { Process.Start(x); } catch (Exception){}

				}
				catch (Exception e)
				{
					e.ShowMessageBox("Ошибка замены файлов");
					Directory.Delete("Temp", true);
				}

				ProgressForm.CloseAsync();

				MessageBox.Show("Обновление завершено");
			}
		}
		
	}
}
