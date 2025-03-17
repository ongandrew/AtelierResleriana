using BepInEx.Logging;
using BestHTTP;
using BestHTTP.Caching;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AtelierResleriana.Plugin.Inspection
{
    public static class Inspector
    {
        public static ManualLogSource Log { get => Plugin.Log; }

        public static void Patch(Harmony Harmony)
        {
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.Copy)),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileCopyPrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.Create), new Type[] { typeof(string) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileCreatePrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.Create), new Type[] { typeof(string), typeof(int) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileCreatePrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.Exists)),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileExistsPrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.InternalWriteAllBytes)),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.Move)),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileMovePrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.Open), new Type[] { typeof(string), typeof(Il2CppSystem.IO.FileMode) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileOpenReadPrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.Open), new Type[] { typeof(string), typeof(Il2CppSystem.IO.FileMode), typeof(Il2CppSystem.IO.FileAccess), typeof(Il2CppSystem.IO.FileShare) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileOpenReadPrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.OpenRead)),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileOpenReadPrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.OpenText)),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileOpenReadPrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.ReadAllBytes)),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileRealAllBytesPrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.ReadAllBytesUnknownLength)),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.ReadAllText)),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileRealAllTextPrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.Replace), new Type[] { typeof(string), typeof(string), typeof(string) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileReplacePrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.Replace), new Type[] { typeof(string), typeof(string), typeof(string), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileReplacePrefix)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.WriteAllBytes)),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.WriteAllText)),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(Il2CppSystemIOFileWriteAllTextPrefix)));

            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromFile), new Type[] { typeof(string) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromFile), new Type[] { typeof(string), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromFile), new Type[] { typeof(string), typeof(uint), typeof(ulong) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromFile_Internal), new Type[] { typeof(string), typeof(uint), typeof(ulong) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromFileAsync), new Type[] { typeof(string) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromFileAsync), new Type[] { typeof(string), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromFileAsync), new Type[] { typeof(string), typeof(uint), typeof(ulong) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromFileAsync_Internal), new Type[] { typeof(string), typeof(uint), typeof(ulong) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromMemory), new Type[] { typeof(Il2CppStructArray<byte>) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromMemory), new Type[] { typeof(Il2CppStructArray<byte>), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromMemory_Internal), new Type[] { typeof(Il2CppStructArray<byte>), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromMemoryAsync), new Type[] { typeof(Il2CppStructArray<byte>) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromMemoryAsync), new Type[] { typeof(Il2CppStructArray<byte>), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromMemoryAsync_Internal), new Type[] { typeof(Il2CppStructArray<byte>), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            // Called
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromStream), new Type[] { typeof(Il2CppSystem.IO.Stream) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromStream), new Type[] { typeof(Il2CppSystem.IO.Stream), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromStream), new Type[] { typeof(Il2CppSystem.IO.Stream), typeof(uint), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromStreamInternal), new Type[] { typeof(Il2CppSystem.IO.Stream), typeof(uint), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromStreamAsync), new Type[] { typeof(Il2CppSystem.IO.Stream) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromStreamAsync), new Type[] { typeof(Il2CppSystem.IO.Stream), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromStreamAsync), new Type[] { typeof(Il2CppSystem.IO.Stream), typeof(uint), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromStreamAsyncInternal), new Type[] { typeof(Il2CppSystem.IO.Stream), typeof(uint), typeof(uint) }),
                prefix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));

            foreach (var methodInfo in typeof(AssetBundle).GetMethods())
            {
                if (!new string[] {
                    "Load",
                    "LoadAsync",
                    "LoadAll",
                    "LoadAllAssets",
                    "LoadAllAssetsAsync",
                    "LoadAsset",
                    "LoadAsset_Internal",
                    "LoadAssetAsync",
                    "LoadAssetAsync_Internal",
                    "LoadAssetWithSubAssets",
                    "LoadAssetWithSubAssets_Internal",
                    "LoadAssetWithSubAssetsAsync",
                    "LoadAssetWithSubAssetsAsync_Internal"
                }.Contains(methodInfo.Name))
                {
                    continue;
                }

                var patchMethodInfo = methodInfo;
                if (methodInfo.IsGenericMethod)
                {
                    patchMethodInfo = methodInfo.MakeGenericMethod(typeof(UnityEngine.Object));
                }

                Harmony.Patch(patchMethodInfo,
                    postfix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));
            }

            // Called
            Harmony.Patch(
                AccessTools.Method(typeof(SceneManager), nameof(SceneManager.Internal_SceneLoaded)),
                postfix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));

            // Called   
            Harmony.Patch(AccessTools.Method(typeof(HTTPCacheService), nameof(HTTPCacheService.LoadLibrary)),
                postfix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));

            // Called
            Harmony.Patch(AccessTools.Method(typeof(HTTPRequest), nameof(HTTPRequest.Send)),
                postfix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));

            // Called
            Harmony.Patch(typeof(Aktsk.ABCache.ABCacheProvider.AssetBundleResource).GetMethod(nameof(Aktsk.ABCache.ABCacheProvider.AssetBundleResource.GetAssetBundle)),
                postfix: new HarmonyMethod(typeof(Inspector), nameof(InspectCall)));

            // Called - only for base initialization.
            Harmony.Patch(AccessTools.PropertySetter(typeof(TMPro.TMP_Text), nameof(TMPro.TMP_Text.text)),
                prefix: new HarmonyMethod(typeof(Plugin), nameof(InspectCall)));
        }

        internal static void InspectCall(MethodBase __originalMethod)
        {
            Log.LogError($"{__originalMethod.Name} ({string.Join(", ", __originalMethod.GetParameters().Select(x => $"{x.ParameterType.Name} {x.Name}"))})");
        }

        private static void Il2CppSystemIOFileExistsPrefix(string path, MethodBase __originalMethod)
        {
            Log.LogInfo($"{__originalMethod.Name}: {path}");
        }

        private static void Il2CppSystemIOFileRealAllBytesPrefix(string path, ref Il2CppStructArray<byte> __result, MethodBase __originalMethod)
        {
            // Reads CachedMasterData.bin
            Log.LogInfo($"{__originalMethod.Name}: {path}");
        }

        private static void Il2CppSystemIOFileRealAllTextPrefix(string path, MethodBase __originalMethod)
        {
            Log.LogInfo($"{__originalMethod.Name}: {path}");
        }

        private static void Il2CppSystemIOFileWriteAllTextPrefix(string path, MethodBase __originalMethod)
        {
            Log.LogInfo($"{__originalMethod.Name}: {path}");
        }

        private static void Il2CppSystemIOFileCreatePrefix(string path, MethodBase __originalMethod)
        {
            Log.LogInfo($"{__originalMethod.Name}: {path}");
        }

        private static void Il2CppSystemIOFileCopyPrefix(string sourceFileName, string destFileName, bool overwrite, MethodBase __originalMethod)
        {
            Log.LogInfo($"{__originalMethod.Name}: {sourceFileName} -> {destFileName} ({overwrite})");
        }

        private static void Il2CppSystemIOFileReplacePrefix(string sourceFileName, string destinationFileName, MethodBase __originalMethod)
        {
            Log.LogInfo($"{__originalMethod.Name}: {sourceFileName} -> {destinationFileName}");
        }

        private static void Il2CppSystemIOFileMovePrefix(string sourceFileName, string destFileName, MethodBase __originalMethod)
        {
            Log.LogInfo($"{__originalMethod.Name}: {sourceFileName} -> {destFileName}");
        }

        private static void Il2CppSystemIOFileOpenReadPrefix(string path, MethodBase __originalMethod)
        {
            Log.LogInfo($"{__originalMethod.Name}: {path}");
        }
    }
}
