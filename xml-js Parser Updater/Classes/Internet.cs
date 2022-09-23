using System;
using System.Net; //для работы с интернетом
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Io;
using AngleSharp.Io.Network;
using ICSharpCode.SharpZipLib.Zip;
using Octokit;
using static Titanium.Classes;
using FileMode = System.IO.FileMode;

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

		public class UpdateResult
		{
			public Status status;
			public string? ReleaseDiscription { get; private set; }
			public string? ReleaseName { get; private set; }
			public Version? Version { get; private set; }

			public UpdateResult(Status Status = Status.NoAction, Version? Version = null, string? ReleaseName = null, string? ReleaseDiscription = null)
			{
				status = Status;
				this.ReleaseDiscription = ReleaseDiscription;
				this.ReleaseName = ReleaseName;
				this.Version = Version;
			}

			public UpdateResult Change(Status? Status = null, Version? Version = null, string? ReleaseName = null, string? ReleaseDiscription = null)
			{
				if (Status!=null) status = (Status)Status;
				if (ReleaseDiscription!=null) this.ReleaseDiscription = ReleaseDiscription;
				if (ReleaseName!=null) this.ReleaseName = ReleaseName;
				if (Version!=null) this.Version = Version;
				return this;
			}
		}

		
		public enum UpdateMode
		{
			/// <summary> Download programm if it's not exist, but don't update it if it's exist </summary>
			Download,
			/// <summary> Only checks a new version, but not update it. </summary>
			Check,
			/// <summary> Update programm only if it's not installed or older version is installed </summary>
			Update,
			/// <summary> Update programm even if the newer version is installed </summary>
			Replace
		}

		/// <summary>
		/// Updates the exe in specified paths from GitHub releases page
		/// </summary>
		/// <param name="repositoryLink">GitHub repository link from where updates will be downloaded. In any format from "https://github.com/TuTAH1/xml-js-Parser/releases/tag/1.2.0" to just "TuTAH1/xml-js-Parser" (both variants will give the same result)</param>
		/// <param name="ProgramExePath">Path of physical exe file that should be updated</param>
		/// <param name="Unpack">Should archives be unpacked while placing in </param>
		/// <param name="GitHubFilenameRegex">regex of the filename of the release</param>
		/// <param name="TempFolder">Leave GitHub release files in ./Temp. Don't Forget to DELETE TEMP folder after performing needed operations</param>
		/// <param name="ArchiveIgnoreFileList">List of files that shouldn't extracted from downloaded archive. If null, all files will be extracted</param>
		/// <param name="ReverseArchiveFileList">Turns Blacklist into whitelist if true</param>
		/// <param name="UpdateQuestion">Function that will be executed if update found. If this function will return false, update will be canceled</param>
		/// <returns></returns>
		public static async Task<UpdateResult> checkSoftwareUpdates(UpdateMode UpdateMode, string repositoryLink, string ProgramExePath, Func<bool> UpdateQuestion = default, bool Unpack = true, Regex? GitHubFilenameRegex = null, bool ReverseGithubFilenameRegex = false, bool TempFolder = false, Regex[] ArchiveIgnoreFileList = null, bool ReverseArchiveFileList = false, bool KillRelatedProcesses = false)
		{
			string[] ss = repositoryLink.RemoveFrom(TypesFuncs.Side.Start, "https://", "github.com/").Split("/");
			if (ss.Length < 2) throw new ArgumentException("Can't get username and repName from " + repositoryLink);
			return await checkSoftwareUpdates(UpdateMode, ss[0], ss[1], ProgramExePath, UpdateQuestion, Unpack, GitHubFilenameRegex,ReverseGithubFilenameRegex, TempFolder, ArchiveIgnoreFileList, ReverseArchiveFileList, KillRelatedProcesses);
		}

		/// <summary>
		/// Updates the exe in specified paths from GitHub releases page
		/// </summary>
		/// /// <param name="author">Repository author id (example: TuTAH1)</param>
		/// <param name="repName">Repository name (example: SteamVR-OculusDash-Switcher)</param>
		/// <param name="ProgramExePath">Path of physical exe file that should be updated</param>
		/// <param name="Unpack">Should archives be unpacked while placing in </param>
		/// <param name="GitHubFilenameRegex">regex of the filename of the release</param>
		/// <param name="TempFolder">Leave GitHub release files in ./Temp. Don't Forget to DELETE TEMP folder after performing needed operations</param>
		/// <param name="ArchiveIgnoreFileList">List of files that shouldn't extracted from downloaded archive. If null, all files will be extracted</param>
		/// <param name="ReverseArchiveFileList">Turns Blacklist into whitelist if true</param>
		/// <param name="UpdateQuestion">Function that will be executed if update found. If this function will return false, update will be canceled</param>
		/// <returns></returns>
		public static async Task<UpdateResult> checkSoftwareUpdates(UpdateMode UpdateMode, string author, string repName, string ProgramExePath, Func<bool> UpdateQuestion = default, bool Unpack = true, Regex? GitHubFilenameRegex = null, bool ReverseGithubFilenameRegex = false,  bool TempFolder = false, Regex[] ArchiveIgnoreFileList = null, bool ReverseArchiveFileList = false, bool? KillRelatedProcesses = false)

		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			KillRelatedProcesses ??= !TempFolder;

			var github = new GitHubClient(new ProductHeaderValue("Titanium-GithubSoftwareUpdater"));
			var release = await github.Repository.Release.GetLatest(author, repName).ConfigureAwait(false);
			Version? relVersion = null;
			try
			{
				relVersion = Version.Parse(new Regex("[^.0-9]").Replace(release.TagName, ""));
			} catch (Exception) { }
			UpdateResult result = new(Status.NoAction, relVersion, release.Name, release.Body);
			
			bool fileExist = File.Exists(ProgramExePath);

			if (UpdateMode!= UpdateMode.Check && !fileExist || UpdateMode == UpdateMode.Replace)
			{
				await DownloadLastest().ConfigureAwait(false);
				return result.Change(Status.Downloaded);
			}
			else if (UpdateMode is UpdateMode.Update or UpdateMode.Check)
			{
				var currentVersion = FileVersionInfo.GetVersionInfo(ProgramExePath);
				if (currentVersion is null) 
					throw new InvalidOperationException("Product version field is empty");

				//:If current file's version is higher than in github, don't do anything
				if (UpdateMode == UpdateMode.Check || relVersion <= Version.Parse(currentVersion.ProductVersion!)) return result;

				if (UpdateQuestion == default || !UpdateQuestion()) 
					return result;
				await DownloadLastest().ConfigureAwait(false);
				return result.Change(Status.Updated);
			}
			return result;

			async Task<bool> DownloadLastest()
			{

				var gitHubFiles = release.Assets;

				if (!gitHubFiles.Any()) throw new ArgumentNullException(nameof(gitHubFiles),"No any files found in the release");

				gitHubFiles = (
					from file in gitHubFiles 
					where (GitHubFilenameRegex?.IsMatch(file.Name) ^ ReverseGithubFilenameRegex ??  true) //: Select all files aliased with GitHubFilename regex 
					select file).ToList();

				if (!gitHubFiles.Any()) throw new ArgumentNullException(nameof(gitHubFiles),GitHubFilenameRegex==null? "No files found in the release" : $"No files matching \"{GitHubFilenameRegex}\" found in the release");

				foreach (var file in gitHubFiles)
				{
					string filepath = $"Temp\\{file.Name}";

					using var client = new HttpClient();
					var s = await client.GetStreamAsync(file.BrowserDownloadUrl).ConfigureAwait(false);
					try {Directory.Delete("Temp", true); }
					catch (Exception) {}
					Directory.CreateDirectory("Temp");
					var fs = new FileStream(filepath, FileMode.OpenOrCreate);
					s.CopyTo(fs); //TODO: may be done async
					fs.Close();
					s.Close();
					
					Unpack = Unpack && new FileInfo(filepath).Extension == ".zip";
					if (Unpack)
					{
						if ((bool)KillRelatedProcesses)
						{
							var archive = new ZipFile(fs.Name);
							foreach (ZipEntry entry in archive)
							{
								var entryPath = (TempFolder ? "Temp\\" : "") + entry.Name;
								var entryName = entryPath.Slice("\\", LastStart: true);

								if ((bool)KillRelatedProcesses && entryName.EndsWith(".exe"))
									TypesFuncs.KillProcesses(Path: AppContext.BaseDirectory + entryName, Name: entryName);
							}

							archive.Close();
						}

						ZipStrings.CodePage = 866;
						new FastZip { EntryFactory = new ZipEntryFactory { IsUnicodeText = true } }.ExtractZip(filepath, (TempFolder? "Temp\\" : ""), null);
						File.Delete(filepath);
					}
					else
					{
						File.Move(filepath, filepath.RemoveFrom(TypesFuncs.Side.Start, "Temp\\"));
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
	/*public static async Task<UpdateResult> checkSoftwareUpdates(bool CheckUpdates, string repositoryLink, string ProgramExePath, Func<bool> UpdateQuestion = default, bool Unpack = true, Regex GitHubFilenameRegex = null, bool TempFolder = false, Regex[] ArchiveIgnoreFileList = null, bool ReverseArchiveFileList = false, bool KillRelatedProcesses = false)
		
		{
			string releasesPage = "https://".Add(repositoryLink).Add("/releases");
			var doc = await Internet.getResponseAsync(releasesPage).ConfigureAwait(false);
			if (doc == null) throw new NullReferenceException("can't get response from page " + releasesPage);
			await doc.WaitForReadyAsync();
			if (doc.Body.Children.Length == 0) throw new NullReferenceException("Unable to access the page. Possible reason: no internet connection");
			var lastestReleaseBlock = doc.QuerySelector("div[data-hpc]")?.FirstElementChild;
			if (lastestReleaseBlock == null) throw new NullReferenceException("Can't get releases from page " + releasesPage);

			string? releaseName = lastestReleaseBlock.QuerySelector(".Link--primary").Text();
			string? releaseDetails = lastestReleaseBlock.QuerySelector("div[data-test-selector=\"body-content\"]")?.Text();
			Version? latestVersion = null;

			bool fileExist = File.Exists(ProgramExePath);
			try
			{
				string? verStr = lastestReleaseBlock.QuerySelector(".ml-1.wb-break-all")?.Text();
				if (verStr == null) 
					throw new ArgumentNullException(nameof(verStr), "Can't get lastest version");
				latestVersion = Version.Parse(new Regex("[^.0-9]").Replace(verStr, ""));
			}
			catch (Exception e)
			{
				if (CheckUpdates && fileExist) throw;
			}

			UpdateResult result = new(Status.NoAction, latestVersion, releaseName, releaseDetails);

			if (!fileExist)
			{
				await DownloadLastest().ConfigureAwait(false);
				return result.Change(Status.Downloaded);
			}
			else if (CheckUpdates)
			{
				var currentVersion = FileVersionInfo.GetVersionInfo(ProgramExePath);
				if (currentVersion is null) 
					throw new InvalidOperationException("Product version field is empty");
				
				/*MessageBox.Show($"Lastest version: {lastestVersion};" +
				                $"\nParsed: {Version.Parse(lastestVersion)}" +
				                $"\n Current version: {currentVersion.ProductVersion}" +
				                $"\n Parsed: {Version.Parse(currentVersion.ProductVersion)}");#1#

				//:If current file's version is lower than in github, download lastest from github
				
				if (latestVersion > Version.Parse(currentVersion.ProductVersion!))
				{
					if (UpdateQuestion == default || !UpdateQuestion()) 
						return result;
					await DownloadLastest().ConfigureAwait(false);
					return result.Change(Status.Updated);
				}
			}
			return result;

			async Task<bool> DownloadLastest()
			{
				var allAssets = lastestReleaseBlock.QuerySelector(".mb-3 > details").QuerySelectorAll("ul > li");
				if (allAssets.Length == 0)
				{
					throw new ArgumentNullException("Can't find Assets list", new InvalidOperationException("Lastest release block html:\n" + lastestReleaseBlock.Html()));
				}
				var gitHubFiles = (
					from fileBlock in allAssets
					let fileNameBlock = fileBlock.FirstElementChild.LastElementChild
					let metadataGroup = fileBlock.LastElementChild
					select new GitHubFile(
						fileNameBlock.Text(),//.RemoveAllFrom("\n\t\r "),
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
					string filepath = $"Temp\\{file.Name}";

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
							var entryPath = TempFolder? "Temp\\" : "" + entry.FullName;
							var entryName = entryPath.Slice("\\", LastStart: true);

							if (KillRelatedProcesses && entryName.EndsWith(".exe"))
								(
									from proc in Process.GetProcessesByName(entryName)
									where proc.MainModule.FileName == AppContext.BaseDirectory + entryName
									select proc
								).ToList().ForEach(p => p.Kill());

							entry.ExtractToFile(entryPath, true);
						}
						archive.Dispose();
						File.Delete(filepath);
					}
					else
					{
						File.Move(filepath, filepath.RemoveFrom("Temp\\", TypesFuncs.Side.Start));
					}
				}
				

				//new WebClient().DownloadFile(releasesPage, "OculusDash.exe");
				return true;
			}*/

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
