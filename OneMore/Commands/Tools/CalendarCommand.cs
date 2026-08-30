//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;


	/// <summary>
	/// Invokes  the OneMore Calendar app showing pages that were created and modified on each day
	/// </summary>
	internal class CalendarCommand : Command
	{
		public CalendarCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var processes = Process.GetProcessesByName("OneMoreCalendar");
			if (processes.Any())
			{
				logger.WriteLine("OneMoreCalendar already running, activating window");
				var handle = processes[0].MainWindowHandle;
				Native.SetForegroundWindow(handle);
				Native.ShowWindow(handle, Native.SW_RESTORE);
				return;
			}

			// presume same location as executing addin assembly

			var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var path = Path.Combine(location, "OneMoreCalendar.exe");

			// special override for development and debugging
			if (!File.Exists(path))
			{
				path = Path.Combine(
					location.Substring(0, location.LastIndexOf("OneMore")),
					@"OneMoreCalendar\bin\Debug\OneMoreCalendar.exe");
			}

			try
			{
				logger.WriteLine($"starting {path}");
				Process.Start(path);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error starting calendar at {path}", exc);
			}

			await Task.Yield();
		}
	}
}
