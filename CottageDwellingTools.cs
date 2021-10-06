using System;
using System.Collections.Generic;
using System.IO;
using Discord.Webhook;
using Discord;

/// <summary>
/// CottageDwellingCats custom tools
/// </summary>
namespace CottageDwellingTools
{
	public static class Logger
	{
		/// <summary>
		/// the max length of a log level
		/// </summary>
		public const int MaxLogLevelLength = 8;
		/// <summary>
		/// the max length of a source value
		/// </summary>
		public const int MaxSourceLength = 15;
		/// <summary>
		/// the name of the latest log file
		/// </summary>
		public const string LogFileName = "&latest.catLog";
		/// <summary>
		/// the name of older log files
		/// </summary>
		public const string OldLogFileName = "logx.catLog";
		/// <summary>
		/// the different level of logs
		/// </summary>
		public enum LogLevel
		{
			/// <summary>
			/// logs that would be unnecessary on a production build
			/// </summary>
			Verbose,
			/// </summary>
			/// logs detailing normal operation
			/// </summary>
			Info,
			/// </summary>
			/// logs detailing a recoverable or unimportant fail of normal operation (eg disconnected from gateway)
			/// </summary>
			Warning,
			/// </summary>
			/// logs detailing a non-recoverable or important fail of normal operation (eg a user command couldent be run)
			/// </summary>
			Error,
			/// </summary>
			/// logs detailing a critical fail of any operation (eg a crash, uncaught exception thrown on the main thread, ect)
			/// </summary>
			Exception,
			/// </summary>
			/// logs detailing something critically important that does not fit nicely into any other log level
			/// </summary>
			Notice
		}

		/// <summary>
		/// what color different loglevels should be written out as in the console
		/// </summary>
		/// <value></value>
		public static readonly Dictionary<LogLevel, ConsoleColor> LevelColors = new Dictionary<LogLevel, ConsoleColor>
		{
			{ LogLevel.Verbose,		ConsoleColor.DarkGray	},
			{ LogLevel.Info,		ConsoleColor.Gray		},
			{ LogLevel.Warning,		ConsoleColor.DarkYellow },
			{ LogLevel.Error,		ConsoleColor.Red		},
			{ LogLevel.Exception,	ConsoleColor.DarkRed	},
			{ LogLevel.Notice,		ConsoleColor.Cyan		},
		};
		
		public static readonly Dictionary<Discord.LogSeverity, LogLevel> DiscordLevelEq = new Dictionary<Discord.LogSeverity, LogLevel>
		{
			{ Discord.LogSeverity.Verbose,	LogLevel.Verbose 	},
			{ Discord.LogSeverity.Debug,	LogLevel.Verbose 	},
			{ Discord.LogSeverity.Info, 	LogLevel.Info		},
			{ Discord.LogSeverity.Warning, 	LogLevel.Warning	},
			{ Discord.LogSeverity.Error, 	LogLevel.Error		},
			{ Discord.LogSeverity.Critical, LogLevel.Exception	},
		};
		
		public static readonly Dictionary<LogLevel, Color> EmbedColorsEq = new Dictionary<LogLevel, Color>
		{
			{ Logger.LogLevel.Verbose, new Color(0) },
			{ Logger.LogLevel.Info, new Color(444) },
			{ Logger.LogLevel.Warning, Color.Orange },
			{ Logger.LogLevel.Error, Color.Red },
			{ Logger.LogLevel.Exception, Color.DarkRed },
			{ Logger.LogLevel.Notice, Color.Blue },
		};
		/// <summary>
		/// The directory log files should be placed in
		/// </summary>
		public static string LogFileLocation { get; private set; }
		/// <summary>
		/// true if logs should be placed in a file
		/// </summary>
		public static bool DoFileLogging { get; private set; } = false;
		/// <summary>
		/// true if logs should be placed in the console
		/// </summary>
		public static bool DoConsoleLogging { get; private set; } = true;
		/// <summary>
		/// a list of all messages that have been logged
		/// </summary>
		public static List<LogMessage> Messages { get; private set; }
		/// <summary>
		/// a list of all messages that have been logged (in string form)
		/// </summary>
		public static List<string> MessagesAsString { get; private set; }
		/// <summary>
		/// true if the logger was initialized, and can accsept logs
		/// </summary>
		public static bool Initialized { get; private set; } = false;
		
		/// <summary>
		/// the webhook that should be used for sending messages
		/// </summary>
		public static DiscordWebhookClient WebhookClient { get; private set; } = null;
		/// <summary>
		/// true if the logger was initialized, and can accsept logs
		/// </summary>
		public static LogLevel WebhookMinLevel{ get; private set; } = Logger.LogLevel.Error;
		
		#if DEBUG
		/// <summary>
		/// the mininum loglevel for messages that will be stored
		/// </summary>
		public static LogLevel MinLevel { get; private set; } = LogLevel.Verbose;
		#else
		/// <summary>
		/// the mininum loglevel for messages that will be stored
		/// </summary>
		public static LogLevel MinLevel { get; private set; } = LogLevel.Info;
		#endif

