using System;
using System.Reflection;
using UnityEngine;

namespace AdminTools
{
	public static class Extensions
	{
		public static void InvokeStaticMethod(this Type type, string methodName, object[] param)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic |
								 BindingFlags.Static | BindingFlags.Public;
			MethodInfo info = type.GetMethod(methodName, flags);
			info?.Invoke(null, param);
		}

		public static void OldRefreshPlyModel(this CharacterClassManager ccm, GameObject player, RoleType classId = RoleType.None)
		{
			ReferenceHub hub = ReferenceHub.GetHub(player);
			hub.GetComponent<AnimationController>().OnChangeClass();
			if (ccm.MyModel != null)
			{
				UnityEngine.Object.Destroy(ccm.MyModel);
			}
			Role role = ccm.Classes.SafeGet((classId < RoleType.Scp173) ? ccm.CurClass : classId);
			if (role.team != Team.RIP)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(role.model_player, ccm.gameObject.transform, true);
				gameObject.transform.localPosition = role.model_offset.position;
				gameObject.transform.localRotation = Quaternion.Euler(role.model_offset.rotation);
				gameObject.transform.localScale = role.model_offset.scale;
				ccm.MyModel = gameObject;
				AnimationController component = hub.GetComponent<AnimationController>();
				if (ccm.MyModel.GetComponent<Animator>() != null)
				{
					component.animator = ccm.MyModel.GetComponent<Animator>();
				}
				FootstepSync component2 = ccm.GetComponent<FootstepSync>();
				FootstepHandler component3 = ccm.MyModel.GetComponent<FootstepHandler>();
				if (component2 != null)
				{
					component2.FootstepHandler = component3;
				}
				if (component3 != null)
				{
					component3.FootstepSync = component2;
					component3.AnimationController = component;
				}
				if (ccm.isLocalPlayer)
				{
					if (ccm.MyModel.GetComponent<Renderer>() != null)
					{
						ccm.MyModel.GetComponent<Renderer>().enabled = false;
					}
					Renderer[] componentsInChildren = ccm.MyModel.GetComponentsInChildren<Renderer>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].enabled = false;
					}
					foreach (Collider collider in ccm.MyModel.GetComponentsInChildren<Collider>())
					{
						if (collider.name != "LookingTarget")
						{
							collider.enabled = false;
						}
					}
				}
			}
			ccm.GetComponent<CapsuleCollider>().enabled = (role.team != Team.RIP);
			if (ccm.MyModel != null)
			{
				ccm.GetComponent<WeaponManager>().hitboxes = ccm.MyModel.GetComponentsInChildren<HitboxIdentity>(true);
			}
		}
	}
}