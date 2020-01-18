using System;
using System.Reflection;

namespace AdminTools
{
	public static class Extensions
	{
		public static void RAMessage(this CommandSender sender, string message, bool success = true) =>
			sender.RaReply("AdminTools#" + message, success, true, string.Empty);
		
		public static void InvokeStaticMethod(this Type type, string methodName, object[] param)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic |
			                     BindingFlags.Static | BindingFlags.Public;
			MethodInfo info = type.GetMethod(methodName, flags);
			info?.Invoke(null, param);
		}
		
		public static void Broadcast(this ReferenceHub rh, uint time, string message) =>
			rh.GetComponent<Broadcast>()
				.TargetAddElement(rh.scp079PlayerScript.connectionToClient, message, time, false);
	}
}