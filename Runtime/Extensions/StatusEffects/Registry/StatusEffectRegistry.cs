using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Text;

using Mandible.Registry;

namespace Mandible.Entities.StatusEffects
{
    [InitializeOnLoad]
    public static class StatusEffectRegistry
    {
        public const string StatusEffectRegistryFolder = "Entities/StatusEffects/Registry";
        private const string StatusEffectRegistryAssetName = "StatusEffectRegistryData.asset";

        static StatusEffectRegistryData registryAsset;
        static List<StatusEffectData> cached;
        static Dictionary<Type, StatusEffectData> typeLookup;

        public static IReadOnlyList<StatusEffectData> All
        {
            get
            {
                EnsureCache();
                return cached;
            }
        }

        static StatusEffectRegistry()
        {
            #if UNITY_EDITOR
            MandibleData.onDataCreated += () => EditorReload();
            MandibleData.onDataUpdated += () => EditorReload();
            MandibleData.onDataRepaired += () => EditorReload();

            EditorReload();
            
            #endif
        }

        //API
        public static StatusEffectData GetDataByEffectType<T>() where T : StatusEffect
        {
            EnsureCache();
            typeLookup.TryGetValue(typeof(T), out var data);
            return data;
        }

        public static T GetStatusEffect<T>(Entity owner = default) where T : StatusEffect
        {
            EnsureCache();
            if (!typeLookup.TryGetValue(typeof(T), out var data))
            {
                return null;
            }

            return (T)data.CreateRuntimeEffect(owner);
        }

        public static StatusEffect GetStatusEffectByName(string effectName)
        {
            EnsureCache();
            var data = cached.FirstOrDefault(d => d.effectName == effectName);
            if (data == null) return null;

            return data.CreateRuntimeEffect(null);
        }

        public static bool HasRegisteredEffectType(Type effectType)
        {
            EnsureCache();
            return typeLookup.ContainsKey(effectType);
        }

        //Registry

        public static void AddStatusEffectData(StatusEffectData data)
        {
            EnsureCache();

            if(data == null) return;
            if (cached.Contains(data)) return;

            var type = GetEffectType(data);
            
            cached.Add(data);
            typeLookup[type] = data;
            MarkDirty();
        }

        public static void UpdateStatusEffectData(StatusEffectData before, StatusEffectData data)
        {
            EnsureCache();

            if(before != null)
            {
                //Remove old
                var beforeType = GetEffectType(before);
                if (typeLookup.ContainsKey(beforeType))
                    typeLookup.Remove(beforeType);
                
                registryAsset.statusEffects.Remove(before);
            }

            if(data == null){
                MarkDirty();
                return;
            }

            var type = GetEffectType(data);

            //Add new
            typeLookup[type] = data;
            registryAsset.statusEffects.Add(data);
            MarkDirty();
        }

        public static void RemoveStatusEffectData(StatusEffectData data)
        {
            EnsureCache();
            if (data == null) return;

            cached.Remove(data);
            var type = GetEffectType(data);
            if (typeLookup.ContainsKey(type))
            {
                typeLookup.Remove(type);
            }
            registryAsset.statusEffects.Remove(data);
            MarkDirty();
        }

        public static void ForceRegistryChange(List<StatusEffectData> newEntries)
        {
            registryAsset.statusEffects = newEntries;
            EditorUtility.SetDirty(registryAsset);
        }

        //Helpers

        public static Type GetEffectType(StatusEffectData data)
        {
            if(data == null) return null;

            var effect = data.CreateRuntimeEffect(null);
            if (effect == null) return null;

            return effect.GetType();
        }

        public static List<StatusEffectData> GetAllStatusEffectData()
        {
            EnsureCache();
            

            var data = cached != null ? new List<StatusEffectData>(cached) : new List<StatusEffectData>();
            return data;
        }

        //Cache

        static StatusEffectRegistryData EnsureData()
        {
            registryAsset = MandibleData.EnsureDataAsset<StatusEffectRegistryData>(StatusEffectRegistryFolder + "/" + StatusEffectRegistryAssetName);
            //registryAsset = Resources.Load<StatusEffectRegistryData>("StatusEffectRegistryData"); OLD
            return registryAsset;
        }

        static void EnsureCache()
        {
            if (cached != null && typeLookup != null) return;

            if (registryAsset == null) EnsureData();
            if (registryAsset == null) return;
            
            cached = registryAsset.statusEffects ?? new List<StatusEffectData>();

            typeLookup = new Dictionary<Type, StatusEffectData>();
            foreach (var data in cached)
            {
                var type = GetEffectType(data);
                if (type == null) continue;
                typeLookup[type] = data;
            }
        }

        public static void EditorReload()
        {
            typeLookup?.Clear();
            cached = null;

            EnsureData();
            EnsureCache();

            //Create accessory data
            StatusEffectEnum.Generate();
        }

        static void MarkDirty()
        {
            #if UNITY_EDITOR
            if(registryAsset == null) return;
            UnityEditor.EditorUtility.SetDirty(registryAsset);
            #endif
        }
    }

   internal static class StatusEffectEnum
    {
        private const string Namespace = "Mandible.Entities.StatusEffects";

        private static string GetOutputPath()
        {
            string rootFolder = MandibleData.GetRootFolder();
            if (string.IsNullOrEmpty(rootFolder))
            {
                Debug.LogError("StatusEffectEnum: Mandible root folder not found.");
                return null;
            }

            // Build the full path
            string fullPath = Path.Combine(rootFolder, "Data", StatusEffectRegistry.StatusEffectRegistryFolder, "StatusEffectId.cs");
            return fullPath.Replace("\\", "/");
        }

        public static void Generate()
        {
            var effects = StatusEffectRegistry.All;
            if (effects == null) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("// Auto-generated. Do not edit.");
            sb.AppendLine($"namespace {Namespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public enum StatusEffectId");
            sb.AppendLine("    {");
            sb.AppendLine("        None = 0,");

            foreach (var e in effects)
            {
                if (string.IsNullOrWhiteSpace(e.effectName)) continue;
                sb.AppendLine($"        {MakeSafe(e.effectName)},");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            // Determine output path
            var outputPath = GetOutputPath();
            if (string.IsNullOrEmpty(outputPath)) return;

            // Ensure folder exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var newText = sb.ToString();
            if (File.Exists(outputPath))
            {
                var existing = File.ReadAllText(outputPath);
                if (existing == newText) return;
            }

            File.WriteAllText(outputPath, newText);
            UnityEditor.AssetDatabase.Refresh();
        }

        private static string MakeSafe(string input)
        {
            var valid = new string(input
                .Where(c => char.IsLetterOrDigit(c) || c == '_')
                .ToArray());

            if (string.IsNullOrEmpty(valid))
                valid = "Effect";

            if (char.IsDigit(valid[0]))
                valid = "_" + valid;

            return valid;
        }
    }
}
