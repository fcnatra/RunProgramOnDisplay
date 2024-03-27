using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace RunProgram
{
	class RunProgramOnDisplay
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;        // x position of upper-left corner
			public int Top;         // y position of upper-left corner
			public int Right;       // x position of lower-right corner
			public int Bottom;      // y position of lower-right corner
		}

		static int Width(RECT rect) => rect.Right - rect.Left;
		static int Height(RECT rect) => rect.Bottom - rect.Top;
		static int GetDisplay(string firstArgument) => int.Parse(firstArgument.Substring(firstArgument.Length - 1)) - 1;
		static string GetArgumentsArgument(string[] args) => args.Length >= 3 ? string.Join(' ', args.Except(new List<string> { args[0], args[1] })) : "";

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

		[DllImport("user32.dll")]
		public static extern int GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern IntPtr GetDesktopWindow();


		static void Main(string[] args)
		{
			if (!ArgumentsAreValid(args))
			{
				ShowHelp();
				return;
			}

			var process = Process.Start(args[1], GetArgumentsArgument(args));
			Thread.Sleep(2000);

			IntPtr id = (IntPtr)GetForegroundWindow();
			GetWindowRect(id, out RECT winSize);

			IntPtr desktop = GetDesktopWindow();
			GetWindowRect(desktop, out RECT desktopSize);

			MoveWindow(id, GetDisplay(args[0]) * Width(desktopSize), 0, Width(winSize), Height(winSize), true);
			Thread.Sleep(2000);
		}

		private static void ShowHelp()
		{
			Console.WriteLine("Something's happening with the arguments.");
			Console.WriteLine("Here is how to use me:");
			Console.WriteLine("\tRunProgramOnDisplay <screen<1|2>> <program's full path> [arguments]");
			Console.WriteLine("\nExample: RunProgramOnDisplay screen1 C:\\Windows\\System32\\cmd.exe /C echo test");
		}

		private static bool ArgumentsAreValid(string[] args)
		{
			if (args is null || args.Length < 2)
				return false;
				
			bool result = args[0].StartsWith("screen", StringComparison.InvariantCultureIgnoreCase) && (args[0].EndsWith('1') || args[0].EndsWith('2'));
			result = File.Exists(args[1]);
			
			return result;
		}
	}
}
