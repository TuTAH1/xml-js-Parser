using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Titanium;
using static Titanium.Consol;

/*
 	public static class Colors {
		public static Color 
			White = Color.White        //1
			, Gray = Color.Gray            //2
			, Black = Color.Black      //3
			, Lime = Color.YellowGreen //4
			, Red = Color.FromArgb(255, 30, 60)    //5
			, Orange = Color.FromArgb(255, 165, 0) //6
			, Select = Color.FromArgb(0, 80, 80) //7
			, WeakSelect = Color.FromArgb(0, 40, 40) //8
			, SeaGreen = Color.SeaGreen   //9
			, Yellow = Color.Yellow        //10
			, Cyan = Color.Cyan            //11
			, Violet = Color.BlueViolet //12
			, Silver = Color.Silver        //13
			, Green = Color.FromArgb(73,110,0) //14
			, DefText = White
			, DefBgr = Black
			;
	}
	*/

namespace Titanium {
	public static class Matrix
	{
		public class WriteOptions //TODO: Добавить TextColorMassage
		{
			public string[] INames;
			public string[] JNames;
			public c TextColorHeaders;
			public c TextColorError;
			public c TextColorTip;
			public c TextColorMessage;
			public c TextColor;
			public c TextColorActive;
			public c TextColorActiveHeader;
			public c TextColorDisabled;
			public c TextColorInfinity;
			public c TextColorZero;
			public c BgrColor;
			public c BgrColorActive;
			public c BgrColorActiveHeader;
			public c BgrColorDisabled;
			public c BgrColorInfinity;
			public c BgrColorZero;
			public bool DisableDiagEdit;
			public double DefaultValue;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="jNames">Имена строк, должно совпадать с количеством строк в матрице или == 1 (тогда это имя применится ко всем столбцам); "%#" подменяется номером строк</param>
			/// <param name="iNames">Имена столбцов, аналогично</param>
			/// <param name="textColorHeaders"> Цвет имён строк и столбцов </param>
			/// <param name="textColor"> Цвет текста</param>
			/// <param name="textColorActive">Цвет текста выделенной ячейки</param>
			/// <param name="BgrColorActive">Цвет заливки выделенной ячейки</param>
			public WriteOptions(
				string[] iNames = null, 
				string[] jNames = null, 
				bool disableDiagEdit = false, 
				double defaultValue = 0,
				c textColor = c.Default,
				c bgrColor = c.Default,
				c textColorHeaders = c.dcyan,
				c textColorActiveHeader = c.orange,
				c textColorActive = c.black, 
				c bgrColorActive = c.cyan,
				c bgrColorActiveRow = c.dcyan,
				c textColorZero = c.silver, 
				c bgrColorZero = c.Null,
				c textColorInfinity = c.purple,
				c bgrColorInfinity = c.Null,
				c textColorDisabled = c.gray,
				c bgrColorDisabled = c.Null,
				c textColorError = c.red,
				c textColorTip= c.gray,
				c textColorMessage = c.yellow
				)
			{
				INames=iNames;
				JNames=jNames;
				DisableDiagEdit = disableDiagEdit;
				DefaultValue = defaultValue;

				TextColor=textColor;
				BgrColor = bgrColor;

				TextColorHeaders=textColorHeaders;
				TextColorActive=textColorActive;
				BgrColorActive = (bgrColorActive == c.Default)?			bgrColor : bgrColorActive;
				TextColorActiveHeader = textColorActiveHeader;

				BgrColorActiveHeader = (bgrColorActiveRow == c.Default)?	bgrColor : bgrColorActiveRow;

				TextColorDisabled = (textColorDisabled == c.Default)?	textColor : textColorDisabled;
				BgrColorDisabled = (bgrColorDisabled == c.Default)?		bgrColor : bgrColorDisabled;

				TextColorInfinity = (textColorInfinity == c.Default)?	bgrColor : textColorInfinity;
				BgrColorInfinity = (bgrColorInfinity == c.Default)?		bgrColor : bgrColorInfinity;

				TextColorZero = textColorZero;
				BgrColorZero = bgrColorZero;

				TextColorError = textColorError;
				TextColorTip = textColorTip;
				TextColorMessage = textColorMessage;

				DefaultValue = defaultValue;

			}
		}
		/// <summary>
		/// Выводит матрицу в консоли
		/// </summary>
		/// <param name="matrix">Двумерная матрица [столбцы, строки]</param>
		/// <param name="iActive">Номер подсвеченного столбца</param>
		/// <param name="jActive"> Номер подсвеченной строки (если указаны оба, будет подсвечена ячейка)</param>
		public static void Print(this string[,] matrix, int iActive = -1, int jActive = -1, WriteOptions WO = null)  //:09.10.2021 bugfix
		{
			c DefColor = c.Default, DefBgr = c.Null;//(Consol.DefaultTextColor!=wo.TextColor)? Consol.DefaultTextColor ://null(Consol.DefaultBackgroundColor != wo.BgrColor)? Consol.DefaultBackgroundColor : ;
			if (DefaultTextColor != WO.TextColor&&WO.TextColor!=c.Default)// if def color has been changed
			{
				DefColor = WO.TextColor;
				RClr(WO.TextColor);
			}

			if (WO == null) WO = new WriteOptions();
			bool SameJNames = false, SameINames = false;

			if (WO.INames==null&&WO.JNames==null) //Если имена строк и столбцов не назначены, назначить пустую строку
			{
				WO.JNames = new []{""};
				WO.INames = new []{""};
				SameJNames=SameJNames=true;
			}
			else if (
				WO.INames.Length>1&&WO.JNames.Length>1&&
				WO.JNames.Length!=matrix.GetLength(0)&&
				WO.INames.Length!=matrix.GetLength(1)
			)
				throw new ArgumentException($"Количество имён строк/столбцов ({WO.INames.Length}/{WO.JNames.Length}) не совпадает с количеством строк/столбцов ({matrix.GetLength(1)}/{matrix.GetLength(0)})");

			if (WO.INames.Length==1) SameINames=true;
			if (WO.JNames.Length==1) SameJNames=true;

			int J = matrix.GetLength(0),
				I = matrix.GetLength(1);


			string[] newINames = new string[I];
			string[] newJNames = new string[J];

			for (int i = 0; i<I; i++) //замена "%#" на номер
			{
				int INi = SameINames ? 0 : i; //если имена столбцов одинаковы, то берется первое (единственное) имя
				newINames[i]=WO.INames[INi].Replace("%i", i.ToString());
			}

			int MaxJNameLength = 0;
			for (int j = 0; j<J; j++)
			{
				int INj = SameJNames ? 0 : j;
				newJNames[j]=WO.JNames[INj].Replace("%i", j.ToString()).Replace("%j", j.ToString());
				MaxJNameLength = Math.Max(newJNames[j].Length, MaxJNameLength);
			}

			int[] MaxValLength = new int[I];
			for (int i = 0; i < I; i++)         // Нахождение самой длинной строки в столбце
			{
				int max = 0;
				for (int j = 0; j < J; j++)
				{
					if (matrix[j, i] == double.MaxValue.ToString()) matrix[j, i] = "∞";
					if (matrix[j, i].Length>max)
						max=matrix[j, i].Length;
				}

				MaxValLength[i] = Math.Max(max, newINames[i].Length);
			}

			ReWrite(new string(' ', MaxJNameLength)+ " ");

			for (int i = 0; i < I; i++)
			{
				Clr(WO.TextColorHeaders,WO.BgrColor);
				if (i==iActive) Clr(WO.TextColorActiveHeader,WO.BgrColorActiveHeader);
				ReWrite(newINames[i].FormatString(MaxValLength[i], TypesFuncs.Positon.center) + " "); //Вывод названий столбцов
				if (i==I-1) ReWrite(BackgroundColor: WO.BgrColor);
			}

			for (int j = 0; j<J; j++) //Цикл вывода матрицы
			{
				Clr(WO.TextColorHeaders,WO.BgrColor);
				if (j==jActive) Clr(WO.TextColorActiveHeader, WO.BgrColorActiveHeader);
				ReWrite("\n");
				ReWrite(newJNames[j].FormatString(MaxJNameLength, TypesFuncs.Positon.center) + " "); //Вывод названия строки
				if (jActive >= 0)
				{
					if (iActive < 0) //Если активного столбца не выбрано
						if (jActive == j) Clr(WO.TextColorActive, WO.BgrColorActive); //Если текущая строка – активная, окрас текста
						else Clr(WO.TextColor,WO.BgrColor);
				}

				for (int i = 0; i<I; i++)
				{
					RClr();
					if (WO.DisableDiagEdit && i==j) Clr(WO.TextColorDisabled, WO.BgrColorDisabled); else Clr(WO.TextColor, WO.BgrColor);

					if (iActive >= 0)
					{
						if (jActive < 0)
						{ if (iActive == i) Clr(WO.TextColorActive, WO.BgrColorActive); }
						else if (iActive == i && jActive == j) Clr(WO.TextColorActive, WO.BgrColorActive);
					}

					if (matrix[j, i] == "∞") Clr(WO.TextColorInfinity, WO.BgrColorInfinity);
					if (matrix[j,i]=="0") Clr(WO.TextColorZero, WO.BgrColorZero);
					ReWrite(matrix[j, i].FormatString(MaxValLength[i], TypesFuncs.Positon.center) + " ");
				}
				ReWrite(BackgroundColor: WO.BgrColor);
			}
		}
		public static int LongestString(string[,] matrix)
		{
			int max = 0;
			foreach (string el in matrix)
			{
				if (el.Length > max)
					max = el.Length;
			}

			return max;
		}

		public class ReadOptions
		{
			public bool Symmetrix;
			public string Message;
			public bool ControlMassage;
			public WriteOptions WO;
			/// <summary>
			/// 
			/// </summary>
			/// <param name="defaultValue">Значение, на которое заменяются все записи, не соответствующие формауту double</param>
			/// <param name="symmetrix">Симметричная ли матрица</param>
			/// <param name="DisableDiagEdit">Отключить ли редактирвоание диагонали</param>
			/// <param name="message">Сообщение, показывающееся во время ввода матрицы</param>
			/// <param name="controlMassage">Показывать ли управление</param>
			/// <param name="wo">Write Options</param>
			public ReadOptions(bool symmetrix = false, string message = "Введите матрицу", bool controlMassage = true, WriteOptions wo = null)
			{
				Symmetrix = symmetrix;
				Message = message;
				ControlMassage = controlMassage;
				WO = wo?? new WriteOptions();
			}
		}

