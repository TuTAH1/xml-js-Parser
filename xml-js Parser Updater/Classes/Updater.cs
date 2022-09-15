using Titanium;
using xml_js_Parser_Updater;

namespace xml_js_Parser.Classes
{
	public static class Updater
	{

		public static async Task Update()
		{
			//var UpdaterForm = new FormUpdater();

			var status = await GitHub.checkSoftwareUpdates(true, "https://github.com/TuTAH1/xml-js-Parser", "xml-js Parser.exe", () =>
			{
				bool result = MessageBox.Show("Найдена новая версия программы. Обновить? (Приложение закроется для обновления)", "Обновление найдено", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes;
			//	if (result) UpdaterForm.Show();

				return result;
			});

			//if (status != GitHub.Status.NoAction) UpdaterForm.Close();

		}
		
	}
}
