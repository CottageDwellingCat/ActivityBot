using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.API;
using Discord.Rest;
using Discord.Net;
using System.Collections.Generic;
using CottageDwellingTools;
using System.Linq;

namespace ActivityBot
{
	class Program
	{	
		public static Dictionary<int, ulong> ActivityIds = new Dictionary<int, ulong>
		{
			{ 1, 755827207812677713 }, // Poker Night
			{ 2, 773336526917861400 }, // Betrayal.io
			{ 3, 814288819477020702 }, // Fishington.io
			{ 4, 832012774040141894 }, // Chess in the Park
			{ 5, 878067389634314250 }, // Doodle Crew
			{ 6, 879863686565621790 }, // Letter Tile
			{ 7, 879863976006127627 }, // Word Snacks
			{ 8, 880218394199220334 }, // Watch Together
		};
		
		public static Dictionary<int, string> ActivityNames = new Dictionary<int, string>
		{
			{ 1, "Poker Night" }, 
			{ 2, "Betrayal.io" },
			{ 3, "Fishington.io" },
			{ 4, "Chess in the Park"},
			{ 5, "Doodle Crew" },
			{ 6, "Letter Tile" }, 
			{ 7, "Word Snacks" },
			{ 8, "Watch Together" }, 
		};
		
		public static readonly SlashCommandBuilder[] commands = new SlashCommandBuilder[]
		{
			new SlashCommandBuilder()
			{
				Name = "launch",
				Description = "launch an embedded activity in a voice channel",
				Options = new List<SlashCommandOptionBuilder>()
				{
					new SlashCommandOptionBuilder()
					{
						Name = "channel",
						Description = "the channel you want to launch the activity in",
						Type = ApplicationCommandOptionType.Channel,
						ChannelTypes = new List<ChannelType>()
						{
							ChannelType.Voice
						},
						Required = true
					},
					new SlashCommandOptionBuilder()
					{
						Name = "activity",
						Description = "the embedded activity you want to launch",
						Type = ApplicationCommandOptionType.Integer,
						Choices = new List<ApplicationCommandOptionChoiceProperties>()
						{
							new ApplicationCommandOptionChoiceProperties()
							{
								Name = "Poker Night",
								Value = 1
							},
							new ApplicationCommandOptionChoiceProperties()
							{
								Name = "Betrayal.io",
								Value = 2
							},
							new ApplicationCommandOptionChoiceProperties()
							{
								Name = "Fishington.io",
								Value = 3
							},
							new ApplicationCommandOptionChoiceProperties()
							{
								Name = "Chess in the Park",
								Value = 4
							},
							new ApplicationCommandOptionChoiceProperties()
							{
								Name = "Doodle Crew",
								Value = 5
							},
							new ApplicationCommandOptionChoiceProperties()
							{
								Name = "Letter Tile",
								Value = 6
							},
							new ApplicationCommandOptionChoiceProperties()
							{
								Name = "Word Snacks",
								Value = 7
							},
							new ApplicationCommandOptionChoiceProperties()
							{
								Name = "Watch Together",
								Value = 8
							}
						},
						Required = true
					},
					new SlashCommandOptionBuilder()
					{
						Name = "public",
						Description = "send the activity invite to everyone in the channel",
						Type = ApplicationCommandOptionType.Boolean,
						Required = true
					}
				}
			}
		};
		
		public static DiscordSocketClient client;
		
		static void Main(string[] args)
			=> Task.Run(async () => await Program.MainAsync()).Wait();
			
		public static async Task MainAsync()
		{
			client = new DiscordSocketClient(new DiscordSocketConfig()
			{
				AlwaysDownloadUsers = false,
				LogLevel = LogSeverity.Verbose,	
				GatewayIntents = GatewayIntents.AllUnprivileged,
				LargeThreshold = 1,
				DefaultRetryMode = RetryMode.AlwaysFail,
			});
			
			await client.LoginAsync(TokenType.Bot, System.IO.File.ReadAllText("token"));
			await client.StartAsync();
			
			client.Ready += async () =>
			{
				if ((await client.Rest.GetGlobalApplicationCommands()).Count == 0)
				{
					commands.ForEach(async x => _ = await client.Rest.CreateGlobalCommand(x.Build()));
				}
			};
			
			client.SlashCommandExecuted += async (x) =>
			{
				var args = x.Data.Options.ToArray();
				await x.DeferAsync(!(bool)args[2].Value);
				try
				{
					ulong id = (ulong)ActivityIds.GetValueOrDefault(Convert.ToInt32(args[1].Value));
					string invite = "discord://discord.gg/" + (await ((SocketVoiceChannel)args[0].Value).CreateInviteToApplicationAsync(applicationId:id, maxAge:null)).Code;
					var cb = new ComponentBuilder()
						.WithButton("Join Activity", url:invite, style:ButtonStyle.Link)
						.WithButton("Get Help", url:"https://github.com/CottageDwellingCat/ActivityBot/blob/main/README.md#trouble-shooting", style:ButtonStyle.Link);
					_ = await x.FollowupAsync("Click below to join " + ActivityNames.GetValueOrDefault(Convert.ToInt32(args[1].Value), "Unknown Activity"), component:cb.Build());
				}
				catch
				{
					var cb = new ComponentBuilder()
						.WithButton("Get Help", url:"https://github.com/CottageDwellingCat/ActivityBot/blob/main/README.md#trouble-shooting", style:ButtonStyle.Link);
					_ = await x.FollowupAsync("Something went wrong, make sure the bot has permission to create invites and connect to the chanel.", ephemeral:true, component:cb.Build());
				}
			};
			
			client.Log += async (x) => await Task.Run(() => Console.WriteLine(x.ToString()));
			
			await Task.Delay(-1);
		}
	}
}