		public static double[,] Read(string[,] matrix = null, ReadOptions RO = null)
		{
			int curT = CursorTop;
			bool CurVi = CursorVisible;
			CursorVisible = false;
			int i = 0, j = 0,
				I = matrix.GetLength(1),
				J = matrix.GetLength(0);
			bool Exit = false, ignoreInput = false;

			RO ??= new ReadOptions();
			CursorVisible = false;
			ConsoleKeyInfo CurrentKey = new ConsoleKeyInfo();
			do
			{
				string curStr = matrix[j, i];
				ReWrite(RO.Message
					        .Replace("%i", RO.WO.INames.Length==matrix.GetLength(1)? RO.WO.INames[i] : i.ToString())
					        .Replace("%j", RO.WO.JNames.Length==matrix.GetLength(0)? RO.WO.JNames[j] : j.ToString())
				        +"\n",TextColor: RO.WO.TextColorMessage, ClearLine: true, CurPosH: CPH.Left); //BUG: Почему-то не работает ClearLine TODO:
				ReWrite("Используйте стрелки, чтобы перемещаться по матрице, enter чтобы завершить ввод", TextColor: RO.WO.TextColorTip, CurPosH: CPH.Left, CurPosV: CPV.Bottom);
				matrix.Print(i, j, RO.WO);
				if (ignoreInput) ignoreInput = false;
				else CurrentKey = ReadKey(true);
				BufferHeight = Console.WindowHeight+3;
				switch (CurrentKey.Key)
				{
					case ConsoleKey.DownArrow:
						if (j == J - 1)
						{
							if (i == I - 1) { i = 0; j = 0; } //последняя ячейка
							else { j = 0; i++; } //последняя строка
						}
						else j++;
						break;

					case ConsoleKey.UpArrow:
						if (j == 0) {
							if (i == 0) { i = I-1; j = J-1; } //перавая ячейка
							else { i--; j=J-1; } //первая строка
						}
						else j--;
						break;

					case ConsoleKey.Spacebar:
					case ConsoleKey.Tab:
					case ConsoleKey.RightArrow:
						if (i==I-1) {
							if (j==J-1) { i = 0; j = 0; } //последняя ячейка
							else { j++; i=0; } //последний столбец
						} else i++;
					break;

					case ConsoleKey.LeftArrow:
						if (i==0) {
							if (j==0) { j = J-1; i = I-1; } //первая ячейка
							else { j--; i=I-1; } //первый столбец
						} else i--;
					break;

					case ConsoleKey.Enter:
						ReWrite("\nВы уверены, что хотите выйти? Нажмите Enter ещё раз, чтобы подтвердить",RO.WO.TextColorError,RO.WO.BgrColor);
						CurrentKey = ReadKey();
						if (CurrentKey.Key == ConsoleKey.Enter)
						{
							Exit = true;
						}
						else ignoreInput = true;
					break;

					default:
						{
							if (RO.WO.DisableDiagEdit)
								if (i == j) break;

							if (CurrentKey.KeyChar.IsDoubleT()) //TODO: добавить поддержку отрицательных чисел
							{
								if (curStr == "" && CurrentKey.KeyChar.ToString()==CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
									curStr = "0";
								curStr += CurrentKey.KeyChar;
							}
							else if (CurrentKey.Key==ConsoleKey.Backspace)
							{
								if (curStr.Length>0) curStr = curStr.Substring(0, curStr.Length - 1);
							}

							updMatrix(matrix,curStr,j,i,RO.Symmetrix);
						}
						break;
				}
				ClearWindow();
				CursorTop = curT;
				CursorLeft = 0;
			} while (!Exit);
			CursorVisible = true;
			double[,] dmatrix = new double[J, I];
			for (j = 0; j < J; j++)
			{
				for (i = 0; i < I; i++)
				{
					if (!double.TryParse(matrix[j, i], out dmatrix[j, i])) dmatrix[j,i] = RO.WO.DefaultValue; //если парситься, засунуть это значение в dmatrix, иначе засунуть defaultValue;
				}
			}

			CursorVisible = CurVi;
			return dmatrix;
		}

		private static void updMatrix(string [,] matrix, string curStr, int j, int i, bool Symmetrix)
		{
				matrix[j, i] = curStr;
				if (Symmetrix) matrix[i, j] = curStr;
		}

		/// <summary>
		/// Создаёт матрицу
		/// </summary>
		public class Create
		{
			public static string[,] FillDiag(int Columns, int Rows = 0, string defaultStr = "", string diagStr = "x")
			{
				Rows=(Rows==0) ? Columns : Rows; //если m не назначен, m=n
				string[,] mx = new String[Rows, Columns];

				for (int j = 0; j<mx.GetLength(0); j++)
				{
					for (int i = 0; i < mx.GetLength(1); i++)
					{
						mx[j,i] = (i == j) ? diagStr : defaultStr;
					}
				}

				return mx;
			}

			public static string[,] FillAll(int Columns, int Rows = 0, string defaultStr = "")
			{
				Rows=(Rows==0) ? Columns : Rows; //если m не назначен, m=n
				string[,] mx = new string[Rows,Columns];

				for (int j = 0; j<mx.GetLength(0); j++)
				{
					for (int i = 0; i < mx.GetLength(1); i++)
					{
						mx[j,i] = defaultStr;
					}
				}

				return mx;
			}
		}
	}

	public static class Consol
	{
		#region SystemConsoleReplacements

		public static bool CursorVisible
		{
			get => Console.CursorVisible;
			set => Console.CursorVisible = value;
		}
		public static int BufferWidth
		{
			get => Console.BufferWidth;
			set => Console.BufferWidth = value;
		}

		public static int BufferHeight
		{
			get => Console.BufferHeight;
			set => Console.BufferHeight = value;
		}
		public static int CursorTop
		{
			get => Console.CursorTop;
			set
			{
				if (value < 0) value = 0;
				if (value >= Console.BufferHeight) value = Console.BufferHeight - 1;

				Console.CursorTop = value;
			}
		}

		public static int CursorLeft
		{
			get => Console.CursorLeft;
			set
			{
				while (value<0)
				{
					value += Console.BufferWidth;
					CursorTop -= 1;
				}

				while (value>=BufferWidth)
				{
					value -= Console.BufferWidth;
					CursorTop += 1;
				}

				Console.CursorLeft = value;
			}
		}

		public static ConsoleKeyInfo ReadKey(bool Intercept = false) => Console.ReadKey(Intercept);

		#endregion

		#region Color

		private static ColorMapper colorMapper = null;
		public static c DefaultTextColor { get; private set; } = c.white;
		public static c DefaultBackgroundColor { get; private set; } = c.black;

		public enum c
		{
			dblue = ConsoleColor.DarkBlue,
			dcyan = ConsoleColor.DarkCyan,
			blue = ConsoleColor.Blue,
			cyan = ConsoleColor.Cyan,
			green = ConsoleColor.DarkGreen,
			lime = ConsoleColor.Green,
			red = ConsoleColor.DarkRed,
			pink = ConsoleColor.Red,
			orange = ConsoleColor.DarkYellow,
			yellow = ConsoleColor.Yellow,
			dpurple = ConsoleColor.DarkMagenta,
			purple = ConsoleColor.Magenta,
			gray = ConsoleColor.DarkGray,
			silver = ConsoleColor.Gray,
			dwhite = silver,
			white = ConsoleColor.White,
			black = ConsoleColor.Black,
			Null = -1,
			Default = -2
		}

		/// <summary>
		/// Changes text and background color. c.Null = don't change
		/// Изменяет цвет текста и фона. c.Null = не изменять
		/// </summary>
		/// <param name="textColor"></param>
		/// <param name="backgroundColor"></param>
		public static void Clr(c? TextColor = c.Default, c? BackgroundColor = c.Null)
		{
			CheckColorPallete();
			TextColor??=c.Null;
			BackgroundColor ??= c.Null;

			if (TextColor != c.Null)
				Console.ForegroundColor = (ConsoleColor)((TextColor == c.Default)?
					DefaultTextColor : TextColor);

			if (BackgroundColor != c.Null)
				Console.BackgroundColor = (ConsoleColor)((BackgroundColor == c.Default)?
					DefaultBackgroundColor : BackgroundColor);
		}

		public static void BClr(c? BackgroundColor = c.Default)
		{
			Clr(c.Null,BackgroundColor);
		}

		/// <summary>
		/// Изменяет и применяет цвет текста и фона по умолчанию
		/// </summary>
		/// <param name="newDefTextClr">new Default Text Color;
		/// c.Null – не изменять цвет текста, c.Default – применить дефолтный цвет текста, иначе – Изменить цвет текста по умолчанию</param>
		/// <param name="newDefBgrClr">new Default Background Color;
		/// c.Null – не изменять цвет фона, c.Default – применить дефолтный цвет фона, иначе – Изменить цвет фона по умолчанию</param>
		public static void RClr(c? newDefTextClr = c.Default, c? newDefBgrClr = c.Default)
		{
			CheckColorPallete();
			newDefTextClr??= c.Null;
			newDefBgrClr ??= c.Null;
			if (newDefTextClr != c.Default && newDefTextClr != c.Null) DefaultTextColor = (c)newDefTextClr;
			if (newDefTextClr !=c.Null) Console.ForegroundColor = (ConsoleColor)DefaultTextColor;

			if (newDefTextClr != c.Default && newDefTextClr !=c.Null) DefaultBackgroundColor = (c)newDefBgrClr;
			if (newDefBgrClr !=c.Null) Console.BackgroundColor = (ConsoleColor)DefaultBackgroundColor;
		}

		private static void CheckColorPallete()
		{
			if (colorMapper != null) return;

			Console.OutputEncoding = Encoding.UTF8;
			colorMapper = new ColorMapper();
			colorMapper.SetConsolePallete();
		}

		public static void SetConsolePallete(Color? Black = null, Color? DarkBlue = null, Color? DarkCyan = null, Color? Blue = null, Color? Cyan = null, Color? Green = null, Color? Lime = null, Color? Red = null, Color? Pink = null, Color? Orange = null, Color? Yellow = null, Color? Purple = null, Color? Violet = null, Color? Gray = null, Color? Silver = null, Color? White = null)
		{
			colorMapper = new ColorMapper();
			colorMapper.SetConsolePallete(Black, DarkBlue, DarkCyan, Blue, Cyan, Green, Lime, Red, Pink, Orange, Yellow, Purple, Violet, Gray, Silver, White);
		}


		#region Color remapping

		/// <summary>
		/// Based on code that was originally written by Alex Shvedov, and that was then modified by MercuryP, edited for integrating in ColorfulConsole by tomakita, slightly modified by Титан
		/// </summary>
		private sealed class ColorMapper
		{
			/// <summary>
			/// A Win32 COLORREF, used to specify an RGB color.  See MSDN for more information:
			/// https://msdn.microsoft.com/en-us/library/windows/desktop/dd183449(v=vs.85).aspx
			/// </summary>
			[StructLayout(LayoutKind.Sequential)]
			public struct COLORREF
			{
				private uint ColorDWORD;

				internal COLORREF(Color color)
				{
					ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
				}

				internal COLORREF(uint r, uint g, uint b)
				{
					ColorDWORD = r + (g << 8) + (b << 16);
				}

				public override string ToString()
				{
					return ColorDWORD.ToString();
				}
			}

			[StructLayout(LayoutKind.Sequential)]
			private struct COORD
			{
				internal short X;
				internal short Y;
			}

			[StructLayout(LayoutKind.Sequential)]
			private struct SMALL_RECT
			{
				internal short Left;
				internal short Top;
				internal short Right;
				internal short Bottom;
			}

