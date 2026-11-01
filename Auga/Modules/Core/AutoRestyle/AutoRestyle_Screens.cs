using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Auga
{
    /// <summary>
    /// What a dependent mod adds to Auga's inventory, HUD and pause menu (see <see cref="AutoRestyle"/>). Once Auga
    /// has built a screen, the objects in it are recorded, at a priority just below Auga's own setup and above other
    /// mods' postfixes. When the screen opens (the HUD: a frame after its Awake) every object added since that
    /// carries a component of a dependent may be restyled, the topmost such object with what is under it. Objects
    /// that carry only Unity's components cannot be told apart by mod and are left alone.
    /// </summary>
    public static class AutoRestyle_Screens
    {
        private const int AfterAugaSetup = Priority.First - 1;
        private static readonly Dictionary<int, HashSet<int>> Recorded = new Dictionary<int, HashSet<int>>();

        private static void Record(Component screen)
        {
            if (screen == null)
                return;
            var known = new HashSet<int>();
            foreach (var t in screen.GetComponentsInChildren<Transform>(true))
                known.Add(t.GetInstanceID());
            Recorded[screen.GetInstanceID()] = known;
        }

        private static void Scan(Component screen)
        {
            if (screen == null || !AutoRestyle.Enabled || !Recorded.TryGetValue(screen.GetInstanceID(), out var known))
                return;
            // each object is looked at once: the inventory's slots and tooltips come and go every time it opens
            var taken = new List<Transform>();
            foreach (var t in screen.GetComponentsInChildren<Transform>(true))
            {
                if (!known.Add(t.GetInstanceID()) || taken.Exists(t.IsChildOf))
                    continue;
                var owner = OwnerOf(t);
                if (owner == null)
                    continue;
                taken.Add(t);
                AutoRestyle.RestyleObject(t.gameObject, owner);
            }
        }

        private static BepInEx.PluginInfo OwnerOf(Transform t)
        {
            foreach (var behaviour in t.GetComponents<MonoBehaviour>())
            {
                if (behaviour == null)
                    continue;
                var owner = AutoRestyle.DependentOf(behaviour.GetType().Assembly);
                if (AutoRestyle.ShouldRestyle(owner))
                    return owner;
            }
            return null;
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
        public static class InventoryRecord
        {
            [HarmonyPriority(AfterAugaSetup)]
            public static void Postfix(InventoryGui __instance) => Record(__instance);
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Show))]
        public static class InventoryShow
        {
            public static void Postfix(InventoryGui __instance) => Scan(__instance);
        }

        [HarmonyPatch(typeof(Menu), nameof(Menu.Start))]
        public static class MenuRecord
        {
            [HarmonyPriority(AfterAugaSetup)]
            public static void Postfix(Menu __instance) => Record(__instance);
        }

        [HarmonyPatch(typeof(Menu), nameof(Menu.Show))]
        public static class MenuShow
        {
            public static void Postfix(Menu __instance) => Scan(__instance);
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.Awake))]
        public static class HudRecord
        {
            [HarmonyPriority(AfterAugaSetup)]
            public static void Postfix(Hud __instance)
            {
                Record(__instance);
                __instance.StartCoroutine(ScanNextFrame(__instance));
            }

            // other mods' Hud.Awake postfixes run after this one
            private static IEnumerator ScanNextFrame(Hud hud)
            {
                yield return null;
                Scan(hud);
            }
        }
    }
}
