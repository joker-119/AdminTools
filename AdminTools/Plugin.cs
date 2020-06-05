using System;
using System.Collections.Generic;
using System.IO;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events;
using Handlers = Exiled.Events.Handlers;

namespace AdminTools
{
	public class Plugin : Exiled.API.Features.Plugin
	{
		public EventHandlers EventHandlers;
		public List<Jailed> JailedPlayers = new List<Jailed>();
		public static Dictionary<Player, InstantKillComponent> IkHubs = new Dictionary<Player, InstantKillComponent>();
		public static Dictionary<Player, BreakDoorComponent> BdHubs = new Dictionary<Player, BreakDoorComponent>();
		public string OverwatchFilePath;
		public string HiddenTagsFilePath;
		public bool GodTuts;
		public static bool Scp049Speak;
		public static int PatchCounter;
		public Config Cfg;
		
		public override void OnEnabled()
		{
			try
			{
				string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				string pluginPath = Path.Combine(appData, "Plugins");
				string path = Path.Combine(pluginPath, "AdminTools");
				string overwatchFileName = Path.Combine(path, "AdminTools-Overwatch.txt");
				string hiddenTagFileName = Path.Combine(path, "AdminTools-HiddenTags.txt");

				Cfg = (Config) Config;
				Cfg.Reload();

				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				if (!File.Exists(overwatchFileName))
					File.Create(overwatchFileName).Close();

				if (!File.Exists(hiddenTagFileName))
					File.Create(hiddenTagFileName).Close();

				OverwatchFilePath = overwatchFileName;
				HiddenTagsFilePath = hiddenTagFileName;

				EventHandlers = new EventHandlers(this);
				Handlers.Server.SendingRemoteAdminCommand += EventHandlers.OnCommand;
				Handlers.Player.Joined += EventHandlers.OnPlayerJoin;
				Handlers.Server.RoundEnded += EventHandlers.OnRoundEnd;
				Handlers.Player.TriggeringTesla += EventHandlers.OnTriggerTesla;
				Handlers.Player.ChangingRole += EventHandlers.OnSetClass;
				Handlers.Server.WaitingForPlayers += EventHandlers.OnWaitingForPlayers;
			}
			catch (Exception e)
			{
				Log.Error($"Loading error: {e}");
			}
		}

		public override void OnDisabled()
		{
			Handlers.Server.SendingRemoteAdminCommand -= EventHandlers.OnCommand;
			Handlers.Player.Joined -= EventHandlers.OnPlayerJoin;
			Handlers.Server.RoundEnded -= EventHandlers.OnRoundEnd;
			Handlers.Player.TriggeringTesla -= EventHandlers.OnTriggerTesla;
			Handlers.Player.ChangingRole -= EventHandlers.OnSetClass;
			Handlers.Server.WaitingForPlayers -= EventHandlers.OnWaitingForPlayers;
			EventHandlers = null;
		}

		public override void OnReloaded()
		{
			
		}

		public override IConfig Config { get; }
	}
}