			[StructLayout(LayoutKind.Sequential)]
			private struct CONSOLE_SCREEN_BUFFER_INFO_EX
			{
				internal int cbSize;
				internal COORD dwSize;
				internal COORD dwCursorPosition;
				internal ushort wAttributes;
				internal SMALL_RECT srWindow;
				internal COORD dwMaximumWindowSize;
				internal ushort wPopupAttributes;
				internal bool bFullscreenSupported;
				internal COLORREF black;
				internal COLORREF darkBlue;
				internal COLORREF darkGreen;
				internal COLORREF darkCyan;
				internal COLORREF darkRed;
				internal COLORREF darkMagenta;
				internal COLORREF darkYellow;
				internal COLORREF gray;
				internal COLORREF darkGray;
				internal COLORREF blue;
				internal COLORREF green;
				internal COLORREF cyan;
				internal COLORREF red;
				internal COLORREF magenta;
				internal COLORREF yellow;
				internal COLORREF white;
			}

			private const int STD_OUTPUT_HANDLE = -11;                               // per WinBase.h
			private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);    // per WinBase.h

			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern IntPtr GetStdHandle(int nStdHandle);

			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX csbe);

			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern bool SetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX csbe);

			/// <summary>
			/// Maps a System.Drawing.Color to a System.ConsoleColor.
			/// </summary>
			/// <param name="oldColor">The color to be replaced.</param>
			/// <param name="newColor">The color to be mapped.</param>
			public void MapColor(ConsoleColor oldColor, Color newColor)
			{
				// NOTE: The default console colors used are gray (foreground) and black (background).
				MapColor(oldColor, newColor.R, newColor.G, newColor.B);
			}

			public void MapColor(ConsoleColor oldColor, Color? newColor)
			{
			   MapColor(oldColor, (Color)newColor);
			}


			/// <summary>
			/// Gets a collection of all 16 colors in the console buffer.
			/// </summary>
			/// <returns>Returns all 16 COLORREFs in the console buffer as a dictionary keyed by the COLORREF's alias
			/// in the buffer's ColorTable.</returns>
			public Dictionary<string, COLORREF> GetBufferColors()
			{
				Dictionary<string, COLORREF> colors = new Dictionary<string, COLORREF>();
				IntPtr hConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE);    // 7
				CONSOLE_SCREEN_BUFFER_INFO_EX csbe = GetBufferInfo(hConsoleOutput);

				colors.Add("black", csbe.black);
				colors.Add("darkBlue", csbe.darkBlue);
				colors.Add("darkGreen", csbe.darkGreen);
				colors.Add("darkCyan", csbe.darkCyan);
				colors.Add("darkRed", csbe.darkRed);
				colors.Add("darkMagenta", csbe.darkMagenta);
				colors.Add("darkYellow", csbe.darkYellow);
				colors.Add("gray", csbe.gray);
				colors.Add("darkGray", csbe.darkGray);
				colors.Add("blue", csbe.blue);
				colors.Add("green", csbe.green);
				colors.Add("cyan", csbe.cyan);
				colors.Add("red", csbe.red);
				colors.Add("magenta", csbe.magenta);
				colors.Add("yellow", csbe.yellow);
				colors.Add("white", csbe.white);

				return colors;
			}

			/// <summary>
			/// Sets all 16 colors in the console buffer using colors supplied in a dictionary.
			/// </summary>
			/// <param name="colors">A dictionary containing COLORREFs keyed by the COLORREF's alias in the buffer's 
			/// ColorTable.</param>
			public void SetBatchBufferColors(Dictionary<string, COLORREF> colors)
			{
				IntPtr hConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE); // 7
				CONSOLE_SCREEN_BUFFER_INFO_EX csbe = GetBufferInfo(hConsoleOutput);

				csbe.black = colors["black"];
				csbe.darkBlue = colors["darkBlue"];
				csbe.darkGreen = colors["darkGreen"];
				csbe.darkCyan = colors["darkCyan"];
				csbe.darkRed = colors["darkRed"];
				csbe.darkMagenta = colors["darkMagenta"];
				csbe.darkYellow = colors["darkYellow"];
				csbe.gray = colors["gray"];
				csbe.darkGray = colors["darkGray"];
				csbe.blue = colors["blue"];
				csbe.green = colors["green"];
				csbe.cyan = colors["cyan"];
				csbe.red = colors["red"];
				csbe.magenta = colors["magenta"];
				csbe.yellow = colors["yellow"];
				csbe.white = colors["white"];

				SetBufferInfo(hConsoleOutput, csbe);
			}

			private CONSOLE_SCREEN_BUFFER_INFO_EX GetBufferInfo(IntPtr hConsoleOutput)
			{
				CONSOLE_SCREEN_BUFFER_INFO_EX csbe = new CONSOLE_SCREEN_BUFFER_INFO_EX();
				csbe.cbSize = (int)Marshal.SizeOf(csbe); // 96 = 0x60

				if (hConsoleOutput == INVALID_HANDLE_VALUE)
				{
					throw CreateException(Marshal.GetLastWin32Error());
				}

				bool brc = GetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe);

				if (!brc)
				{
					throw CreateException(Marshal.GetLastWin32Error());
				}

				return csbe;
			}

			private void MapColor(ConsoleColor color, uint r, uint g, uint b)
			{
				IntPtr hConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE); // 7
				CONSOLE_SCREEN_BUFFER_INFO_EX csbe = GetBufferInfo(hConsoleOutput);

				switch (color)
				{
					case ConsoleColor.Black:
						csbe.black = new COLORREF(r, g, b);
						break;
					case ConsoleColor.DarkBlue:
						csbe.darkBlue = new COLORREF(r, g, b);
						break;
					case ConsoleColor.DarkGreen:
						csbe.darkGreen = new COLORREF(r, g, b);
						break;
					case ConsoleColor.DarkCyan:
						csbe.darkCyan = new COLORREF(r, g, b);
						break;
					case ConsoleColor.DarkRed:
						csbe.darkRed = new COLORREF(r, g, b);
						break;
					case ConsoleColor.DarkMagenta:
						csbe.darkMagenta = new COLORREF(r, g, b);
						break;
					case ConsoleColor.DarkYellow:
						csbe.darkYellow = new COLORREF(r, g, b);
						break;
					case ConsoleColor.Gray:
						csbe.gray = new COLORREF(r, g, b);
						break;
					case ConsoleColor.DarkGray:
						csbe.darkGray = new COLORREF(r, g, b);
						break;
					case ConsoleColor.Blue:
						csbe.blue = new COLORREF(r, g, b);
						break;
					case ConsoleColor.Green:
						csbe.green = new COLORREF(r, g, b);
						break;
					case ConsoleColor.Cyan:
						csbe.cyan = new COLORREF(r, g, b);
						break;
					case ConsoleColor.Red:
						csbe.red = new COLORREF(r, g, b);
						break;
					case ConsoleColor.Magenta:
						csbe.magenta = new COLORREF(r, g, b);
						break;
					case ConsoleColor.Yellow:
						csbe.yellow = new COLORREF(r, g, b);
						break;
					case ConsoleColor.White:
						csbe.white = new COLORREF(r, g, b);
						break;
				}

				SetBufferInfo(hConsoleOutput, csbe);
			}

			private void SetBufferInfo(IntPtr hConsoleOutput, CONSOLE_SCREEN_BUFFER_INFO_EX csbe)
			{
				csbe.srWindow.Bottom++;
				csbe.srWindow.Right++;

				bool brc = SetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe);
				if (!brc)
				{
					throw CreateException(Marshal.GetLastWin32Error());
				}
			}

			private Exception CreateException(int errorCode)
			{
				const int ERROR_INVALID_HANDLE = 6;
				if (errorCode == ERROR_INVALID_HANDLE) // Raised if the console is being run via another application, for example.
				{
					return new ConsoleAccessException();
				}

				return new ColorMappingException(errorCode);
			}

			public void SetConsolePallete(Color? Black = null, Color? DarkBlue = null, Color? DarkCyan = null, Color? Blue = null, Color? Cyan = null, Color? Green = null, Color? Lime = null, Color? Red = null, Color? Pink = null, Color? Orange = null, Color? Yellow = null, Color? Purple = null, Color? Violet = null, Color? Gray = null, Color? Silver = null, Color? White = null)
			{
				Black??=Color.Black;
				DarkBlue??=Color.DarkBlue;
				DarkCyan??=Color.DarkCyan;
				Blue??=Color.FromArgb(52, 125, 235);
				Cyan??=Color.Cyan;
				Green??=Color.Green;
				Lime??=Color.Lime;
				Red??=Color.Red;
				Pink??=Color.Pink;
				Orange??=Color.Orange;
				Yellow??=Color.Yellow;
				Purple??=Color.Purple;
				Violet??=Color.Violet;
				Gray??=Color.Gray;
				Silver??=Color.Silver;
				White??=Color.White;

				MapColor(ConsoleColor.Black, Black);
				MapColor(ConsoleColor.Blue,Blue);
				MapColor(ConsoleColor.DarkBlue, Color.DarkBlue);
				MapColor(ConsoleColor.DarkCyan,DarkCyan);
				MapColor(ConsoleColor.Cyan,Cyan);
				MapColor(ConsoleColor.DarkGreen,Green);
				MapColor(ConsoleColor.Green,Lime);
				MapColor(ConsoleColor.DarkRed,Red);
				MapColor(ConsoleColor.Red,Pink);
				MapColor(ConsoleColor.DarkYellow,Orange);
				MapColor(ConsoleColor.Yellow,Yellow);
				MapColor(ConsoleColor.DarkMagenta,Purple);
				MapColor(ConsoleColor.Magenta,Violet);
				MapColor(ConsoleColor.DarkGray,Gray);
				MapColor(ConsoleColor.Gray,Silver);
				MapColor(ConsoleColor.White,White);
			}
		}

		/// <summary>
		/// Encapsulates information relating to exceptions thrown while making calls to the console via the Win32 API.
		/// </summary>
		public sealed class ConsoleAccessException : Exception
		{
			/// <summary>
			/// Encapsulates information relating to exceptions thrown while making calls to the console via the Win32 API.
			/// </summary>
			public ConsoleAccessException()
				: base("Color conversion failed because a handle to the actual windows console was not found.")
			{
			}
		}

		/// <summary>
		/// Encapsulates information relating to exceptions thrown during color mapping.
		/// </summary>
		public sealed class ColorMappingException : Exception
		{
			/// <summary>
			/// The underlying Win32 error code associated with the exception that
			/// has been trapped.
			/// </summary>
			public int ErrorCode { get; private set; }

			/// <summary>
			/// Encapsulates information relating to exceptions thrown during color mapping.
			/// </summary>
			/// <param name="errorCode">The underlying Win32 error code associated with the exception that
			/// has been trapped.</param>
			public ColorMappingException(int errorCode)
				: base(string.Format("Color conversion failed with system error code {0}!", errorCode))
			{
				ErrorCode = errorCode;
			}
		}
