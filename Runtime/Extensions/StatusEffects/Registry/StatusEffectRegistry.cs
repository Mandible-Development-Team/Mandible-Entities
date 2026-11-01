using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Mandible.Entities.StatusEffects
{
    public static class StatusEffectRegistry
    {
        private const string RegistryPath =
            "Mandible/Entities/StatusEffects/Registry/StatusEffectRegistryData";

        private static StatusEffectRegistryData registryAsset;
        private static List<StatusEffectData> cached;
        private static Dictionary<Type, StatusEffectData> typeLookup;

        public static IReadOnlyList<StatusEffectData> All
        {
            get
            {
                EnsureCache();
                return cached;
            }
        }

        private static void EnsureData()
        {
            if (registryAsset != null) return;

            registryAsset =
                Resources.Load<StatusEffectRegistryData>(RegistryPath);
        }

        private static void EnsureCache()
        {
            if (cached != null && typeLookup != null) return;

            EnsureData();

            cached = registryAsset != null
                ? registryAsset.statusEffects ?? new List<StatusEffectData>()
                : new List<StatusEffectData>();

            typeLookup = new Dictionary<Type, StatusEffectData>();

            foreach (var data in cached)
            {
                var type = GetEffectType(data);
                if (type == null) continue;

                typeLookup[type] = data;
            }
        }

        public static void Reload()
        {
            registryAsset = null;
            cached = null;
            typeLookup = null;

            EnsureCache();
        }

        public static StatusEffectData GetDataByEffectType<T>()
            where T : StatusEffect
        {
            EnsureCache();

            typeLookup.TryGetValue(typeof(T), out var data);
            return data;
        }

        public static T GetStatusEffect<T>(Entity owner = default)
            where T : StatusEffect
        {
            var data = GetDataByEffectType<T>();
            return data != null
                ? (T)data.CreateRuntimeEffect(owner)
                : null;
        }

        public static StatusEffect GetStatusEffectByName(string effectName)
        {
            EnsureCache();

            var data = cached.FirstOrDefault(
                d => d.effectName == effectName
            );

            return data?.CreateRuntimeEffect(null);
        }

        public static bool HasRegisteredEffectType(Type effectType)
        {
            EnsureCache();
            return typeLookup.ContainsKey(effectType);
        }

        public static Type GetEffectType(StatusEffectData data)
        {
            if (data == null) return null;

            var effect = data.CreateRuntimeEffect(null);
            return effect?.GetType();
        }

        public static List<StatusEffectData> GetAllStatusEffectData()
        {
            EnsureCache();
            return new List<StatusEffectData>(cached);
        }
    }
}