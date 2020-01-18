using System.Collections.Generic;
using EXILED;

namespace AdminTools
{
	public class Plugin : EXILED.Plugin
	{
		public EventHandlers EventHandlers;
		public List<Jailed> JailedPlayers = new List<Jailed>();
		
		public override void OnEnable()
		{
			EventHandlers = new EventHandlers(this);
			Events.RemoteAdminCommandEvent += EventHandlers.OnCommand;
			Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;
		}

		public override void OnDisable()
		{
			
		}

		public override void OnReload()
		{
			
		}

		public override string getName { get; } = "AdminTools";
	}
}