namespace xml_js_Parser_Updater
{
	public partial class FormUpdater : Form
	{
		public FormUpdater(string? Text = null)
		{
			InitializeComponent();
			lbStatus.Text = Text ?? "�������� ����������...";
		}

		public void ToLabel(string text) => lbStatus.Invoke(() => lbStatus.Text = text);

		public void CloseAsync() => Invoke(Close);
	}
}