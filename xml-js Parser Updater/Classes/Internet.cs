using System;
using System.Net; //для работы с интернетом
using System.Text;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Io;
using AngleSharp.Io.Network;
using static Titanium.Classes;

namespace Titanium
{
	public class Internet
	{
		public static string getResponseString(string urlAddress)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
			request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:85.0) Gecko/20100101 Firefox/85.0");
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				Stream receiveStream = response.GetResponseStream();
				StreamReader readStream = null;

				if (response.CharacterSet == null)
				{
					readStream = new StreamReader(receiveStream);
				}
				else
				{
					readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
				}

				string data = readStream.ReadToEnd();

				receiveStream.Close();
				response.Close();
				readStream.Close();

				return data;
			}

			throw new Exception("bad url");
		}

		public static async Task<IDocument> getResponseAsync(string urlAddress)
		{
			var requester = new DefaultHttpRequester("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:99.0) Gecko/20100101 Firefox/99.0");
			var config = new Configuration().WithDefaultLoader().With(requester);
			var document = await BrowsingContext.New(config).OpenAsync(urlAddress).ConfigureAwait(false);
			return document;
		}

		/*public static async Task<IDocument> getResponseAsync(string urlAddress)
		{
			ServicePointManager.SecurityProtocol |=
				SecurityProtocolType.Tls12 | 
				SecurityProtocolType.Tls11 | 
				SecurityProtocolType.Tls;
			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.812");
			var requester = new HttpClientRequester(client);
			var config = Configuration.Default.WithRequester(requester).WithDefaultLoader();
			var context = BrowsingContext.New(config);
			var document = await context.OpenAsync(urlAddress);
			return document;
		}*/

		/*
		public static async Task<IDocument> getResponseAS(string urlAddress)
		{
			var requester = new DefaultHttpRequester("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:69.0) Gecko/20100101 Firefox/69.0");
			var config = Configuration.Default.WithDefaultLoader().With(requester);
			var document = await BrowsingContext.New(config).OpenAsync(urlAddress);
			return document;
		}*/

		/*static void getHTMLfile(string urlAdress, string filename)
		{
			File.WriteAllText(
				filename,
				getResponse(urlAdress),
				System.Text.Encoding.UTF8
			);
		}*/
	}

	public static class GitHub
	{
		public enum Status
		{
			Downloaded,
			Updated,
			NoAction
		}

		/// <summary>
		/// Updates the exe in specified paths from GitHub releases page
		/// </summary>
		/// <param name="CheckUpdates">if false, it doesn't checkUpdates if exe file is already exist</param>
		/// <param name="repositoryLink">GitHub repository link from where updates will be downloaded. Example: github.com/ItsKaitlyn03/OculusKiller</param>
		/// <param name="FilePath">Path of physical exe file that should be updated</param>
		/// <param name="Unpack">Should archives be unpacked while placing in </param>
		/// <param name="GitHubFilenameRegex">regex of the filename of the release</param>
		/// <param name="TempFolder">Leave GitHub release files in ./Temp. Don't Forget to DELETE TEMP folder after performing needed operations</param>
		/// <param name="ArchiveIgnoreFileList">List of files that shouldn't extracted from downloaded archive. If null, all files will be extracted</param>
		/// <param name="ReverseArchiveFileList"></param>
		/// <param name="UpdateQuestion">Function that will be executed if update found. If this function will return false, update will be canceled</param>
		/// <returns></returns>
		public static async Task<Status> checkSoftwareUpdates(bool CheckUpdates, string repositoryLink, string FilePath, Func<bool> UpdateQuestion = default, bool Unpack = true, Regex GitHubFilenameRegex = null, bool TempFolder = false, Regex[] ArchiveIgnoreFileList = null, bool ReverseArchiveFileList = false, bool KillrelatedProcesses = false)
		
		{
			string releasesPage = "https://".Add(repositoryLink).Add("/releases");
			
			var doc = await Internet.getResponseAsync(releasesPage).ConfigureAwait(false);
			if (doc == null) throw new NullReferenceException("can't get response from page " + releasesPage);
			if (doc.Body.Children.Length == 0) throw new NullReferenceException("Невозможно получить доступ к странице. Возможно, отсутствует подключение к Интернету");
			var lastestReleaseBlock = doc.QuerySelector("div[data-hpc]")?.FirstElementChild;
			if (lastestReleaseBlock == null) throw new NullReferenceException("Can't get releases from page " + releasesPage);

			if (!File.Exists(FilePath))
			{
				await DownloadLastest().ConfigureAwait(false);
				return Status.Downloaded;
			}
			else if (CheckUpdates)
			{
				var lastestVersion = lastestReleaseBlock.QuerySelector(".ml-1.wb-break-all")?.Text();
				if (lastestVersion is null) 
					throw new ArgumentNullException(nameof(lastestVersion), "Can't get lastest version");

				var currentVersion = FileVersionInfo.GetVersionInfo(FilePath);
				if (currentVersion is null) 
					throw new InvalidOperationException("Product version field is empty");

				lastestVersion = new Regex("[^.0-9]").Replace(lastestVersion, "");

				/*MessageBox.Show($"Lastest version: {lastestVersion};" +
				                $"\nParsed: {Version.Parse(lastestVersion)}" +
				                $"\n Current version: {currentVersion.ProductVersion}" +
				                $"\n Parsed: {Version.Parse(currentVersion.ProductVersion)}");*/


				//:If current file's version is lower than in github, download lastest from github
				if (Version.Parse(lastestVersion) > Version.Parse(currentVersion.ProductVersion))
				{
					if (UpdateQuestion == default || !UpdateQuestion()) 
						return Status.NoAction;
					await DownloadLastest().ConfigureAwait(false);
					return Status.Updated;
				}
			}
			return Status.NoAction;

			async Task<bool> DownloadLastest()
			{
				var allAssets = lastestReleaseBlock.QuerySelector(".mb-3 > details").QuerySelectorAll("ul > li");
				var gitHubFiles = (
					from fileBlock in allAssets
					let fileNameBlock = fileBlock.FirstElementChild.LastElementChild
					let metadataGroup = fileBlock.LastElementChild
					select new GitHubFile(
						fileNameBlock.TextContent.RemoveAllFrom("\n\t\r "),
						fileNameBlock.Attributes["href"].TextContent,
						metadataGroup.FirstElementChild.TextContent,
						metadataGroup.LastElementChild.TextContent
					)
				).ToList();

				if (!gitHubFiles.Any()) throw new ArgumentNullException(nameof(gitHubFiles),"No any files found in the release");

				gitHubFiles = (
					from file in gitHubFiles 
					where (GitHubFilenameRegex?.IsMatch(file.Name) ?? true) && file.Name != "Source code" //: Select all files aliased with GitHubFilename regex 
					select file).ToList();

				if (!gitHubFiles.Any()) throw new ArgumentNullException(nameof(gitHubFiles),GitHubFilenameRegex==null? "Nothing but source code files found in the release" : $"No files matching \"{GitHubFilenameRegex}\" found in the release");

				foreach (var file in gitHubFiles)
				{
					string filepath = $"Temp/{file.Name}";

					using var client = new HttpClient();
					var s = await client.GetStreamAsync("https://github.com/"+file.Link);
					try {Directory.Delete("Temp", true); }
					catch (Exception) {}
					Directory.CreateDirectory("Temp");
					var fs = new FileStream(filepath, FileMode.OpenOrCreate);
					s.CopyTo(fs); //TODO: may be done async
					
					Unpack = Unpack && new FileInfo(filepath).Extension == ".zip";
					if (Unpack)
					{
						var archive = new ZipArchive(fs, ZipArchiveMode.Read, false);
						foreach (var entry in archive.Entries)
						{
							var entryPath = entry.FullName;
							var entryName = entryPath.Slice("\\", LastStart: true);

							if (KillrelatedProcesses && entryName.EndsWith(".exe"))
								(
									from proc in Process.GetProcessesByName(entryName)
									where proc.MainModule.FileName == System.AppContext.BaseDirectory + entryName
									select proc
								).ToList().ForEach(p => p.Kill());

							entry.ExtractToFile(entryPath, true);
						}
					}
				}
				

				//new WebClient().DownloadFile(releasesPage, "OculusDash.exe");
				return true;
			}

			
		}

		class GitHubFile
		{
			public string Name;
			public string Link;
			public FileSize? Size;
			public DateTime? Date;

			public GitHubFile(string Name, string Link, string Size, string Date)
			{
				this.Name = Name;
				this.Link = Link;
				this.Size = FileSize.Get(Size);
				try 
				{ this.Date = Convert.ToDateTime(Date); }
				catch (Exception) 
				{ this.Date = null; }
				
			}
		}
	}


	#region Garbage
	//public static string getResponse(string urlAddress)
	//{
	//	HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
	//	request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:85.0) Gecko/20100101 Firefox/85.0");
	//	HttpWebResponse response = (HttpWebResponse)request.GetResponse();

	//	if (response.StatusCode == HttpStatusCode.OK)
	//	{
	//		Stream receiveStream = response.GetResponseStream();
	//		StreamReader readStream = null;

	//		if (response.CharacterSet == null)
	//		{
	//			readStream = new StreamReader(receiveStream);
	//		}
	//		else
	//		{
	//			readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
	//		}

	//		string data = readStream.ReadToEnd();

	//		receiveStream.Close();
	//		response.Close();
	//		readStream.Close();
	//		return data;
	//	}

	//	throw new Exception("bad url");
	//}

	//public static void getHWlinks(string urlAddress, string HWname)
	//{
	//	using (IWebDriver driver = new FirefoxDriver())
	//	{
	//		IReadOnlyCollection<IWebElement> HWs;
	//		driver.Navigate().GoToUrl(urlAddress);
	//		driver.Manage().Timeouts().ImplicitWait = new TimeSpan(30);
	//		driver.Manage().Timeouts().AsynchronousJavaScript = new TimeSpan(30);
	//		ForegroundColor = ConsoleColor.Gray;
	//		Clear();
	//		WindowWidth = 110;
	//		ForegroundColor = ConsoleColor.Yellow; WriteLine($"Идёт получение ссылок {HWname} \n");
	//		short added = 0;
	//		CursorTop = 3;
	//		CursorLeft = 0;
	//		ForegroundColor = ConsoleColor.Green; Write("Всего добавлено:");
	//		try
	//		{
	//			while (true) driver.FindElement(By.ClassName("pull-right")).Click();
	//		}
	//		catch { }
	//		driver.Manage().Timeouts().ImplicitWait = new TimeSpan(1);
	//		driver.Manage().Timeouts().AsynchronousJavaScript = new TimeSpan(1);
	//		while (true)
	//		{
	//			CursorTop = 2;
	//			CursorLeft = 0;
	//			ForegroundColor = ConsoleColor.Cyan; Write("[                                                                                                    ]");
	//			//driver.FindElement(By.CssSelector(
	//			//		"a[onclick=\"window.location.href=updateQueryStringParameter(window.location.href,'sort','Newest');\"]"))
	//			//	.Click();
	//			CursorTop = 1;
	//			CursorLeft = 0;
	//			ForegroundColor = ConsoleColor.DarkCyan; Write(driver.FindElement(By.CssSelector("ul[class=\"pagination pagination-lg\"]>li[class=\"disabled\"]>a")).Text);

	//			HWs = driver.FindElements(By.ClassName("tl-tag"));

	//			CursorLeft = 1;
	//			CursorTop = 2;
	//			foreach (var HW in HWs)
	//			{
	//				if (Titanium.String.CountDigitsDouble(HW
	//					.FindElement(By.XPath("//*[contains(text(), 'User benchmarks')]")).Text) > 2)
	//				{
	//					File.AppendAllText(HWname + ".txt", HW.GetAttribute("href"), Encoding.UTF8);
	//					int c = CursorLeft;
	//					CursorLeft = 11;
	//					CursorTop = 3;
	//					Write(++added);
	//					CursorTop = 2;
	//					CursorLeft = c;
	//				}

	//				ForegroundColor = ConsoleColor.Cyan; Write("-");
	//			}
	//			Write("]\n");

	//			var nextButton = driver.FindElement(By.Id("searchForm:j_idt80"));
	//			if (nextButton.Enabled) nextButton.Click();
	//			else break;
	//		}

	//	}
	//}

	//public static string getHWHTML(string urlAddress, string HWname)
	//{
	//	using (IWebDriver driver = new FirefoxDriver())
	//	{
	//		IReadOnlyCollection<IWebElement> HWs;
	//		driver.Navigate().GoToUrl(urlAddress);
	//		try
	//		{
	//			while (true) driver.FindElement(By.ClassName("pull-right")).Click();
	//		}
	//		catch { }

	//		for (int i = 0; ; i++)
	//		{
	//			//File.AppendAllText(HWname+i+".Html",
	//			//driver.PageSource);
	//			IBrowsingContext browsing = new BrowsingContext();
	//			browsing.OpenAsync(driver.PageSource);

	//			var nextButton = driver.FindElement(By.Id("searchForm:j_idt80"));
	//			if (nextButton.Enabled) nextButton.Click();
	//		}

	//	}
	//}
	#endregion
}
