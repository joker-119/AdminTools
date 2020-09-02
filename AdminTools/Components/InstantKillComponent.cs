using Exiled.API.Features;
using Exiled.Events.EventArgs;
using UnityEngine;
using Handlers = Exiled.Events.Handlers;

namespace AdminTools
{
    public class InstantKillComponent : MonoBehaviour
    {
        public Player player;
        public void Awake()
        {
            player = Player.Get(gameObject);
            Handlers.Player.Hurting += RunWhenPlayerIsHurt;
            Handlers.Player.Left += OnLeave;
            Plugin.IkHubs.Add(player, this);
        }

        private void OnLeave(LeftEventArgs ev)
        {
            if (ev.Player == player)
                Destroy(this);
        }

        public void OnDestroy()
        {
            Handlers.Player.Hurting -= RunWhenPlayerIsHurt;
            Handlers.Player.Left -= OnLeave;
            Plugin.IkHubs.Remove(player);
        }

        public void RunWhenPlayerIsHurt(HurtingEventArgs ev)
        {
            if (ev.Attacker != ev.Target && ev.Attacker == player)
                ev.Amount = int.MaxValue;
        }
    }
}