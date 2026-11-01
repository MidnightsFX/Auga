using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace Auga
{
    /// <summary>
    /// Widgets a dependent mod builds with Jotunn's GUIManager (see <see cref="AutoRestyle"/>). Jotunn's Create*
    /// methods style what they create through its Apply*Style methods, so patching those covers both; CreateScrollView
    /// is the one that styles nothing itself. Jotunn's own UI is left alone: its frames are passed over, and the
    /// caller then is no dependent.
    /// </summary>
    public static class AutoRestyle_Jotunn
    {
        [HarmonyPatch]
        public static class ApplyStyle
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(GUIManager).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(m => m.Name.StartsWith("Apply") && m.Name.EndsWith("Style") && m.GetParameters().Length > 0 && typeof(Component).IsAssignableFrom(m.GetParameters()[0].ParameterType))
                    .Cast<MethodBase>();
            }

            public static void Postfix(object[] __args)
            {
                if (__args.Length > 0 && __args[0] is Component target && target != null)
                {
                    var owner = AutoRestyle.Caller();
                    if (owner != null)
                        AutoRestyle.RestyleNow(target.gameObject, owner);
                }
            }
        }

        [HarmonyPatch(typeof(GUIManager), nameof(GUIManager.CreateScrollView))]
        public static class CreateScrollView
        {
            public static void Postfix(GameObject __result)
            {
                if (__result == null)
                    return;
                var owner = AutoRestyle.Caller();
                if (owner != null)
                    AutoRestyle.RestyleNow(__result, owner);
            }
        }
    }
}
