using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using xml_js_Parser.Classes;

namespace Titanium
{
	internal class Logger
	{
		private string _fullPath;
		private string _path;
		private string _filename;
		private bool _writeDate;
		private StreamWriter _sw;

		public Logger(string FilePath, string Filename, bool Overwrite = false, bool WriteDate = true)
		{
			_path = FilePath;
			_filename = Filename;
			_fullPath = (Path.Combine(_path, _filename) + (WriteDate? DateTime.Now.ToString("yy-mm-dd_HH-mm-ss") : "" )).Add(".log");

			if (!Overwrite && File.Exists(_fullPath))
			{
				_sw = new StreamWriter(_fullPath, new FileStreamOptions { Mode = FileMode.Append });
				_sw.WriteLine($"Launched\nDate: {DateTime.Now}");
			}
			else
				_sw = new StreamWriter(_fullPath, new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write});
			
			_sw.AutoFlush = true;
				
			_writeDate = WriteDate;
		}
		
		public void Log(string Message) => _sw.WriteLine(Message);

		/*internal void Log(Table.Block dic)
		{
			foreach (var row in dic)
			{
				
			}
		}*/
	}
}