#endregion

		#endregion

		#region Input
		public static bool qu(string q = "", c TextColor = c.orange)
		{
			Console.Write(" ");
			bool answer;
			ChangeRead(ref q,TextColor:TextColor);

			if ((q[0] == 'д' || q[0] == 'y' || q[0] == '1' || q.Contains("конечно") || q.Contains("Sbrakets") || q.Contains("ок") || q.Contains("ладно") || q.Contains("угу") || q.Contains("ага") || q.Contains("хорошо") || q.Contains("хочу"))&&!q.Contains("нет")) answer = true;
			else answer = false;
			//Color Red = Color.FromArgb(255, 30, 60), Lime = Color.YellowGreen;
			ReWrite(q,TextColor: answer? c.lime:  c.red, ShiftRight: q.Length);

			return answer;
		}

		public static c SelectColor = c.dblue;
		public static bool quSwitch(string QuestionText = null, bool DefaultValue = false, c QuestionColor = c.Default, c TrueTextColor = c.lime, c FalseTextColor = c.red, c QuestionBackgroundColor = c.Default, c? ValueBackgroundColor = null, string Yes = "Да", string No = "Нет", string LeftArrow = "◄ ", string RightArrow = " ►")
		{
			ReWrite(QuestionText, QuestionColor,QuestionBackgroundColor);
			ValueBackgroundColor ??= SelectColor;
			bool OldCursorVisible = CursorVisible;
			CursorVisible = false;
			int MaxYesnoLength = Math.Max(Yes.Length, No.Length);
			Yes = LeftArrow + Yes.FormatString(MaxYesnoLength, TypesFuncs.Positon.center, ' ', -LeftArrow.Length);
			No = No.FormatString(MaxYesnoLength, TypesFuncs.Positon.center, ' ', RightArrow.Length) + RightArrow;

			c oldColor = (c)Console.ForegroundColor;
			c oldBColor = (c)Console.BackgroundColor;
			ConsoleKey key;
			bool result = DefaultValue;
			BClr(ValueBackgroundColor);
			ReWrite(DefaultValue? Yes:No, TextColor: DefaultValue? TrueTextColor:FalseTextColor);
			do
			{
				key = ReadKey(true).Key;
				switch (key)
				{
					case ConsoleKey.LeftArrow:
						if (result == true)
						{
							ReWrite(No, TextColor: FalseTextColor, LinesUp: 0, ShiftRight: -Yes.Length);
							result = false;
						}

						break;

					case ConsoleKey.RightArrow:
						if (result == false)
						{
							ReWrite(Yes, TextColor: TrueTextColor, LinesUp: 0, ShiftRight: -No.Length);
							result = true;
						}

						break;
				}
			} while (key!=ConsoleKey.Enter&&key!=ConsoleKey.Escape);

			CursorLeft = 0;
			CursorTop += 1;
			CursorVisible = OldCursorVisible;
			Clr(oldColor,oldBColor);
			return result;
		}

		/// <summary>
		/// Устарело. Лучше использовать QuSwitch()
		/// <para></para> Функция вопроса с несколькими вариантами ответа
		/// </summary>
		/// <returns>0 = нет; 1 = да; 2 = не знаю</returns>
		public static byte QuB(c TextColor = c.orange)
		{
			Console.Write(" ");
			string q; byte answer = 0;
			Clr(TextColor);
			q = Console.ReadLine();
			Clr();

			if (q[0] == 'д' || q[0] == 'y' || q[0] == '1' || q.Contains("конечно") || q.Contains("Sbrakets") || q.Contains("ок") || q.Contains("ладно") || q.Contains("угу") || q.Contains("ага") || q.Contains("хорошо") || q.Contains("хочу")) answer = 1; // да
			if (q.Contains("что") || q.Contains("как") || q.Contains("зачем") || q.Contains("почему") || q.Contains("понима")) answer = 2; // вопрос непонятен
			if (q.Contains("нет")) answer = 0; //нет

			return answer;
		}

		public class ReadOutput //:08.11.2021
		{
			private string _String;
			//private uint UInt;
			public ConsoleKey KeyPressed;
			public ReadKeyType KeyType;

			public ReadOutput( ConsoleKey keyPressed, ReadKeyType keyType, string s = null)
			{
				_String = s;
				KeyPressed = keyPressed;
				KeyType = keyType;
			}

			public double Double() => _String.ToDoubleT();

			public long Long() => _String.ToLongT();
			

			public int Int() => _String.ToIntT();
			public BigInteger BigInteger() => System.Numerics.BigInteger.Parse(_String);

			public string String() => _String.ToVisibleString(); //TODO: исправить баг (при, вроде, стирании, в строку попадают null), убрать косталь
			public string RawString() => _String;
		}

		public enum ReadKeyType
		{
			StopKey,
			CancelKey
		}

		/// <summary>
		/// Читает и выводит на экран только клавиши, соответствующие указанному типу, остальные игнорирует
		/// </summary>
		/// <param name="InputType">Вводимы тип</param>
		/// <param name="StopReadKeys">Список клавиш, по нажатию которых ввод завершается. По умолчанию – Enter</param>
		/// <param name="ThrewStopKeys">Помещает код последней нажатой клавиши (завершения ввода) в 3 последние символы строки</param>
		/// <param name="Print">Выводить на экран вводимый текст</param>
		/// <param name="InputString">Изначальная вводимая строка (по умолчанию, пуста)</param>
		/// <param name="ShowError">Показывать ли ошибку при нажатии неверной клавиши</param>
		/// <param name="ErrorTextColor">Цвет текста ошибки</param>
		/// <param name="ErrorPosition">вертикальная позиция (отступ) строки, где будет выведена ошибка. Исключительные значения: Int32.Maxvalue – в самом низу окна, Int32.MinValue – в самом верху окна</param>
		/// <param name="isErrorPosition_Absolute">Является ли ErrorPosition абсолютным значением (иначе, это отступ от текущей строки). Игнорируется при исключительных значениях</param>
		/// <returns></returns>
	public static ReadOutput ReadT(Input InputType = Input.String, ConsoleKey[] StopReadKeys = null, ConsoleKey[] CancelReadKeys = null, bool Print = true, c TextColor = c.orange, string InputString = "",bool AllowEmptyString = false, bool ClearLine = false, string Placeholder = "", c PlaceholderColor = c.gray, bool ShowError = false, c ErrorTextColor = c.red, int ErrorPosition = int.MaxValue, bool isErrorPosition_Absolute = false, int? MaxSymbols = null, BigInteger? MaxValue = null, BigInteger? MinValue = null) //:19.08.2022 CancelReadKeys теперь по умолчанию - пустой массив
		{	
			//TODO: Сделать изменение цвета текста после завершения ввода
			//TODO: Сделать показ ошибки при нажатии неподдерживаемой клавиши: добавить  bool ShowError = false, string ErrorTextColor = c.red, int ErrorPosition = Int32.MaxValue, bool isErrorPosition_Absolute = false
			//var curColor =(c) ForegroundColor

			InputString ??= "";
			Placeholder ??= "";
			ConsoleKey ro_keyPressed = default;
			ReadKeyType ro_keyType = default;
			Clr(TextColor);
			ReWrite(InputString,TextColor: TextColor, ClearLine: ClearLine);
			if (Placeholder!="") ReWrite(Placeholder, TextColor: PlaceholderColor, ClearLine: ClearLine, RestoreCurPos: true);
			ConsoleKeyInfo CurrentKey;
			bool Stop, Read, isErrorPrinted = false;
			string SingleUseKeys = "";
			bool[] SUKused;
			int SUKcount = 0;
			StopReadKeys ??= new[] { ConsoleKey.Enter }; //TODO:Вообще, лучше создать отдельный класс ReturnKeys, содержащий массив ConsoleKey и переменную ConsoleKeyType (возможно, в виде строки или числа), а в функцию сувать этот массив класса ReturnKeys
			CancelReadKeys ??= new ConsoleKey[]{ };
			//ErrorTextColor ??= ;

			ReadT_PrintErrorParametrs p = new ReadT_PrintErrorParametrs(ShowError, ErrorTextColor, ErrorPosition, isErrorPosition_Absolute);
			static void Error(string Text, ReadT_PrintErrorParametrs p, out bool isErrorPrinted)
			{
				if (p.ShowError)
				{
					CPV cpv = p.ErrorPosition == int.MaxValue ? CPV.Bottom : p.ErrorPosition == int.MinValue ? CPV.Up : CPV.None;
					int LinesUp = cpv == CPV.None? p.ErrorPosition : 0;
					if (p.isErrorPosition_Absolute) cpv = CPV.Up;

					Console.Write('\a'); //: Beep
					ReWrite(Text, TextColor: p.ErrorTextColor,LinesUp: LinesUp,ClearLine: true, CurPosH: CPH.Left, CurPosV: cpv, RestoreCurPos: true);
					isErrorPrinted = true;
				}
				else
				{
					isErrorPrinted = false;
				}
			}
			 

			/*switch (InputType)
			{
				case Input.Int: 
					SingleUseKeys += '-';
					MaxValue = int.MaxValue;
					MinValue = int.MinValue;
					break;

				case Input.UInt:
					MinValue = uint.MinValue;
					MaxValue = uint.MaxValue;
					break;

				case Input.Long:
					MinValue = long.MinValue;
					MaxValue = long.MaxValue;
				default:
					break;
			}*/

			if (InputType == Input.Double || InputType == Input.UDouble)
			{
				SingleUseKeys += ',';
			}

			if (InputType == Input.Int || InputType == Input.Double || InputType == Input.Long || InputType == Input.BigInteger)
			{
				SingleUseKeys += '-';
			}

			MaxValue ??= InputType == Input.UInt ? uint.MaxValue : InputType == Input.Int ? int.MaxValue : InputType == Input.Long ? long.MaxValue : InputType == Input.BigInteger ? null : -MinValue;
			MinValue ??= InputType == Input.UInt ? uint.MinValue : InputType == Input.Int ? int.MinValue : InputType == Input.Long ? long.MinValue : InputType == Input.BigInteger? null: -MaxValue;

			if (MaxValue == null || MinValue == null) MaxSymbols = null;
			MaxSymbols ??= MaxValue.ToString().Length; //TODO: Добавить проверку для double
			if (MaxSymbols == 0) MaxSymbols = null;

			SUKcount = SingleUseKeys.Length;

			SUKused = new bool[SUKcount];
			SUKused.Initialize();

			string WhitelistKeys = "12345678790";
			foreach (char Key in SingleUseKeys)
			{
				WhitelistKeys += Key;
			}

			while (true)
			{
				Continue2:
				CurrentKey = ReadKey(true);
				//if (CurrentKey == '\0') continue;
				Stop = false;
				Read = false;

				if (CurrentKey.Key == ConsoleKey.Backspace)
				{
					if (InputString == "") continue;
					if (Print)
						if (CursorLeft == 0)
						{
							CursorTop -= 1;
							CursorLeft = BufferWidth-1;
							Console.Write(" ");
						} 
						else Console.Write("\b \b");

					for (int i = 0; i < SUKcount; i++)
						if (InputString[InputString.Length - 1] == SUKcount)
							SUKused[i] = false;

					InputString = InputString[..^1];
					continue;
				} 

				foreach (var Key in StopReadKeys)
				{
					//список клавиш завершения ввода
					if (Key != CurrentKey.Key) continue;

					ro_keyPressed = Key;
					ro_keyType = ReadKeyType.StopKey;
					Stop = true;
					break;
					//if (ThrewStopKeys) InputString = InputString + ((int)CurrentKey.Key).ToString().FormatString(3,TypesFuncs.Positon.right,'0');
				}

				if(!Stop) foreach (var Key in CancelReadKeys)
				{
					//список клавиш завершения ввода
					if (Key != CurrentKey.Key) continue;

					ro_keyPressed = Key;
					ro_keyType = ReadKeyType.CancelKey;
					Stop = true;
					break;
					//if (ThrewStopKeys) InputString = InputString + ((int)CurrentKey.Key).ToString().FormatString(3,TypesFuncs.Positon.right,'0');
				}

				if (Stop)
				{
					if (!string.IsNullOrEmpty(Placeholder))
					{
						InputString = Placeholder;
						CursorLeft += InputString.Length;
					}

					if (AllowEmptyString||!string.IsNullOrEmpty(InputString)) break;
				}
				else
				{
					if (InputType != Input.String)
					{
						foreach (char Key in WhitelistKeys)
						{
							//список разрешенных клавиш
							if (Key == CurrentKey.KeyChar) Read = true;
						}

						for (int i = 0; i < SUKcount; i++)
							if (CurrentKey.KeyChar == SingleUseKeys[i])
								if (SUKused[i]) Read = false;
								else SUKused[i] = true;
					}
					else Read = true;

					if (Read)
					{
						if (Placeholder != null)
						{
							ReWrite(" ".Multiply(Placeholder.Length), RestoreCurPos: true);
							Placeholder = null;
						}

						InputString += CurrentKey.KeyChar;

						if (MaxSymbols != null && (InputString.Length > MaxSymbols ||
						                           (InputString.Length == MaxSymbols && String.Compare(InputString, ((InputString[0] == '-') ? MinValue : MaxValue).ToString()) == 1))) //: Вводимая строка больше MaxValue
						{
							Error($"Ввод игнорируется, поскольку вводимое число не может быть больше {MaxValue}", p, out isErrorPrinted);
							InputString = InputString[..^1];
						}
						else if (Print) Console.Write(CurrentKey.KeyChar);
					}
					else
					{
						Error($"Клавиша \"{CurrentKey.KeyChar}\" недопустима", p, out isErrorPrinted);
					}
				}
			}

			/*double? ro_double = null;
			long? ro_int = null;
			if (InputType is Input.Int or Input.UInt) ro_int = InputString.ToNIntT();
			if (InputType is Input.Double or Input.UDouble) ro_double = InputString.ToNDoubleT();
			if (InputType is Input.Long) ro_int= InputString.ToLongT();*/

			ReadOutput ro = new ReadOutput(ro_keyPressed, ro_keyType, InputString);

			Clr();
			if (isErrorPrinted)
			{
				CPV cpv = p.ErrorPosition == int.MaxValue ? CPV.Bottom : p.ErrorPosition == int.MinValue ? CPV.Up : CPV.None;
				int LinesUp = cpv == CPV.None? p.ErrorPosition : 0;
				if (p.isErrorPosition_Absolute) cpv = CPV.Up;
				ReWrite(TextColor: p.ErrorTextColor,LinesUp: LinesUp, CurPosH: CPH.Left, CurPosV: cpv);
			}
			return ro;
		}

		public static string AskString(this ReadOutput ro) => ro.KeyType is ReadKeyType.CancelKey ? null : ro.String();
		public static double? AskDouble(this ReadOutput ro) => ro.KeyType is ReadKeyType.CancelKey ? null : ro.Double();
		public static int? AskInt(this ReadOutput ro) => ro.KeyType is ReadKeyType.CancelKey ? null : (int)ro.Int();
		public static long? AskLong(this ReadOutput ro) => ro.KeyType is ReadKeyType.CancelKey ? null : ro.Int();

		private class ReadT_PrintErrorParametrs
		{
			public bool ShowError = false;
			public c ErrorTextColor = c.red;
			public int ErrorPosition = Int32.MaxValue;
			public bool isErrorPosition_Absolute = false;

			public ReadT_PrintErrorParametrs(bool showError, c errorTextColor, int errorPosition, bool isErrorPositionAbsolute)
			{
				ShowError = showError;
				ErrorTextColor = errorTextColor;
				ErrorPosition = errorPosition;
				isErrorPosition_Absolute = isErrorPositionAbsolute;
			}
		}

		/// <summary>
		/// This's obsolete, use ReadT() instead
		/// </summary>
		/// <param name="InputString"></param>
		/// <param name="InputType"></param>
		/// <param name="StopReadKeys"></param>
		/// <param name="ThrewStopKeys"></param>
		/// <param name="Print"></param>
		/// <param name="TextColor"></param>
		/// <returns></returns>
		public static string ChangeRead(ref string InputString,Input InputType = Input.String, string StopReadKeys = "\n\r", bool ThrewStopKeys = false, bool Print = true, c TextColor = c.orange)
		{ //:25.08.2022 refactoring
			bool curvis = CursorVisible;
			CursorVisible = true;
			ConsoleColor OldColor = Console.ForegroundColor;
			Clr(TextColor);
			if (InputType == Input.Bool) return qu(InputString).ToRuString();

			char CurrentKey;
			bool Stop, Read;
			string SingleUseKeys = "";
			bool[] SUKused;
			int SUKcount = 0;
			InputString ??= "";
			if (InputType is Input.Double or Input.UDouble)
			{
				SingleUseKeys += ',';
				SUKcount++;
			}

			if (InputType is Input.Int or Input.Double)
			{
				SingleUseKeys += '-';
				SUKcount++;
			}

			SUKused = new bool[SUKcount];
			SUKused.Initialize();

			string WhitelistKeys = "12345678790";
			foreach (char Key in SingleUseKeys)
			{
				WhitelistKeys += Key;
			}

			while (true)
			{
				CurrentKey = ReadKey(true).KeyChar;
				if (CurrentKey == '\0') continue;
				Stop = false;
				Read = false;

				if (CurrentKey == '\b')
				{
					if (InputString == "") continue;
					if (Print) Console.Write("\b \b");
					for (int i = 0; i < SUKcount; i++)
						if (InputString[InputString.Length - 1] == SUKcount)
							SUKused[i] = false;

					InputString = InputString.Remove(InputString.Length - 1);
					continue;
				} 

				foreach (char Key in StopReadKeys)
				{
					if (Key == CurrentKey) //список клавиш завершения ввода
					{
						Stop = true;
						if (ThrewStopKeys) InputString = CurrentKey + InputString;
					}
				}

				if (InputType != Input.String)
				{
					foreach (char Key in WhitelistKeys)
					{
						//список разрешенных клавиш
						if (Key == CurrentKey) Read = true;
					}

					for (int i = 0; i < SUKcount; i++)
						if (CurrentKey == SingleUseKeys[i])
							if (SUKused[i]) Read = false;
							else SUKused[i] = true;
				}

				if (Stop && InputString != "") break;
				else if (Read || InputType ==  Input.String)
				{
					InputString += CurrentKey;
					if(Print) Console.Write(CurrentKey);
				}
			}

			Clr();
			CursorVisible = curvis;
			return InputString;
		}
		/// <summary>
		/// Input type. String is default and can't have Whitelist keys. For using Whitelist keys, choose CustomString
		/// </summary>
		public enum Input //:08.11.2021
		{
			UInt,
			Int,
			Long,
			BigInteger,
			UDouble,
			Double,
			String,
			Bool,
			StringArray,
			CustomString
		}

		public static void WaitKey(string ActionText = "продолжить",c TextColor = c.Null,  c BackgroundColor = c.Null, int LinesUp = -1, int ShiftRight = 0, bool ClearLine = true, CPH CurPosH = CPH.Left, CPV CurPosV = CPV.None, c ClearLineColor = c.Default, bool RestoreCurPos = false)
		{
			ReWrite($"Нажмите любую клавишу, чтобы {ActionText}", TextColor: TextColor,  BackgroundColor: BackgroundColor,  LinesUp: LinesUp,  ShiftRight: ShiftRight, ClearLine: ClearLine, CurPosH: CurPosH, CurPosV: CurPosV, ClearLineColor: ClearLineColor, RestoreCurPos: RestoreCurPos);
			ReadKey();
		}

		public static string GetFilepath(string Extension, Action BeforeReadAction = null, Action AfterReadAction = null) => GetFilepath(new[] { Extension }, BeforeReadAction, AfterReadAction); //:25.08.2022 Created
		public static string GetFilepath(IEnumerable<string> Extensions, Action BeforeReadAction = null, Action AfterReadAction = null) //:25.08.2022 Created
		{
			if (!Extensions.Any()) throw new ArgumentException("Extensions list can't be empty", nameof(Extensions));
			Extensions = Extensions.Add(".", false);
			while (true)
			{
				var cursorPosition = GetCurPos();
				BeforeReadAction?.Invoke();

				var ro = ReadT();
				if (ro.KeyType == ReadKeyType.CancelKey) return null;

				AfterReadAction?.Invoke();
				var inputString = ro.String();
				var filepath = inputString.Slice("\"", "\"", true);
				FileInfo fileInfo;
				if (filepath.IsNullOrEmpty())
				{
					SetCurPos(cursorPosition);
					continue;
				}
				else if (!File.Exists(filepath))
				{
					ReColor(filepath, c.silver);
					ReWrite("\nФайл, который вы ввели, не существует", c.red, ClearLine: true);
					ReWrite(" (повторите попытку)", c.Default);
					SetCurPos(cursorPosition);
					continue;
				}

				if (!Extensions.Contains(new FileInfo(filepath).Extension))
				{
					ReColor(inputString, c.silver); ReColor(inputString.Slice(".",LastStart:true), c.red);
					ReWrite(new []{"\nРасширение файла не совпадает"," (должно быть ", Extensions.ToStringLine(" или "),")"}, new []{c.red,c.gray,c.cyan,c.gray}, ClearLine: true);
					ReWrite(" (повторите попытку)", c.Default);
					SetCurPos(cursorPosition);
					continue;
				}

				return filepath;
			}
		}
		#endregion

		#region Output

		/// <summary>
		/// Cursor position horisontal
		/// </summary>
		public enum CPH
		{
			None,
			Left,
			Right,
			Center,
			Absolute = Left
		}

		/// <summary>
		/// cursor position vertical
		/// </summary>
		public enum CPV
		{
			None,
			Up,
			Top = Up,
			Down,
			Bottom = Down,
			Center,
			Absolute = Up
		}



		public static void ClearWindow(int? LinesCount = null, int? StartPos = null)
		{
			RClr();
			ReWrite();
			if(StartPos != null) CursorTop = (int)StartPos;
			LinesCount ??= BufferHeight - CursorTop;
			for (int i = 0; i < LinesCount; i++)
			{
				CursorTop++;
				ReWrite(CurPosH: CPH.Left);
			}
		}



		public static void ReColor(string[] Strings, c[] TextColors = null, c[] BackgroundColors = null, int LinesUp = 0, int ShiftRight = 0, bool ClearLine = false, CPH CurPosH = CPH.None, CPV CurPosV = CPV.None, c ClearLineColor = c.Default, bool RestoreCurPos = false) =>
			ReWrite(Strings, TextColors: TextColors, BackgroundColors: BackgroundColors, LinesUp: LinesUp, ShiftRight: (int)(ShiftRight-Strings.LongLength), ClearLine: ClearLine, CurPosH: CPH.None, CurPosV: CPV.None, ClearLineColor: ClearLineColor);

		public static void ReColor (string String, c TextColor, c BackgroundColor = c.Null, c ClearLineColor = c.Default,
			int LinesUp = 0, int ShiftRight = 0, bool ClearLine = false)
			=> ReWrite(String, TextColor: TextColor, BackgroundColor: BackgroundColor, LinesUp: LinesUp, ShiftRight: ShiftRight-String.Length, ClearLine: ClearLine, CurPosH: CPH.None, CurPosV: CPV.None, ClearLineColor: ClearLineColor);

		public static void ReWriteLine(string String, c TextColor, c BackgroundColor = c.Null, c ClearLineColor = c.Default,
			int LinesUp = 0, int ShiftRight = 0, bool ClearLine = false, CPH CurPosH = CPH.None, CPV CurPosV = CPV.None)
			=> ReWrite("\n"+String, TextColor: TextColor, BackgroundColor: BackgroundColor, LinesUp: LinesUp, ShiftRight: ShiftRight, ClearLine: ClearLine, CurPosH: CurPosH, CurPosV: CurPosV, ClearLineColor: ClearLineColor);

		/// <summary>
		/// Заменяет содержимое консоли строкой String, находящейся на LinesUp строк выше, чем текущая и на ShiftRight символов правее, с выравниванием по ширине curPosH и по высоте curPosV
		/// </summary>
		/// <param name="String">строка, которой заменяется текст у указанной позиции. Если не задано, то очищается строка срава от курсора</param>
		/// <param name="TextColor">Цвет текста String</param>
		/// <param name="BackgroundColor">Цвет фона String</param>
		/// <param name="LinesUp">Насколько строк выше нужно поднять текст (отрицательные значения – опусить ниже)</param>
		/// <param name="ShiftRight">Изменение позиции курсора от текущего положения</param>
		/// <param name="ClearLine">Очистить ли строку, на которую будет выведен текст</param>
		/// <param name="CurPosH">Позиция текста по горизонтали (по умолчанию – не изменять)</param>
		/// <param name="CurPosV">Позиция текста по вертикали (по умолчанию – не изменять)</param>
		/// <param name="ClearLineColor">Цвет фона пустой строки</param>
		/// <param name="RestoreCurPos"></param>
		public static void ReWrite(string String = "//JustCle@rLine", c TextColor = c.Null, c BackgroundColor = c.Null, int LinesUp = 0, int ShiftRight = 0, bool ClearLine = false, CPH CurPosH = CPH.None, CPV CurPosV = CPV.None, c ClearLineColor = c.Default, bool RestoreCurPos = false) 
		{ //:22.08.2022
//bool curvi = CursorVisible;
//CursorVisible = true;
			
//TODO: реализовать остальные CursorPosition
			if(String == null) return;
			if (String.Contains('\n'))
			{
				string[] strings = String.Split('\n');

				for (int i = 0; i < strings.Length-1; i++)
				{
					ReWrite(strings[i],TextColor: TextColor,BackgroundColor: BackgroundColor,LinesUp: LinesUp, ShiftRight: ShiftRight, ClearLine: ClearLine, CurPosH: CurPosH, CurPosV: CurPosV, ClearLineColor: ClearLineColor);
					CursorTop++;
					CursorLeft = 0;
				}

				if (strings[^1].Length > 0) ReWrite(strings[^1],TextColor: TextColor,BackgroundColor: BackgroundColor,LinesUp: LinesUp,ShiftRight: ShiftRight, ClearLine: ClearLine, CurPosH: CurPosH, CurPosV: CurPosV, ClearLineColor: ClearLineColor, RestoreCurPos: RestoreCurPos);

				return;
			}

			if (String == "//JustCle@rLine")
			{
				String = "";
				ClearLine = true;
			}

			c fClr = (c)Console.ForegroundColor;
			c bClr = (c)Console.BackgroundColor;

			int curLeft = CursorLeft,
				curTop = CursorTop;
			switch (CurPosH) //TODO: добавить сброс на ноль или BufferWidth/WindowHeight, если выходит за этот диапазон
			{
				case CPH.None:
					CursorLeft += ShiftRight;
					break;

				case CPH.Left:
					CursorLeft = Math.Abs(ShiftRight);
					break;

				case CPH.Right:
					CursorLeft = BufferWidth - String.Length; //TODO: создать функцию для подсчёта печатных символов строки
					CursorLeft -= Math.Abs(ShiftRight);
					break;

				case CPH.Center:
					CursorLeft = (BufferWidth - String.SymbolsCount())/2;
					CursorLeft += ShiftRight;
					break;
			}

			switch (CurPosV)
			{
				case CPV.None:		break;
				case CPV.Up:		CursorTop = 0; break;
				case CPV.Bottom:	CursorTop = BufferHeight - 1; break;
				case CPV.Center:	CursorTop = (BufferHeight - 1) / 2; break;
			}

			if (CurPosV == CPV.Absolute) CursorTop += Math.Abs(LinesUp);
			else CursorTop -= LinesUp;

			Clr(TextColor,BackgroundColor);
			Console.Write(String);


			if (ClearLine)
			{
				BClr(ClearLineColor);

				var cp = GetCurPos();
				Console.Write(new string(' ',BufferWidth - CursorLeft));
				SetCurPos(cp);
			}

			Clr(fClr, bClr);

			try
			{
				if (CurPosV == CPV.None)
					CursorTop += LinesUp;
				else CursorTop = curTop;
			}
			catch (Exception e)
			{
				CursorTop = 0;
			}

			if (RestoreCurPos) CursorLeft = curLeft;
//CursorVisible = curvi;
		}

		/// <summary>
		/// Заменяет содержимое консоли строками Strings, каждая из которых имеет соответствующий TextColors и BackgroundColors, находящейся на LinesUp строк выше, чем текущая и на ShiftRight символов правее
		/// </summary>
		/// <param name="String">строка, которой заменяется текст у указанной позиции</param>
		/// <param name="LinesUp">Насколько строк выше нужно поднять текст (отрицательные значения – опусить ниже)</param>
		/// <param name="ShiftRight">Изменение позиции курсора от текущего положения</param>
		/// <param name="ClearLine">Очистить ли строку, на которую будет выведен текст</param>
		/// <param name="CurPosH">Позиция текста по горизонтали (по умолчанию – не изменять)</param>
		/// <param name="CurPosV>Позиция текста по вертикали (по умолчанию – не изменять)</param>
		/// <param name="TextColor">Цвет текста String</param>
		/// <param name="BackgroundColor">Цвет фона String</param>
		/// <param name="ClearLineColor">Цвет фона пустой строки</param>
		public static void ReWrite(string[] Strings, c[] TextColors = null, c[] BackgroundColors = null, int LinesUp = 0, int ShiftRight = 0, bool ClearLine = false, CPH CurPosH = CPH.None, CPV CurPosV = CPV.None, c ClearLineColor = c.Default, bool RestoreCurPos = false)
		{ //:29.11.2021 Изменено поведение CPV и CPH, \n
			//TODO: объединить оба ReWrite, создав два метода до и после Write
			int StringsLength = Strings.Sum(s => s.Length);
			TextColors??= SetDefaultColors(TextColors,Strings.Length);
			BackgroundColors??= SetDefaultColors(BackgroundColors,Strings.Length);

			c[] SetDefaultColors(c[] Colors, int Length)
			{
				Colors = new c[Length];
					for (int i = 0; i < Length; i++)
					{
						Colors[i] = c.Null;
					}

					return Colors;
			}

			if (!(Strings.Length == TextColors.Length && Strings.Length == BackgroundColors.Length))
				throw new ArgumentOutOfRangeException($"Strings[{Strings.Length}] should be same size as TextColors[{TextColors.Length}] and BackgroundColors[{BackgroundColors.Length}]");

			//bool curvi = CursorVisible;
//CursorVisible = true;
			/*if (String.Contains('\n'))
			{
				string[] strings = String.Split('\n');

				for (int i = 0; i < strings.Length-1; i++)
				{
					ReWrite(strings[i],LinesUp,ShiftRight,ClearLine,CurPosH,CurPosV,TextColors,BackgroundColor,ClearLineColor);
					CursorTop++;
				}

				if (strings[^1].Length > 0) ReWrite(strings[^1],LinesUp,ShiftRight,ClearLine,CurPosH,CurPosV,TextColor,BackgroundColor,ClearLineColor);

				return;
			}*/

			c fClr = (c)Console.ForegroundColor;
			c bClr = (c)Console.BackgroundColor;

			int curLeft = CursorLeft,
				curTop = CursorTop;
			switch (CurPosH) //TODO: добавить сброс на ноль или BufferWidth/WindowHeight, если выходит за этот диапазон
			{
				case CPH.None:
					CursorLeft += ShiftRight;
					break;

				case CPH.Left:
					CursorLeft = Math.Abs(ShiftRight);
					break;

				case CPH.Right:
					CursorLeft = BufferWidth - StringsLength; //TODO: создать функцию для подсчёта печатных символов строки
					CursorLeft -= Math.Abs(ShiftRight);
					break;

				case CPH.Center:
					CursorLeft = (BufferWidth - Strings.Sum(s => s.SymbolsCount()))/2;
					CursorLeft += ShiftRight;
					break;
			}

			switch (CurPosV)
			{
				case CPV.None:		CursorTop -= LinesUp; break;
				case CPV.Up:		CursorTop = LinesUp; break;
				case CPV.Bottom:	CursorTop = BufferHeight - 1 - LinesUp; break;
				case CPV.Center:	CursorTop = (BufferHeight - 1) / 2 - LinesUp; break;
			}

			for (int i = 0; i < Strings.Length; i++)
			{
				Clr(TextColors[i],BackgroundColors[i]);
				Console.Write(Strings[i]);
				if (ClearLine&&(Strings[i].Contains("\n")||(i==Strings.Length-1&&Strings[i]!="\n")))
				{
					BClr(ClearLineColor);
				
					ReWrite(new string(' ',BufferWidth - CursorLeft), RestoreCurPos:true);
				}
			}
			


			Clr(fClr, bClr);

			try
			{
				if (CurPosV == CPV.None)
					CursorTop += LinesUp;
				else CursorTop = curTop;
			}
			catch (Exception e)
			{
				CursorTop = 0;
			}
			

			if (RestoreCurPos) CursorLeft = curLeft;
			else
				try
				{
					if (CurPosH == CPH.None)
						if (ClearLine) CursorLeft = curLeft +ShiftRight + StringsLength;
						/*else CursorLeft -= (ShiftRight + StringsLength);
					else CursorLeft = curLeft;*/
				}
				catch (Exception e)
				{
					CursorLeft = 0;
				}
//CursorVisible = curvi;
		}

		public static void ReWrite(string[] Strings, c[] TextColors, c BackgroundColor, int LinesUp = 0, int ShiftRight = 0, bool ClearLine = false, CPH CurPosH = CPH.None, CPV CurPosV = CPV.None, c ClearLineColor = c.Default, bool RestoreCurPos = false)
		{
			var BackgroundColors = new c[TextColors.Length];

			for (int i = 0; i < BackgroundColors.Length; i++)
			{
				BackgroundColors[i] = BackgroundColor;
			}

			ReWrite(Strings,TextColors,BackgroundColors,LinesUp,ShiftRight,ClearLine,CurPosH,CurPosV,ClearLineColor,RestoreCurPos);
		}

		public static void Error(string ErrorText, string ActionText = "продолжить")
		{
			ReWrite("\nОшибка: " + ErrorText, c.red); 

			WaitKey(ActionText);
		}

		public static void Write(this Exception e, string ActionText = "продолжить") => Error(e.ToString(), ActionText);

		public static bool Error(bool Term, string ErrorText, string ActionText = "продолжить")
		{
			if (!Term) return false;

			Error(ErrorText, ActionText);
			return true;

		}

		public static void Error(string ErrorText,int SleepTime)
		{
			ReWrite("Ошибка: " + ErrorText, c.red);
			Thread.Sleep(SleepTime);
		}

		public static bool Error(bool Term, string ErrorText, int SleepTime) 
		{
			if (!Term) return false;

			Error(ErrorText,SleepTime);
			return true;

		}

		public static void SetCurPos(int Left, int Top)
		{
			CursorLeft = Left;
			CursorTop = Top;
		}

		public static void SetCurPos(Point point)
		{
			SetCurPos(point.X,point.Y);
		}

		public static Point GetCurPos() => new Point(CursorLeft, CursorTop);

		#endregion

		#region Complex

		/// <summary>
		/// Writes menu from current cursor position
		/// </summary>
		/// <param name="Options">Menu options. First and last should always be active</param>
		/// <param name="cursor">Current selected option</param>
		/// <param name="TipString">Подсказка, которая отображается внизу, на последней строке консоли</param>
		/// <param name="TipStringColor"></param>
		/// <param name="CancelKeys">Кнопки, которые отменяют выбор опциию. По умолчанию, их нет</param>
		/// <param name="SelectKeys">Кнопки, которые выберают опцию. По умолчанию, Enter и Spacebar (пробел)</param>
		/// <param name="ClearLine">Очистить ли все строки, где будет напечатано меню? Если да, то выделение текущей опции распространиться на всю строку</param>
		/// <returns>Chosed option or -CancelKey if pressed</returns>
		public static int Menu(Option[] Options, int cursor = 0,string TipString = null, c TipStringColor = c.green, ConsoleKey[] CancelKeys = null, ConsoleKey[] SelectKeys = null, bool ClearLine = false, bool SelectWholeLine = false) //:28.11.21
		{
			//BUG: Исправить ClearLine баг, доделать поддержку ClearLine
			SelectKeys ??= new[] {ConsoleKey.Enter, ConsoleKey.Spacebar};

			CursorVisible = false;
			int curLeftStart = CursorLeft,
				curTopStart = CursorTop;

			ConsoleKeyInfo curKey;
			//: Вывод опций на экран
			for (int i = 0; i < Options.Length; i++)
			{
				bool active = Options[i]._active;
				ReWrite(new []{i + ". ",Options[i]._text + "\n"}, new []{(active ? c.orange : c.gray),(active ? c.Default : c.gray)}, (i==cursor ? SelectColor : c.Default),ClearLine:ClearLine,ClearLineColor:SelectWholeLine? SelectColor : c.Default);
				
			}

			int LastConsoleLine =  BufferHeight - 1;
			ReWrite(TipString,TextColor: TipStringColor, LinesUp: LastConsoleLine, ShiftRight: 0, ClearLine: ClearLine, CurPosH: CPH.None, CurPosV: CPV.Top);

			bool Exit = false;
			//: Выбор опции (навигация в меню)
			do
			{
				CursorVisible = false; //: Почему-то иногда сбрасывается, поэтому в цикле
				int oldCur = cursor;
				curKey = ReadKey(true);
				switch (curKey.Key)
				{
					case ConsoleKey.UpArrow:
					{
						if (cursor > 0)
						{
							do
							{
								cursor--;
							} while (!Options[cursor]._active);
						}
					}
						break;

					case ConsoleKey.DownArrow:
					{
						if (cursor < Options.Length - 1)
						{
							do
							{
								cursor++;
							} while (!Options[cursor]._active);
						}
					}
						break;

					default:
					{
						if (curKey.KeyChar.IsDigit())
						{
							cursor = curKey.KeyChar.ToIntT();
							if (!Options[cursor]._active) cursor = oldCur; //:Если опция неактивна, отменить выбор
							else Exit = true;
						}

						if (CancelKeys != null && CancelKeys.Contains(curKey.Key))
						{
							return -curKey.KeyChar;
						}
					}
						break;
				}

				if (cursor != oldCur)
				{
					ReWrite(new string[]{oldCur + ". ",Options[oldCur]._text}, new c[]{c.orange, c.Default},c.Default, curTopStart + oldCur,curLeftStart,ClearLine,CPH.Absolute, CPV.Absolute); //:Затирание выделения предыдущей строки
					ReWrite(new []{cursor + ". ",Options[cursor]._text}, new []{c.orange, c.Default},SelectColor, curTopStart + cursor,curLeftStart,ClearLine,CPH.Absolute, CPV.Absolute); //:Выделение новой строки
				}

			} while ((!SelectKeys.Contains(curKey.Key))&&!Exit);

			CursorVisible = true;
			return cursor;
		}

		/// <summary>
		/// Меню изменения настроек
		/// </summary>
		/// <param name="settings"></param>
		public static void Menu(ref Setting[] settings, bool ClearLine = true)
		{

			if (settings.Length < 1)
			{
				ReWrite("\nНастроек не найдено", TextColor: c.red);
				return;
			}

			CursorVisible = false;
			int curLeftStart = 0,
				curTopStart = CursorTop;

			CursorLeft = 0;
			int cursor = 0;
			ConsoleKeyInfo curKey;
			//Вывод опций на экран
			for (int i = 0; i < settings.Length; i++)
			{
				if (i == cursor)
					if (settings[i].Disabled)
						if (cursor<settings.Length)
						{
							cursor++; continue;
						}
						else
						{
							ReWrite("Все настройки недоступны для изменения");
							return;
						}


				var ColorText = (settings[i].Disabled ? c.gray : c.Default);
				var ColorValue = (settings[i].Disabled? c.gray : c.yellow);

				ReWrite(new []{settings[i].Text+": ",settings[i].Value,settings[i].Unit+"\n"}, new []{ColorText, ColorValue, ColorText});
				

				if (i == cursor)
				{
					BClr();
					ReWrite(String: settings[i].Description, TextColor: c.green, CurPosH: CPH.Left, CurPosV: CPV.Bottom);
				}
			}


			c WeakSelectColor = c.Null;
			//Выбор опции (навигация в меню)
			string GetCurrentListValueString(int currentListIndex1, string[] strings)
			{
				string s;
				if (currentListIndex1 != 0) s = "← ";
				s = strings[currentListIndex1 + 1];
				if (currentListIndex1 != strings.Length - 1) s += " →";
				return s;
			}

			do
			{
				string[] list = null;
				int oldCur = cursor;
				curKey = ReadKey(true);
				switch (curKey.Key)
				{
					case ConsoleKey.UpArrow:
					{
						if (cursor > 0)
						{
							do
							{
								cursor--;
							} while (settings[cursor].Disabled && cursor <= settings.Length-1); //TODO: добавить проверку на конец массива
						}
					} break;

					case ConsoleKey.DownArrow:
					{
						if (cursor < settings.Length - 1)
						{
							do
							{
								cursor++;
							} while (settings[cursor].Disabled && cursor < settings.Length-1);
						}
					} break;

					case ConsoleKey.Enter:
						var valueCurLeft = CursorLeft;
						ref var curSetting = ref settings[cursor];
						ref var value = ref curSetting.Value;
						BClr(WeakSelectColor);
						ReWrite( curSetting.Text+": ", TextColor: c.Default, CurPosH: CPH.Left);
						string valStr;
						
						if (curSetting.Type == Input.StringArray)
						{
							list = value.Split('\0');
							int currentListIndex = list[0].ToIntT();
							valStr = GetCurrentListValueString(currentListIndex, list);
						}
						else if (curSetting.Type == Input.Bool) valStr = value.ToBool() ? "← Да " : "Нет →";
						else valStr = value;
						
						ReWrite(valStr,TextColor: c.orange, LinesUp: 0, ShiftRight: -(value.Length+curSetting.Unit.Length), ClearLine: true);
						CursorLeft -= curSetting.Unit.Length;

						ReWrite("Нажмите Enter, чтобы завершить ввод",TextColor: c.gray,BackgroundColor: c.Default, LinesUp: 0, ShiftRight: 0, ClearLine: true, CurPosH: CPH.Left, CurPosV: CPV.Bottom);
						if (curSetting.Type == Input.StringArray)
						{
							int currentListIndex = list[0].ToIntT();
							ConsoleKey key;
							do
							{
								key = ReadKey(true).Key;
								switch (key)
								{
									case ConsoleKey.LeftArrow:
										if(currentListIndex != 0) currentListIndex--;
									break;

									case ConsoleKey.RightArrow:
										if (currentListIndex != list.Length - 2) currentListIndex++;

									break;
								}

								CursorLeft = valueCurLeft;
								ReWrite(GetCurrentListValueString(currentListIndex, list), TextColor: c.yellow);
							} while (key!=ConsoleKey.Enter&&key!=ConsoleKey.Escape);

							ReWrite(new []{settings[cursor].Text+": ", settings[cursor].Value, settings[cursor].Unit}, new []{c.Default, c.yellow, c.Default}, SelectColor,ClearLine:true, CurPosH:CPH.Left);
						} 
						else if (curSetting.Type == Input.Bool)
						{
							ConsoleKey key;
							do
							{
								Clr(c.orange);
								key = ReadKey(true).Key;
								switch (key)
								{
									case ConsoleKey.LeftArrow:
										if (value.ToBool() == true)
										{
											value = "Нет";
											ReWrite("Нет →", LinesUp: 0, ShiftRight: -5);
										}

										break;

									case ConsoleKey.RightArrow:
										if (value.ToBool() == false)
										{
											value = "Да";
											ReWrite("← Да ", LinesUp: 0, ShiftRight: -5);
										}

										break;
								}
							} while (key!=ConsoleKey.Enter&&key!=ConsoleKey.Escape);
						
							ReWrite(new []{settings[cursor].Text+": ", settings[cursor].Value, settings[cursor].Unit}, new []{c.Default,c.yellow,c.Default}, CurPosH:CPH.Left,ClearLine:true);
						}
						else ChangeRead(ref value, curSetting.Type);

						break;

					default:
					{
					}
						break;
				}

				if (cursor != oldCur)
				{

					ReWrite(new []{settings[oldCur].Text+": ", settings[oldCur].Value,settings[oldCur].Unit}, new []{c.Default,c.orange, c.Default},c.Default, curTopStart + oldCur,curLeftStart,ClearLine,CPH.Absolute, CPV.Absolute); //:Затирание выделения предыдущей строки
					ReWrite(new []{settings[oldCur].Text+": ", settings[oldCur].Value,settings[oldCur].Unit}, new []{c.Default,c.orange, c.Default},SelectColor, curTopStart + cursor,curLeftStart,ClearLine,CPH.Absolute, CPV.Absolute); //:Выделение новой строки
					ReWrite(String: settings[cursor].Description, TextColor: c.green, ClearLine: true, CurPosH: CPH.Left, CurPosV: CPV.Bottom); //:Вывод подсказки
				}

			} while (curKey.Key != ConsoleKey.Escape);

			CursorVisible = true;
		}

		#endregion
		
	}
}

