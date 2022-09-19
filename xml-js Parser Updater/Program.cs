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
		static async Task Main()
		{
			//: To customize application configuration such as set high DPI settings or default font,
			//: see https://aka.ms/applicationconfiguration.

			//MessageBox.Show("test");
			ApplicationConfiguration.Initialize();
			ErrorTaskDialog.InitializeVocabulary(			
				"Открыть справку Microsoft",
				"Скопировать текст ошибки в буфер обмена", 
				"Открыть внутреннее исключение", 
				"Закрыть");
			try
			{
				await Updater.Update().ConfigureAwait(false);
			}
			catch (Exception e)
			{
				e.ShowMessageBox();
			}
		}
	}
}