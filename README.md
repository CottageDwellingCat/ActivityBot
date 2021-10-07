# Activity Bot
Activity bot allows you to use experimatal discord features called embeded activities. Embeded activities are games which are played from discord voice calls. you can add the bot to your server [here](https://discord.com/api/oauth2/authorize?client_id=895378477052751883&permissions=1048577&redirect_uri=about%3Ablank&scope=bot%20applications.commands). Currently this bot only works on the desktop version of discord, and is only tested on the latest windows canary. 
## Permissions
### Create Instant Invite
The trick that allows ActivityBot to launch games requires creating a special type invite to a voice channel, so it needs permission to create invites to work.
### Connect
Discord requires the connect permission to create instant invites in voice channels. 
## Trouble Shooting
### Commands Not Sending
Generaly this means the bot crashed and (some how?????) didn't start itself again, create an issue and I'll have it fixed pretty quick
### Commands Stuck on "ActivityBot is thinking"
This can happen when ActivityBot is getting a lot of commands at the same time, when this happens you can wait four or five minuets for things to calm down, then try again.
### Clicking the Button does nothing
This is generaly caused by an incompatable client. Make sure you are using the latest version of the **Desktop or Web** client. If that still doesn't work try using the [canary](https://support.discord.com/hc/en-us/articles/360035675191-Discord-Testing-Clients) client. If nothing happens and you are on windows make sure the regestry value at `Computer\HKEY_CURRENT_USER\Software\Classes\Discord\shell\open\command` is a valid discord client executable (matching the pattern `"C:\Users\Name\AppData\Local\Discord<ptb,canary,nothing>\app-1.0.42\Discord<ptb,canary,nothing>.exe" --url -- "%1"`. If that doesen't fix your problem file a bug report at [dtesters](https://discord.gg/discord-testers) 
### Activites loading infinitly
This is a discord issue, try restarting your client and waiting a few hours.
### 'Invalid Interaction Application Command' Error
This is a Discord issue that happens when I update the internal list of activites. Restarting your client should fix the issue, but if it doesen't kicking and readding the bot will reset the guild cache (hopefully) updating everything. In the worst case you will have to wait two hours for the discord datacenter in your region to sync commands.
### Wrong Activity being launched
This has the same cuase and sollution as the Invalid Interaction Application Commad Error.
## supported games
Check out this dictionary
```cs
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
 ```
## Random Photos
![image](https://user-images.githubusercontent.com/80918250/136310677-136c6db1-df24-49de-93b3-10447942e9e4.png)
![image](https://user-images.githubusercontent.com/80918250/136310710-4bcab1f8-aac6-4432-a046-321fc51c7723.png)
> The windows screenshot tool made blurple plurple for some reason
