using Titanium;
using xml_js_Parser.Classes;

namespace xml_js_Parser_Updater
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static async Task Main(string[] args)
		{
			//: To customize application configuration such as set high DPI settings or default font,
			//: see https://aka.ms/applicationconfiguration.

			var updateMode = args.Length == 0 ? GitHub.UpdateMode.Update :
				args.Contains("-download") ? GitHub.UpdateMode.Download :
				args.Contains("-replace") || args.Contains("-force") ? GitHub.UpdateMode.Replace :
				GitHub.UpdateMode.Update;

#if DEBUG
	updateMode = GitHub.UpdateMode.Replace;	
#endif

			//MessageBox.Show("test");
			ApplicationConfiguration.Initialize();
			ErrorTaskDialog.InitializeDictionary //: Exception messagebox initialization 
			(			
				"Открыть справку Microsoft",
				"Скопировать текст ошибки в буфер обмена", 
				"Открыть внутреннее исключение", 
				"Закрыть",
				"Ошибка"
			);
			try
			{
				var formUpdater = new FormUpdater();
				Task.Run(() => formUpdater.ShowDialog());
				await Task.Run(() => Updater.Update(formUpdater, updateMode)).ConfigureAwait(false);
				//upd.Close();
			}
			catch (Exception e)
			{
				e.ShowMessageBox();
			}
		}
	}
}