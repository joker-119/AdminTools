using System;
using System.Collections.Generic;
using System.IO;
using EXILED;
using Harmony;

namespace AdminTools
{
	public class Plugin : EXILED.Plugin
	{
		public EventHandlers EventHandlers;
		public List<Jailed> JailedPlayers = new List<Jailed>();
		public string OverwatchFilePath;
		public string HiddenTagsFilePath;
		public bool GodTuts;
		public static bool Scp049Speak;
		public static int PatchCounter;
		
		public override void OnEnable()
		{
			try
			{
				string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				string pluginPath = Path.Combine(appData, "Plugins");
				string path = Path.Combine(pluginPath, "AdminTools");
				string overwatchFileName = Path.Combine(path, "AdminTools-Overwatch.txt");
				string hiddenTagFileName = Path.Combine(path, "AdminTools-HiddenTags.txt");

				ReloadConfigs();

				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				if (!File.Exists(overwatchFileName))
					File.Create(overwatchFileName).Close();

				if (!File.Exists(hiddenTagFileName))
					File.Create(hiddenTagFileName).Close();

				OverwatchFilePath = overwatchFileName;
				HiddenTagsFilePath = hiddenTagFileName;

				EventHandlers = new EventHandlers(this);
				Events.RemoteAdminCommandEvent += EventHandlers.OnCommand;
				Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;
				Events.RoundEndEvent += EventHandlers.OnRoundEnd;
				Events.TriggerTeslaEvent += EventHandlers.OnTriggerTesla;
				Events.SetClassEvent += EventHandlers.OnSetClass;
				HarmonyInstance instance = HarmonyInstance.Create($"com.joker.admintools.{PatchCounter}");
				instance.PatchAll();
			}
			catch (Exception e)
			{
				Log.Error($"Loading error: {e}");
			}
		}

		private void ReloadConfigs()
		{
			GodTuts = Config.GetBool("admin_god_tuts", true);
			Scp049Speak = Config.GetBool("admin_scp049_speech", true);
		}

		public override void OnDisable()
		{
			Events.RemoteAdminCommandEvent -= EventHandlers.OnCommand;
			Events.PlayerJoinEvent -= EventHandlers.OnPlayerJoin;
			Events.RoundEndEvent -= EventHandlers.OnRoundEnd;
			Events.TriggerTeslaEvent -= EventHandlers.OnTriggerTesla;
			Events.SetClassEvent -= EventHandlers.OnSetClass;
			EventHandlers = null;
		}

		public override void OnReload()
		{
			
		}

		public override string getName { get; } = "AdminTools";
	}
}