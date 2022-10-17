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

		public static async Task Update(FormUpdater ProgressForm, GitHub.UpdateMode? UpdateMode = null, Func<Process,bool>? KillProcIf = null)
		{
			//var UpdaterForm = await Task.Run(() => new FormUpdater("Установка обновлений..."));
			KillProcIf ??= (proc) => false;

			var updateResult = new GitHub.UpdateResult();
			var labelTask = Task.Run(() => ProgressForm.ToLabel("Чтение конигурации")).ConfigureAwait(false);
			
			List<string> IgnoreList = new(); //TODO: RegEx
			List<string> RenameList = new();

			try
			{
				try //! Чтение конфигурации
				{
					var updaterConfig = File.ReadAllText("Updater.cfg");
					// Updater.cfg
					// param: value line ; comment
					foreach (var r in updaterConfig.RemoveAll("\r").Split("\n")) {
						try {
							var param = r.Slice(0,";", true, LastEnd: false).Split(":", 2, StringSplitOptions.TrimEntries);
							if(param.Length>0 && param[0] == "") continue;
							if(param.Length!=2) throw new IndexOutOfRangeException("Неверный синтакс параметра");
							switch(param[0]) {
								case "proxy": 
									GitHub.ProxyAddress = param[1];
								break;

								case "mode":
									UpdateMode = (GitHub.UpdateMode) Enum.Parse(typeof(GitHub.UpdateMode), param[1]);
								break;
								case "ignore":
									IgnoreList.AddRange(param[1].Split(',', StringSplitOptions.RemoveEmptyEntries));
									break;
								case "rename": 
									RenameList.AddRange(param[1].Split(',', StringSplitOptions.RemoveEmptyEntries));
									break;
								default: throw new ArgumentOutOfRangeException(nameof(param), "Немзвестный параметр");
							}
						
						}
						catch (Exception e) {
							e.ShowMessageBox("Ошибка при чтении параметра");
						}

					}
				}
				catch (Exception e) 
				{
					UpdateMode = GitHub.UpdateMode.Update;
					e.ShowMessageBox("Ошибка при чтении файла конфигурации");
				}

				await labelTask;
				labelTask = Task.Run(() => ProgressForm.ToLabel("Проверка обновлений...")).ConfigureAwait(false);

				//! Проверка и скачивание обновления 
				updateResult = await Task.Run(() => GitHub.checkSoftwareUpdates((GitHub.UpdateMode)UpdateMode, "github.com/TuTAH1/xml-js-Parser", "xml-js Parser.exe", () =>
				{
					bool result = MessageBox.Show("Найдена новая версия программы. Обновить? (Приложение ЗАКРОЕТСЯ для обновления)\n\n Описание обновления:\n" + updateResult.ReleaseDiscription, "Обновление найдено", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes;
					if (result) 
						labelTask = Task.Run(() => ProgressForm.ToLabel("Скачивание и распаковка обновления")).ConfigureAwait(false);
						

					return result;
				},
					true, new Regex("^Update"), true, true, KillRelatedProcesses:true)).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				e.ShowMessageBox("Ошибка при скачивании файла обновления");
			}

			if (updateResult.status != GitHub.Status.NoAction) //! Установка обновления
			{
				try
				{
					await labelTask;
					labelTask = Task.Run(() => ProgressForm.ToLabel("Установка обновления")).ConfigureAwait(false);
				} catch (Exception){}
				try
				{ //! Перезапись словаря и создание бекапов
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

				try //! Убийство процессаов
				{
					var procList = new List<Process>();
					var pathList = new List<string>();
					foreach (var proc in Process.GetProcesses())
					{
						try
						{
							string? procPath = proc?.MainModule?.FileName;
							if (procPath != Environment.ProcessPath //: Not own process
								&& proc?.MainModule?.FileVersionInfo?.FileDescription != Process.GetProcessById(Environment.ProcessId).MainModule?.FileVersionInfo?.FileDescription
							    && KillProcIf(proc)) //: Custom conditions
							{
								procList.Add(proc);
								pathList.Add(procPath);
							}
						}
						catch (Exception) {}
					
					}

					var errorList = new List<Exception>();

					foreach (var x in procList)
						try
						{
							x.Kill();
						}
						catch (Exception e)
						{
							errorList.Add(new Exception($"Can't kill {x.ProcessName}"));
						}



					IO.MoveAllTo("Temp", "", true, false, new List<Regex>(new []{new Regex(@".*\\Updater\..*")}));

					foreach (var x in pathList) 
						try { Process.Start(x); } catch (Exception){errorList.Add(new Exception($"Can't start {x}"));}

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
