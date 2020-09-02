using System.Collections.Generic;
using UnityEngine;

namespace AdminTools
{
	public class Jailed
	{
		public string Userid;
		public string Name;
		public List<Inventory.SyncItemInfo> Items;
		public RoleType Role;
		public Vector3 Position;
		public float Health;
	}
}