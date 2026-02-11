using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Mandible.Entities
{
    public static class EntityExtensionRegistry
    {
        public static readonly List<Type> extensionTypes = new();

        public static void GetExtensions()
        {
            extensionTypes.Clear();

            var allTypes = Assembly.GetExecutingAssembly().GetTypes();

            var leafExtensions = allTypes
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => typeof(EntityExtension).IsAssignableFrom(t))
                .Where(t => !allTypes.Any(sub => sub.BaseType == t))
                .ToList();

            foreach (var extType in leafExtensions)
            {
                extensionTypes.Add(extType);
            }
        }

        public static List<EntityExtension> CreateExtensions()
        {
            List<EntityExtension> extensions = new List<EntityExtension>();
            foreach(var type in extensionTypes)
            {
                if (Activator.CreateInstance(type) is EntityExtension instance)
                {
                    extensions.Add(instance);
                }
            }
            return extensions;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void EditorReload()
        {
            GetExtensions();
        }
    }
}
