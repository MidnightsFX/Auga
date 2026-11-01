using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Auga
{
    /// <summary>
    /// Auga's look for the UI of mods that depend on it, given at runtime. A mod that declares a BepInDependency on
    /// Auga gets the UI prefabs in its asset bundles (AutoRestyle_Bundles), the widgets it builds with Jotunn's
    /// GUIManager (AutoRestyle_Jotunn) and what it adds to Auga's screens (AutoRestyle_Screens) restyled in place
    /// (<see cref="AugaPanelRestyler.RestyleInPlace"/>). Every other mod is left alone and uses the API to style itself.
    /// A dependent that styles itself through the API opts out with API.AutoRestyle_Exclude; [Compatibility]
    /// AutoRestyleDependents turns all of it off.
    /// </summary>
    public static class AutoRestyle
    {
        // Valheim Admin Tool styles itself with AugaPanelRestyler, by reflection
        private static readonly HashSet<string> Excluded = new HashSet<string> { "com.icecub.valheimadmintool" };
        private static readonly Dictionary<Assembly, PluginInfo> Dependents = new Dictionary<Assembly, PluginInfo>();
        private static readonly HashSet<string> Announced = new HashSet<string>();
        private static readonly HashSet<int> RestyledAssets = new HashSet<int>();

        // frames from these are passed over when looking for the mod behind a call: Auga itself, the libraries a mod
        // builds UI through, and the engine, runtime and loader
        private static readonly string[] PassThrough = { "Auga", "Jotunn", "0Harmony", "BepInEx", "mscorlib", "netstandard" };
        private static readonly string[] PassThroughPrefixes = { "UnityEngine", "Unity.", "System", "MonoMod", "Mono." };

        public static bool Enabled => ValConfig.AutoRestyleDependents == null || ValConfig.AutoRestyleDependents.Value;

        internal static void Exclude(string pluginGUID)
        {
            if (!string.IsNullOrEmpty(pluginGUID))
                Excluded.Add(pluginGUID);
        }

        /// <summary>
        /// The dependent whose plugin file is <paramref name="assembly"/>, or null. Its info is in
        /// Chainloader.PluginInfos from just before its Awake, and its code cannot run any earlier.
        /// </summary>
        internal static PluginInfo DependentOf(Assembly assembly)
        {
            if (assembly == null || assembly.IsDynamic)
                return null;
            if (Dependents.TryGetValue(assembly, out var known))
                return known;
            string location;
            try { location = assembly.Location; }
            catch (NotSupportedException) { location = null; }
            PluginInfo owner = null;
            if (!string.IsNullOrEmpty(location))
            {
                owner = Chainloader.PluginInfos.Values.FirstOrDefault(p =>
                    string.Equals(p.Location, location, StringComparison.OrdinalIgnoreCase) &&
                    p.Dependencies.Any(d => d.DependencyGUID == Auga.PluginID));
            }
            Dependents[assembly] = owner;
            return owner;
        }

        /// <summary>
        /// The mod behind the current call: the first frame that is not Auga, Jotunn, Unity, the runtime or the
        /// loader decides, and it counts only when it belongs to a dependent that may be restyled. A Harmony patch
        /// replaces its method's frame with a dynamic one, which GetMethodFromStackframe maps back.
        /// </summary>
        internal static PluginInfo Caller()
        {
            if (!Enabled)
                return null;
            var frames = new StackTrace(2, false).GetFrames();
            if (frames == null)
                return null;
            foreach (var frame in frames)
            {
                MethodBase method;
                try { method = Harmony.GetMethodFromStackframe(frame); }
                catch (Exception) { method = frame.GetMethod(); }
                var assembly = method?.DeclaringType?.Assembly;
                if (assembly == null || assembly.IsDynamic || IsPassThrough(assembly))
                    continue;
                var owner = DependentOf(assembly);
                return ShouldRestyle(owner) ? owner : null;
            }
            return null;
        }

        private static bool IsPassThrough(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return PassThrough.Contains(name) || PassThroughPrefixes.Any(name.StartsWith);
        }

        internal static bool ShouldRestyle(PluginInfo owner)
        {
            return owner != null && Enabled && !Excluded.Contains(owner.Metadata.GUID);
        }

        /// <summary>A scene object found by a later pass: restyled once, and marked so the next pass skips it.</summary>
        internal static void RestyleObject(GameObject go, PluginInfo owner)
        {
            if (go == null || go.GetComponent<AutoRestyled>() != null)
                return;
            RestyleNow(go, owner);
            go.AddComponent<AutoRestyled>();
        }

        /// <summary>
        /// Restyled whether or not it was before: a Jotunn Apply*Style call on a widget puts Jotunn's look back, so
        /// the widget is restyled after every one (RestyleInPlace changes nothing that already has Auga's look).
        /// </summary>
        internal static void RestyleNow(GameObject go, PluginInfo owner)
        {
            if (go == null)
                return;
            Announce(owner);
            AugaPanelRestyler.RestyleInPlace(go.transform);
        }

        /// <summary>
        /// A UI prefab from a dependent's bundle: restyled once on the asset itself, so every copy the mod makes of it
        /// already has Auga's look. An asset gets no marker component; its instance ID is remembered instead.
        /// </summary>
        internal static void RestyleAsset(GameObject prefab, PluginInfo owner)
        {
            if (prefab == null || !(prefab.transform is RectTransform) || !RestyledAssets.Add(prefab.GetInstanceID()))
                return;
            Announce(owner);
            AugaPanelRestyler.RestyleInPlace(prefab.transform);
        }

        // always logged, whatever Auga's logging settings: the mod's author needs to know why its UI changed
        private static void Announce(PluginInfo owner)
        {
            if (Announced.Add(owner.Metadata.GUID))
                Debug.Log($"[Auga] {owner.Metadata.Name} depends on Auga, so Auga gives its UI Auga's look. A mod that styles itself calls Auga.API.AutoRestyle_Exclude(\"{owner.Metadata.GUID}\") first thing in Awake.");
        }
    }

    /// <summary>Marks an object <see cref="AutoRestyle"/> has restyled for a dependent mod.</summary>
    public class AutoRestyled : MonoBehaviour
    {
    }
}
