using System.IO;
using Titanium;
using xml_js_Parser_Updater;
using static Titanium.Forms;

namespace xml_js_Parser.Classes
{
	public static class Updater
	{

		public static async Task Update()
		{
			//var UpdaterForm = new FormUpdater();
			var updateResult = new GitHub.UpdateResult();

			try
			{
				updateResult = await GitHub.checkSoftwareUpdates(true, "github.com/TuTAH1/xml-js-Parser", "xml-js Parser.exe", () =>
				{
					bool result = MessageBox.Show("Найдена новая версия программы. Обновить? (Приложение закроется для обновления)", "Обновление найдено", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes;
			//		if (result) UpdaterForm.Show();

					return result;
				},
					TempFolder:true).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				e.ShowMessageBox("Ошибка при скачивании файла обновления");
			}

			if (updateResult.status != GitHub.Status.NoAction)
			{
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
					IO.MoveAllTo("Temp", "");
				}
				catch (Exception e)
				{
					e.ShowMessageBox("Ошибка замены файлов");
				}

			//	UpdaterForm.Close();

				MessageBox.Show("Обновление завершено");
			}


		}
		
	}
}
