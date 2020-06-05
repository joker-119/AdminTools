using EXILED;
using EXILED.Extensions;
using System;
using UnityEngine;

namespace AdminTools
{
    public class InstantKillComponent : MonoBehaviour
    {
        public ReferenceHub hub;
        public void Awake()
        {
            hub = gameObject.GetPlayer();
            Events.PlayerHurtEvent += RunWhenPlayerIsHurt;
            Events.PlayerLeaveEvent += OnLeave;
            Plugin.IkHubs.Add(hub, this);
        }
        
        private void OnLeave(PlayerLeaveEvent ev)
        {
            if (ev.Player == hub)
                Destroy(this);
        }

        public void OnDestroy()
        {
            Events.PlayerHurtEvent -= RunWhenPlayerIsHurt;
            Events.PlayerLeaveEvent -= OnLeave;
            Plugin.IkHubs.Remove(hub);
        }

        public void RunWhenPlayerIsHurt(ref PlayerHurtEvent plyHurt)
        {
            if (plyHurt.Attacker != plyHurt.Player && plyHurt.Attacker == hub) 
                plyHurt.Amount = int.MaxValue;
        }
    }
}