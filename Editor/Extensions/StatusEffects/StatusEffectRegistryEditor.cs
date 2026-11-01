using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

using Mandible.Registry;

namespace Mandible.Entities.StatusEffects
{
    [InitializeOnLoad]
    public static class StatusEffectRegistryEditor
    {
        public const string StatusEffectRegistryFolder =
            "Entities/StatusEffects/Registry";

        private const string StatusEffectRegistryAssetName =
            "StatusEffectRegistryData.asset";

        private static StatusEffectRegistryData registryAsset;

        static StatusEffectRegistryEditor()
        {
            MandibleData.onDataCreated += EditorReload;
            MandibleData.onDataUpdated += EditorReload;
            MandibleData.onDataRepaired += EditorReload;

            EditorReload();
        }

        private static void EnsureData()
        {
            registryAsset =
                MandibleData.EnsureDataAsset<StatusEffectRegistryData>(
                    "Resources/Mandible/" +
                    StatusEffectRegistryFolder + "/" +
                    StatusEffectRegistryAssetName
                );
        }

        public static void EditorReload()
        {
            EnsureData();

            StatusEffectRegistry.Reload();

            StatusEffectEnum.Generate();
        }

        public static void AddStatusEffectData(StatusEffectData data)
        {
            EnsureData();

            if (data == null || registryAsset == null) return;

            if (registryAsset.statusEffects.Contains(data)) return;

            registryAsset.statusEffects.Add(data);

            Save();
        }

        public static void RemoveStatusEffectData(StatusEffectData data)
        {
            EnsureData();

            if (data == null || registryAsset == null) return;

            registryAsset.statusEffects.Remove(data);

            Save();
        }

        public static void UpdateStatusEffectData(StatusEffectData before, StatusEffectData data)
        {
            EnsureData();

            if (registryAsset == null) return;

            if (before != null) 
                registryAsset.statusEffects.Remove(before);

            if (data != null && !registryAsset.statusEffects.Contains(data)) 
                registryAsset.statusEffects.Add(data);

            Save();
        }

        public static void ForceRegistryChange(
            List<StatusEffectData> newEntries)
        {
            EnsureData();

            if (registryAsset == null) return;

            registryAsset.statusEffects = newEntries;

            Save();
        }

        private static void Save()
        {
            EditorUtility.SetDirty(registryAsset);
            AssetDatabase.SaveAssets();

            StatusEffectRegistry.Reload();
        }
    }
}