using System;
using System.Collections.Generic;
using System.IO;
using EXILED;

namespace AdminTools
{
	public class Plugin : EXILED.Plugin
	{
		public EventHandlers EventHandlers;
		public List<Jailed> JailedPlayers = new List<Jailed>();
		public string OverwatchFilePath;
		public string HiddenTagsFilePath;
		
		public override void OnEnable()
		{
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string pluginPath = Path.Combine(appData, "Plugins");
			string path = Path.Combine(pluginPath, "AdminTools");
			string overwatchFileName = Path.Combine(path, "AdminTools-Overwatch.txt");
			string hiddenTagFileName = Path.Combine(path, "AdminTools-HiddenTags.txt");

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
		}

		public override void OnDisable()
		{
			Events.RemoteAdminCommandEvent -= EventHandlers.OnCommand;
			Events.PlayerJoinEvent -= EventHandlers.OnPlayerJoin;
			Events.RoundEndEvent -= EventHandlers.OnRoundEnd;
			EventHandlers = null;
		}

		public override void OnReload()
		{
			
		}

		public override string getName { get; } = "AdminTools";
	}
}