namespace xml_js_Parser_Updater
{
	public partial class FormUpdater : Form
	{
		public FormUpdater(string? Text = null)
		{
			InitializeComponent();
			lbStatus.Text = Text ?? "Проверка обновлений...";
		}

		public void ToLabel(string text)
		{
			if (lbStatus.IsHandleCreated)
				lbStatus.Invoke(() => lbStatus.Text = text);
			else
				lbStatus.HandleCreated += (Sender, Args) => ToLabel(text);
		}

		public void CloseAsync() => Invoke(Close);

		protected override void OnClosed(EventArgs e)
		{
			if (Directory.Exists("Temp")) Directory.Delete("Temp", true);
			base.OnClosed(e);
		}
	}
}