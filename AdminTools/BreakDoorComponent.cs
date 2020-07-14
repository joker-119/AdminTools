using Exiled.API.Features;
using Exiled.Events.EventArgs;
using UnityEngine;
using Handlers = Exiled.Events.Handlers;

namespace AdminTools
{
    public class BreakDoorComponent : MonoBehaviour
    {
        public Player player;
        public bool breakAll = false;
        string[] unbreakableDoorNames = { "079_FIRST", "079_SECOND", "372", "914", "CHECKPOINT_ENT", "CHECKPOINT_LCZ_A", "CHECKPOINT_LCZ_B", "GATE_A", "GATE_B", "SURFACE_GATE" };

        public void Awake()
        {
            Handlers.Player.InteractingDoor += OnDoorInteract;
            Handlers.Player.Left += OnLeave;
            player = Player.Get(gameObject);
            player.IsBypassModeEnabled = true;
        }

        private void OnLeave(LeftEventArgs ev)
        {
            if (ev.Player == player)
                Destroy(this);
        }

        private void OnDoorInteract(InteractingDoorEventArgs ev)
        {
            if (ev.Player != player)
                return;

            if (!unbreakableDoorNames.Contains(ev.Door.DoorName))
                BreakDoor(ev.Door);
            else if (breakAll)
                BreakDoor(ev.Door);
        }

        private void BreakDoor(Door door)
        {
            door.Networkdestroyed = true;
            door.DestroyDoor(true);
            door.destroyed = true;
        }

        public void OnDestroy()
        {
            Handlers.Player.InteractingDoor -= OnDoorInteract;
            Handlers.Player.Left -= OnLeave;
            player.IsBypassModeEnabled = false;
            breakAll = false;
            Plugin.BdHubs.Remove(player);
        }
    }
}
