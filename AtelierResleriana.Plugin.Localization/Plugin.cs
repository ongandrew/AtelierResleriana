using AtelierResleriana.Text;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace AtelierResleriana.Plugin.Localization
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static new ManualLogSource Log { get; set; }
        private Harmony Harmony { get; set; }
        private static ManifestResources ManifestResourceLoader { get; set; } = new ManifestResources(typeof(Resources.Root));
        private static TextAssetFactory TextAssetFactory { get; set; } = new TextAssetFactory();

        private const string Language = "en";
        private static Dictionary<string, Dictionary<string, string>> LocalizationMap { get; set; }

        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            LocalizationMap = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(ManifestResourceLoader.GetString("LocalizationMap.json"), new JsonSerializerOptions()
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            var provideHandleCompleteMethodInfo = typeof(ProvideHandle).GetMethod("Complete").MakeGenericMethod(typeof(UnityEngine.Object));
            Harmony.Patch(provideHandleCompleteMethodInfo,
                prefix: new HarmonyMethod(typeof(Plugin), nameof(ProvideHandleCompletePrefix)));
        }

        private static void ProvideHandleCompletePrefix(ProvideHandle __instance, ref UnityEngine.Object result, bool status, Il2CppSystem.Exception exception)
        {
            if (result == null)
            {
                return;
            }

            if (result.GetIl2CppType().Equals(Il2CppType.Of<TextAsset>()))
            {
                var textAsset = result.TryCast<TextAsset>();
                if (textAsset != null)
                {
                    string primaryKey = __instance.Location.PrimaryKey;

                    if (LocalizationMap.ContainsKey(primaryKey) && LocalizationMap[primaryKey].ContainsKey(Language))
                    {
                        PackedText packedText = new PackedText(textAsset.bytes);
                        string localizedPrimaryKey = LocalizationMap[primaryKey][Language];

                        if (!packedText.Properties.Any(x => x.Id == PropertyIds.Id && x.Type == PropertyTypes.UnsignedInteger) ||
                            !packedText.Properties.Any(x => x.Id == PropertyIds.Text && x.Type == PropertyTypes.String))
                        {
                            Log.LogWarning($"Not patching {primaryKey} to {localizedPrimaryKey} due to invalid source schema.");
                            return;
                        }

                        PackedText localizedPackedText = new PackedText(ManifestResourceLoader.GetStream($"{localizedPrimaryKey}"));
                        if (!localizedPackedText.Properties.Any(x => x.Id == PropertyIds.Id && x.Type == PropertyTypes.UnsignedInteger) ||
                            !localizedPackedText.Properties.Any(x => x.Id == PropertyIds.Text && x.Type == PropertyTypes.String))
                        {
                            Log.LogWarning($"Not patching {primaryKey} to {localizedPrimaryKey} due to invalid destination schema.");
                            return;
                        }

                        Dictionary<uint, string> localizedTextMap = new Dictionary<uint, string>();
                        for (int entryIndex = 0; entryIndex < localizedPackedText.Entries.Count; entryIndex++)
                        {
                            uint id = localizedPackedText.GetValue<uint>(entryIndex, PropertyIds.Id);
                            string text = localizedPackedText.GetValue<string>(entryIndex, PropertyIds.Text);
                            localizedTextMap.Add(id, text);
                        }

                        for (int entryIndex = 0; entryIndex < packedText.Entries.Count; entryIndex++)
                        {
                            uint id = packedText.GetValue<uint>(entryIndex, PropertyIds.Id);

                            if (localizedTextMap.ContainsKey(id))
                            {
                                packedText.SetValue(entryIndex, PropertyIds.Text, localizedTextMap[id]);
                            }
                        }

                        TextAsset newTextAsset = TextAssetFactory.CreateFromBytes(packedText.ToBytes());
                        newTextAsset.name = textAsset.name;
                        result = newTextAsset;

                        Log.LogInfo($"Generated override for {primaryKey} ({textAsset.name}) using {localizedPrimaryKey}.");
                    }
                }
            }
        }
    }
}