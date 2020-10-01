using System;
using UnityEngine;
using MEC;
using System.Collections.Generic;
using Exiled.API.Features;

namespace AdminTools
{
    public class RegenerationComponent : MonoBehaviour
    {
        private Player player;
        CoroutineHandle Handle;
        public void Awake()
        {
            player = Player.Get(gameObject);
            Handle = Timing.RunCoroutine(HealHealth(player));
            Plugin.RgnHubs.Add(player, this);
        }

        public void OnDestroy()
        {
            Timing.KillCoroutines(Handle);
            Plugin.RgnHubs.Remove(player);
        }

        public IEnumerator<float> HealHealth(Player ply)
        {
            while (true)
            {
                if (ply.Health < ply.MaxHealth)
                    ply.Health += Plugin.HealthGain;
                else
                    ply.Health = ply.MaxHealth;

                yield return Timing.WaitForSeconds(Plugin.HealthInterval);
            }
        }
    }
}