using Titanium;
using xml_js_Parser_Updater;
using static Titanium.Forms;

namespace xml_js_Parser.Classes
{
	public static class Updater
	{

		public static async Task Update()
		{
			var UpdaterForm = new FormUpdater();
			GitHub.Status status = GitHub.Status.NoAction;

			try
			{
				status = await GitHub.checkSoftwareUpdates(true, "github.com/TuTAH1/xml-js-Parser", "xml-js Parser.exe", () =>
				{
					bool result = MessageBox.Show("Найдена новая версия программы. Обновить? (Приложение закроется для обновления)", "Обновление найдено", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes;
					if (result) UpdaterForm.Show();

					return result;
				}).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				e.ShowMessageBox();
			}
			if (status != GitHub.Status.NoAction) UpdaterForm.Close();

		}
		
	}
}
