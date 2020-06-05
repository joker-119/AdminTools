using System;
using UnityEngine;
using EXILED;
using EXILED.Extensions;
using UnityEngine.Serialization;

namespace AdminTools
{
    public class BreakDoorComponent : MonoBehaviour
    {
        public ReferenceHub hub;
        public bool breakAll = false;
        string[] unbreakableDoorNames = { "079_FIRST", "079_SECOND", "372", "914", "CHECKPOINT_ENT", "CHECKPOINT_LCZ_A", "CHECKPOINT_LCZ_B", "GATE_A", "GATE_B", "SURFACE_GATE" };

        public void Awake()
        {
            Events.DoorInteractEvent += OnDoorInteract;
            Events.PlayerLeaveEvent += OnLeave;
            hub = gameObject.GetPlayer();
        }

        private void OnLeave(PlayerLeaveEvent ev)
        {
            if (ev.Player == hub)
                Destroy(this);
        }

        private void OnDoorInteract(ref DoorInteractionEvent doorInter)
        {
            if (doorInter.Player != hub)
                return;

            if (!unbreakableDoorNames.Contains(doorInter.Door.DoorName))
                BreakDoor(doorInter.Door);
            else if (breakAll)
                BreakDoor(doorInter.Door);
        }

        private void BreakDoor(Door door)
        {
            door.Networkdestroyed = true;
            door.DestroyDoor(true);
            door.destroyed = true;
        }

        public void OnDestroy()
        {
            Events.DoorInteractEvent -= OnDoorInteract;
            Events.PlayerLeaveEvent -= OnLeave;
            hub.SetBypassMode(false);
            breakAll = false;
            Plugin.BdHubs.Remove(hub);
        }
    }
}
