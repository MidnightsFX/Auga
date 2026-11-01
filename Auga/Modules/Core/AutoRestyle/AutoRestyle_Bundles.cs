using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Auga
{
    /// <summary>
    /// UI prefabs in a dependent mod's asset bundles (see <see cref="AutoRestyle"/>). Loading a bundle records the
    /// mod behind the call; every GameObject later loaded from that bundle whose root is a RectTransform is restyled
    /// on the asset itself. The public non-generic loaders are patched: the generic overloads call them.
    /// </summary>
    public static class AutoRestyle_Bundles
    {
        private static readonly Dictionary<int, PluginInfo> BundleOwners = new Dictionary<int, PluginInfo>();
        private static readonly Dictionary<AsyncOperation, PluginInfo> PendingOwners = new Dictionary<AsyncOperation, PluginInfo>();

        private static void Record(AssetBundle bundle, PluginInfo owner)
        {
            if (bundle != null && owner != null)
                BundleOwners[bundle.GetInstanceID()] = owner;
        }

        private static PluginInfo OwnerOf(AssetBundle bundle)
        {
            return bundle != null && BundleOwners.TryGetValue(bundle.GetInstanceID(), out var owner) && AutoRestyle.ShouldRestyle(owner) ? owner : null;
        }

        private static void Restyle(PluginInfo owner, Object asset)
        {
            if (owner != null && asset is GameObject go)
                AutoRestyle.RestyleAsset(go, owner);
        }

        private static void Restyle(PluginInfo owner, Object[] assets)
        {
            if (owner == null || assets == null)
                return;
            foreach (var asset in assets)
                Restyle(owner, asset);
        }

        [HarmonyPatch]
        public static class LoadBundle
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(AssetBundle), "LoadFromFile_Internal", new[] { typeof(string), typeof(uint), typeof(ulong) });
                yield return AccessTools.Method(typeof(AssetBundle), "LoadFromMemory_Internal", new[] { typeof(byte[]), typeof(uint) });
                yield return AccessTools.Method(typeof(AssetBundle), "LoadFromStreamInternal", new[] { typeof(Stream), typeof(uint), typeof(uint) });
            }

            public static void Postfix(AssetBundle __result)
            {
                if (__result != null)
                    Record(__result, AutoRestyle.Caller());
            }
        }

        [HarmonyPatch]
        public static class LoadBundleAsync
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(AssetBundle), "LoadFromFileAsync_Internal", new[] { typeof(string), typeof(uint), typeof(ulong) });
                yield return AccessTools.Method(typeof(AssetBundle), "LoadFromMemoryAsync_Internal", new[] { typeof(byte[]), typeof(uint) });
                yield return AccessTools.Method(typeof(AssetBundle), "LoadFromStreamAsyncInternal", new[] { typeof(Stream), typeof(uint), typeof(uint) });
            }

            public static void Postfix(AssetBundleCreateRequest __result)
            {
                var owner = AutoRestyle.Caller();
                if (__result != null && owner != null)
                    PendingOwners[__result] = owner;
            }
        }

        [HarmonyPatch(typeof(AssetBundleCreateRequest), nameof(AssetBundleCreateRequest.assetBundle), MethodType.Getter)]
        public static class BundleCreated
        {
            public static void Postfix(AssetBundleCreateRequest __instance, AssetBundle __result)
            {
                if (__result != null && PendingOwners.TryGetValue(__instance, out var owner))
                {
                    PendingOwners.Remove(__instance);
                    Record(__result, owner);
                }
            }
        }

        [HarmonyPatch(typeof(AssetBundle), nameof(AssetBundle.LoadAsset), typeof(string), typeof(Type))]
        public static class LoadAsset
        {
            public static void Postfix(AssetBundle __instance, Object __result)
            {
                Restyle(OwnerOf(__instance), __result);
            }
        }

        [HarmonyPatch]
        public static class LoadAssets
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadAssetWithSubAssets), new[] { typeof(string), typeof(Type) });
                yield return AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadAllAssets), new[] { typeof(Type) });
            }

            public static void Postfix(AssetBundle __instance, Object[] __result)
            {
                Restyle(OwnerOf(__instance), __result);
            }
        }

        [HarmonyPatch]
        public static class LoadAssetsAsync
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadAssetAsync), new[] { typeof(string), typeof(Type) });
                yield return AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadAssetWithSubAssetsAsync), new[] { typeof(string), typeof(Type) });
                yield return AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadAllAssetsAsync), new[] { typeof(Type) });
            }

            public static void Postfix(AssetBundle __instance, AssetBundleRequest __result)
            {
                var owner = OwnerOf(__instance);
                if (__result != null && owner != null)
                    PendingOwners[__result] = owner;
            }
        }

        [HarmonyPatch(typeof(AssetBundleRequest), nameof(AssetBundleRequest.asset), MethodType.Getter)]
        public static class AsyncAsset
        {
            public static void Postfix(AssetBundleRequest __instance, Object __result)
            {
                if (PendingOwners.TryGetValue(__instance, out var owner))
                    Restyle(owner, __result);
            }
        }

        [HarmonyPatch(typeof(AssetBundleRequest), nameof(AssetBundleRequest.allAssets), MethodType.Getter)]
        public static class AsyncAssets
        {
            public static void Postfix(AssetBundleRequest __instance, Object[] __result)
            {
                if (PendingOwners.TryGetValue(__instance, out var owner))
                    Restyle(owner, __result);
            }
        }
    }
}
