using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Vuplex.WebView;
using Vuplex.WebView.Internal;

namespace AtelierResleriana.Plugin.Inspection;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public static new ManualLogSource Log { get; set; }
    public static Harmony Harmony { get; set; }

    public override void Load()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo(Paths.PluginPath);

        Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        //Harmony.Patch(AccessTools.Method(typeof(BaseWebViewPrefab), nameof(BaseWebViewPrefab.WaitUntilInitialized)),
        //    prefix: new HarmonyMethod(typeof(Plugin), nameof(BaseWebViewPrefabWaitUntilInitializedPrefix)));

        foreach (var methodInfo in typeof(BaseWebView).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (methodInfo.Name != "LoadUrl")
            {
                continue;
            }

            Harmony.Patch(methodInfo,
                prefix: new HarmonyMethod(typeof(Plugin), nameof(BaseWebViewLoadUrlPrefix)));
        }

        //Harmony.Patch(AccessTools.Method(typeof(BaseWebView), nameof(BaseWebView.LoadUrl), new Type[] { typeof(string) }),
        //    prefix: new HarmonyMethod(typeof(Plugin), nameof(Inspect)));

        //Harmony.Patch(AccessTools.Method(typeof(CanvasWebViewPrefab), nameof(CanvasWebViewPrefab.Instantiate), new Type[] { }),
        //    prefix: new HarmonyMethod(typeof(Plugin), nameof(Inspect)));

        /*
        foreach (MethodInfo methodInfo in typeof(ConsumptionItemInfoDisp).GetMethods())
        {
            if (methodInfo.Name == "KDNMGKADBEF")
            {
                Log.LogInfo("FOUND");
                Harmony.Patch(methodInfo,
                    prefix: new HarmonyMethod(typeof(Plugin), nameof(Inspect)));
            }
        }
        */

        // No longer necessary with master data localization.
        //Harmony.Patch(AccessTools.Method(typeof(TMPro.TMP_Text), nameof(TMPro.TMP_Text.SetText), new Type[] { typeof(string), typeof(bool) }),
        //    prefix: new HarmonyMethod(typeof(Plugin), nameof(TMPTextSetTextPrefix)));

        /*
        Reflection.Type[] types = Types.From(
            Il2CppType.Of<AccountLinkageDialog>().Assembly, 
            Il2CppType.Of<ActFixDataManager>().Assembly, 
            Il2CppType.Of<PMDBPANLOAP>().Assembly);
        File.WriteAllText(Path.Combine(Paths.PluginPath, "Types.json"), JsonSerializer.Serialize(types, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
        */
    }

    private static void BaseWebViewLoadUrlPrefix(ref string url)
    {
        if (url == "https://info.resleriana.jp/news/")
        {
            //url = "https://atelierresleriana.azurewebsites.net/News";
        }
    }

    private static void BaseWebViewUrlSetterPrefix(BaseWebView __instance, string value)
    {
        Log.LogInfo(value);
    }

    private static void BaseWebViewPrefabWaitUntilInitializedPrefix(BaseWebViewPrefab __instance)
    {
        Log.LogInfo("Called");
        Log.LogInfo(__instance.GetIl2CppType().Name);
        if (__instance != null)
        {
            Log.LogInfo($"Uri: {__instance.InitialUrl}");
        }
    }

    private static void Inspect(MethodBase __originalMethod)
    {
        Log.LogError($"{__originalMethod.Name}({string.Join(", ", __originalMethod.GetParameters().Select(x => $"{x.ParameterType.Name} {x.Name}"))}) called.");
    }

    private static string GetFullPath(Transform transform)
    {
        List<string> pathParts = new List<string>(32);

        Transform current = transform;
        while (current != null)
        {
            pathParts.Add(current.name);
            current = current.parent;
        }

        pathParts.Reverse();
        return string.Join("/", pathParts);
    }

    /*
    private static void TMPTextSetTextPrefix(TMPro.TMP_Text __instance, ref string sourceText, MethodBase __originalMethod)
    {
        if (__instance != null)
        {
            if (Il2CppType.Of<TMPro.TMP_Text>().IsAssignableFrom(__instance.GetIl2CppType()))
            {
                if (sourceText.ShouldLocalize())
                {
                    if (LocalizationService.TryLocalize(Locale, sourceText, out string localizedText))
                    {
                        sourceText = localizedText;
                    }
                }
            }
        }
    }
    */
}
