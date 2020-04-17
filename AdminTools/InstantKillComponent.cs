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
        }

        public void OnDestroy()
        {
            Events.PlayerHurtEvent -= RunWhenPlayerIsHurt;
        }

        public void RunWhenPlayerIsHurt(ref PlayerHurtEvent plyHurt)
        {
            if (plyHurt.Attacker != plyHurt.Player && plyHurt.Attacker == hub)
            {
                plyHurt.Amount = int.MaxValue;
                return;
            }
        }
    }
}
