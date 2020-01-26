using Assets._Scripts.Dissonance;
using Harmony;

namespace AdminTools.Patches
{
	[HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.CallCmdAltIsActive))]
	public class Scp049HumanSpeech
	{
		public static void Prefix(DissonanceUserSetup __instance, bool value)
		{
			if (!Plugin.Scp049Speak) 
				return;
			CharacterClassManager ccm = __instance.gameObject.GetComponent<CharacterClassManager>();
			if (ccm.CurClass == RoleType.Scp049 || ccm.CurClass.Is939()) 
				__instance.MimicAs939 = value;
		}
	}
}