		/// <summary>
		/// log a message
		/// </summary>
		/// <param name="source">what system/function/ect sent the log (eg. logger, commandArgParse)</param>
		/// <param name="message">what is the message you wish to log</param>
		/// <param name="level">what <c>LogLevel</c> best describes your message</param>
		public static void Log(string source, string message, LogLevel level = LogLevel.Info)
		{
			if (Initialized = false || (int)level < (int)MinLevel)
				return;
			// create the log message and store it
			LogMessage logMessage = new LogMessage(level, source, message);
			Messages.Add(logMessage);
			MessagesAsString.Add(logMessage.ToString());

			// write to the file
			if (DoFileLogging)
				File.WriteAllLines(LogFileLocation + LogFileName, MessagesAsString.ToArray());

			// write to the console
			if (DoConsoleLogging)
			{
				// get the correct color, and log the level
				ConsoleColor color = ConsoleColor.Red;
				LevelColors.TryGetValue(level, out color);
				Console.ForegroundColor = color;
				Console.WriteLine(logMessage.ToString());
				Console.ResetColor();
			}
			
			// send a webhook message 
			if(WebhookClient != null && (int)level >= (int)WebhookMinLevel)
			{
				EmbedBuilder eb = new EmbedBuilder()
					.WithColor(EmbedColorsEq.GetValueOrDefault(logMessage.Level, Color.Blue))
					.WithTimestamp(logMessage.Time)
					.WithAuthor(logMessage.Level.ToString())
					.WithTitle(source)
					.WithDescription(logMessage.Message);
				WebhookClient.SendMessageAsync(embeds: new Embed[] {eb.Build()});
			}
		}

		/// <summary>
		/// where should log messages be stored
		/// </summary>
		public enum LogLocation
		{
			/// <summary>
			/// write log messages only to a file
			/// </summary>
			File,
			/// <summary>
			/// write log messages only to the console
			/// </summary>
			Console,
			/// <summary>
			/// write log messages only to both the console and a file
			/// </summary>
			Both
		}
		/// <summary>
		/// initialize the logger
		/// </summary>
		/// <param name="minLevel">the minimum loglevel that will be stored and written to the console</param>
		/// <param name="locations">where messages will be logged</param>
		/// <param name="fileLogLocation">what dirrectory logmessages will be stored in</param>
		/// <param name="keepOldLogs">should old log files be kept</param>
		public static void InitializeLogger(LogLevel? minLevel = null, LogLocation locations = LogLocation.Console, string fileLogLocation = "", bool keepOldLogs = false,
											string webhookUrl = "", LogLevel webhookMinLevel = LogLevel.Error)
		{
			int locationVal = (int)locations;
			if(locationVal == 0)
			{
				DoConsoleLogging = false;
				DoFileLogging = true;
				LogFileLocation = fileLogLocation;
			}
			else
			if(locationVal == 1)
			{
				DoConsoleLogging = true;
				DoFileLogging = false;
				fileLogLocation = "";
			}
			else
			{
				DoConsoleLogging = true;
				DoFileLogging = true;
				LogFileLocation = fileLogLocation;
			}
			Messages = new List<LogMessage>();
			MessagesAsString = new List<string>();
			Initialized = true;
			MinLevel = minLevel ?? MinLevel;

			string movedFileTo = "";
			bool MentionFileMove = false;
			// make sure file writing is functioning
			if (DoFileLogging)
			{
				// make sure the dirrectory is valid
				Directory.CreateDirectory(fileLogLocation);
				if (!fileLogLocation.EndsWith("\\")) fileLogLocation += "\\";
				// save the old log
				if (File.Exists(LogFileLocation + LogFileName) && keepOldLogs)
				{
					string fileName = LogFileLocation + OldLogFileName.Replace("x", (Directory.GetFiles(fileLogLocation).Length + 1).ToString());
					movedFileTo += fileName;
					MentionFileMove = true;
					File.Copy(LogFileLocation + LogFileName, fileName);
				}
			}

			if (!String.IsNullOrWhiteSpace(webhookUrl))
			{
				WebhookClient = new DiscordWebhookClient(webhookUrl);
				WebhookMinLevel = webhookMinLevel;
			}

			if(MentionFileMove)
			Log("logger", $"moved old log file to : {movedFileTo}", LogLevel.Verbose);
			Log("logger", $"initialized logger with log option : {locations}", LogLevel.Notice);
			
		}

		/// <summary>
		/// a messages sent to the logger
		/// </summary>
		public class LogMessage
		{ 
			/// <summary>
			/// the loglevel of the current message
			/// </summary>
			public LogLevel Level { get; private set; }
			/// <summary>
			/// what system/function/ect sent the log (eg. logger, commandArgParse)
			/// </summary>
			public string Source { get; private set; }
			/// <summary>
			/// the message represented by this logMessage
			/// </summary>
			public string Message { get; private set; }
			/// <summary>
			/// when this messages was logged
			/// </summary>
			public DateTimeOffset Time { get; private set; }
			public override string ToString()
			{
				string time = Time.ToUnixTimeSeconds().ToString();
				string level = $"[{Level}]";
				string source = "";
				for (int i = 0; i < MaxSourceLength; i++)
					source += (i < Source.Length) ? Source.ToCharArray()[i].ToString() : " ";
				return $"{time} : {source} {Message}";
			}

			/// <summary>
			/// create a new logMessage
			/// </summary>
			/// <param name="level">the loglevel of the current message</param>
			/// <param name="source">what system/function/ect sent the log (eg. logger, commandArgParse)</param>
			/// <param name="message">the message</param>
			public LogMessage(LogLevel level, string source, string message)
			{
				Level = level;
				Source = source;
				Message = message;
				Time = DateTimeOffset.UtcNow;
			}
		}
	}

	public static class Extentions
	{
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach(T value in collection)
			{
				action(value);
			}
		}
		public static bool ContainsArray<T>(this List<T[]> arrays, T[] test)
		{
			try
			{
				foreach(T[] testAgainst in arrays)
				{
					bool goodMatch = true;

					for(int i = 0; i < testAgainst.Length; i++)
						if(!testAgainst[i].Equals(test[i]))
							goodMatch = false;

					if(goodMatch)
					return true;
				}
			}
			catch
			{
				return false;
			}
			return false;
		}
	}
}