#region Classes

	public class Loadbar
	{
		internal string BorderLeftSymbol{ get; }
		internal c BorderLeftColor { get; }
		internal string BorderRightSymbol{ get; }
		internal c BorderRightColor { get; }
		internal string LineSymbol { get; }
		internal c LineColor { get; }
		internal string EmptyLineSymbol  { get; }
		public int MaxLoadLineLength { get; }
		public int LoadbarPositionLeft { get; internal set; }
		internal int Cursor { get; set; }
		public int LoadbarPositionTop { get; internal set; }
		internal int CurrentLoadlineLength { get; set; }


		public Loadbar(int LoadlineLength = 10, string borderLeftSymbol = "[", string borderRightSymbol = "]", string lineSymbol = "—", string emptySymbol = " ", c borderLeftColor = c.cyan, c borderRightColor = c.cyan, c lineColor = c.lime)
		{
			if (lineSymbol.Length < 1) throw new ArgumentException("lineSymbol should have at least 1 symbol");
			if (lineSymbol.Length != emptySymbol.Length) throw new ArgumentException("lineSymbol and emptySymbol should be same length");
			LoadbarPositionLeft = CursorLeft;
			LoadbarPositionTop = CursorTop;
			Cursor = CursorLeft + BorderLeftSymbol.Length;
			CurrentLoadlineLength = 0;

			LineSymbol = lineSymbol;
			EmptyLineSymbol = emptySymbol;
			BorderLeftSymbol = borderLeftSymbol;
			BorderRightSymbol = borderRightSymbol;
			int MaxPossibleLineLength = (BufferWidth - (LoadbarPositionLeft + borderLeftSymbol.Length + borderRightSymbol.Length)) / lineSymbol.Length;
			MaxLoadLineLength = Math.Min(LoadlineLength*LineSymbol.Length, MaxPossibleLineLength);
			BorderLeftColor = borderLeftColor;
			BorderRightColor = borderRightColor;
			LineColor = lineColor == c.lime ? c.lime : lineColor;
		}

		public void Reset(bool ResetPosition = false)
		{
			CurrentLoadlineLength = 0;
			if (ResetPosition)
			{
				this.LoadbarPositionLeft = CursorLeft;
				this.LoadbarPositionTop = CursorTop;
			}
			this.Write();
		}

		public void Write(int Increments = 0, bool ClearLine = false)
		{
			if (Increments < 0) Increments = 0;
			{
				int MaxIncrements = this.MaxLoadLineLength - this.CurrentLoadlineLength;
			}
			if (Increments > this.MaxLoadLineLength) Increments = this.MaxLoadLineLength;
			if (this.CurrentLoadlineLength == 0)
			{
				if (this == null) throw new ArgumentNullException("LoadbarSettings can't be null");
				if (this.CurrentLoadlineLength == 0) ReWrite(new[] { this.BorderLeftSymbol, this.EmptyLineSymbol.Multiply(this.MaxLoadLineLength), this.BorderRightSymbol }, new[] { this.BorderLeftColor, this.LineColor, this.BorderRightColor }, c.Default, -this.LoadbarPositionTop, this.LoadbarPositionLeft, ClearLine, CPH.Left, CPV.Up, RestoreCurPos: true);
			}

			if (Increments > 0)
			{
				int cl = CursorLeft, ct = CursorTop;
				ReWrite(this.LineSymbol.Multiply(Increments),TextColor: this.LineColor, LinesUp: this.LoadbarPositionTop, ShiftRight: this.Cursor, ClearLine: ClearLine, CurPosH: CPH.Left, CurPosV: CPV.Up);
				this.CurrentLoadlineLength = CursorLeft - this.LoadbarPositionLeft;
				this.Cursor = CursorLeft;
				CursorLeft = cl;
			}
		}

		public void Write(float AddPercent)
		{
			if (AddPercent < 0) AddPercent = 0;
			if (AddPercent > 100) AddPercent = 100;
			this.Write((int)(this.MaxLoadLineLength*(AddPercent/100)));
		}
	}


	public static class SettingsFunctions
	{
		public static void Enable(this Setting[] S, string name)
		{
			foreach (var s in S)
			{
				s.Enable(name);
			}
		}
		public static void Disable(this Setting[] S, string name)
		{
			foreach (var s in S)
			{
				s.Disable(name);
			}
		}
		public static string Get(this Setting[] S, string name)
		{
			string R;
			foreach (var s in S)
			{
				R = s.Get(name);
				if (R != null) return R;
			}

			return null;
		}
	}

	public class Setting //TODO: Добавить dependency
	{
		public string Text;
		public string Description;
		public string Name;
		public Input Type;
		public string Value;
		public string Unit; //:the unit of the value || Единица измерения
		public bool Disabled;

		/// <summary>
		/// Стандартный конструктор для создания опции настроек
		/// </summary>
		/// <param name="name">Имя параметра</param>
		/// <param name="type">Тип</param>
		/// <param name="value">Значение по умолчанию</param>
		/// <param name="description">Описание</param>
		/// <param name="unit">Единица измерения</param>
		/// <param name="disabled">Доступен ли параметр для изменения</param>
		/// <param name="Text">Выводимый текст, при выборе параметра (если не выбрано, то = имени параметра)</param>
		public Setting(string name, Input type = Input.Int, string value = null, string description = "", string unit = "", bool disabled = false, string Text =  null)
		{
			Text ??= name;
			Name = name;
			Type = type;
			Description = description;
			Unit = unit;
			Value = value;
			Disabled = disabled;
		}
		
		/// <summary>
		/// Конструктор для типа StringArray (выпадающего списка)
		/// </summary>
		///  <param name="name">Имя параметра</param>
		/// <param name="listValues">Список возможных значений</param>
		/// <param name="defaultValue">Порядковый номер значения по умолчанию</param>
		///<param name="description">Описание</param>
		/// <param name="unit">Единица измерения</param>
		/// <param name="disabled">Доступен ли параметр для изменения</param>
		/// <param name="Text">Выводимый текст, при выборе параметра (если не выбрано, то = имени параметра)</param>
		public Setting(string name, string[] listValues, int defaultValue = 0, string description = "", string unit = "", bool disabled = false, string Text =  null)
		{
			if (defaultValue >= listValues.Length) throw new ArgumentOutOfRangeException($"defaultValue ({defaultValue}) can't be more than the count of listValues ({listValues.Length})");
			if (listValues.Length<=1) throw  new ArgumentOutOfRangeException("listValues length should be more than 1");

			Text ??= name;
			Name = name;
			Type = Input.StringArray;
			Description = description;
			Unit = unit;
			Value = defaultValue + "\0" + String.Join('\0', listValues);
			Disabled = disabled;
		}

		/// <summary>
		/// Конструктор для типа StringArray (выпадающего списка) с отдельным описанием для каждой опции
		/// </summary>
		/// <param name="name">Имя параметра</param>
		/// <param name="listValues">Список возможных значений</param>
		/// <param name="defaultValue">Порядковый номер значения по умолчанию</param>
		///<param name="descriptions">Описание для каждого параметра.</param>
		/// <param name="unit">Единица измерения</param>
		/// <param name="disabled">Доступен ли параметр для изменения</param>
		/// <param name="Text">Выводимый текст, при выборе параметра (если не выбрано, то = имени параметра)</param>
		/// <param name="sharedDescription">Общее описание. Вставляется вначале каждого отдельного описания (если есть)</param>
		public Setting(string name, string[] listValues, int defaultValue = 0, string[] descriptions = null, string unit = "", bool disabled = false, string sharedDescription = null, char Separator = ' ', string Text =  null)
		{
			if (defaultValue >= listValues.Length) throw new ArgumentOutOfRangeException($"defaultValue ({defaultValue}) can't be more than the count of listValues ({listValues.Length})");
			if (listValues.Length<=1) throw  new ArgumentOutOfRangeException("listValues length should be more than 1");
			if (descriptions == null||descriptions.Length <=1) throw new ArgumentOutOfRangeException("description length should be more than 1");

			Text ??= name;
			Name = name;
			Type = Input.StringArray;
			Description = "\0" + String.Join('\0', descriptions) + ((sharedDescription == null)? ' ' + sharedDescription + "\0" : "");
			Unit = unit;
			Value = defaultValue + "\0" + String.Join('\0', listValues) + "\0";
			Disabled = disabled;
		}

		public string Get(string name)
		{
			if (Name == name) return Value;
			
			return null;
		}

		public string Get()
		{
			if (Type != Input.StringArray) return Value;
			var list = Value.Split('\0');

			return list[list[0].ToIntT() + 1];
		}

		public void Enable(string name)
		{
			if (Name == name) Disabled = false;
		}
		public void Disable(string name)
		{
			if (Name == name) Disabled = true;
		}
	}

	public static class OptionsFunctions
	{
		public static void Activate(this Option[] ops, int dependency)
		{
			foreach (var op in ops)
			{
				op.Activate(dependency);
			}
		}
		public static void Deactivate(this Option[] ops, int dependency)
		{
			foreach (var op in ops)
			{
				op.Deactivate(dependency);
			}
		}
	}

	public class Option
	{
		public string _text;
		public int _dependency;
		public bool _active;
		public string _name;

		public Option(string text, string name = null, int dependency = 0, bool? active = null)
		{
			_text = text;
			_name = name;
			_dependency = dependency;
			_active = active?? dependency == 0; //:if active unset, set true if dependecy == 0
		}

		public void Activate() => _active = true;
		public void Activate(int dependency)
		{
			if (dependency == _dependency) _active = true;
		}
		public void Activate(string Name)
		{
			if (Name == _name) _active = true;
		}
		
		public void Deactivate() => _active = false;
		public void Deactivate(int dependency)
		{
			if (dependency == _dependency) _active = false;
		}
		public void Deactivate(string Name)
		{
			if (Name == _name) _active = false;
		}
	}

#endregion

//:Мусорка

//private static string[,] FormatStringForRewrite(string String, int count = 0)
//{
//	int start, end, i=0;
//	string tempString, endString;
//	if (count == 0) throw new NotImplementedException(); //Пусть ищет количество "@{" в строке
//	string[] outStrings = new string[count];
//	string[] outcolors = new string[count];

//	while (true)
//	{
//		start = String.IndexOf("@{");
//		if (start<=0) break;
//		tempString = String.Substring(start);
//		//do TODO: добавить исключающий символ
//		//{
//		//	end = tempString.IndexOf("}");
//		//	if (tempString[end-1]!= '\\') break;
//		//	endString = tempString.Substring(end);
//		//}
//		end = tempString.IndexOf(";");

//	} 
//	return  outStrings
//}

//public class Input {
//	public static string Uint = "12345678790";
//	public static string Int = "12345678790-";
//	public static string Udouble = "12345678790,";
//	public static string Double = "12345678790,-";
//}