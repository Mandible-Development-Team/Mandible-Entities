#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Mandible.Entities.StatusEffects
{
    [CustomEditor(typeof(StatusEffectContribution))]
    public class StatusEffectContributionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Get all effect names from the registry
            var registryNames = StatusEffectRegistry.All.Select(d => d.effectName).ToArray();

            var listProp = serializedObject.FindProperty("contributions");

            for (int i = 0; i < listProp.arraySize; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                var nameProp = element.FindPropertyRelative("name");
                var valueProp = element.FindPropertyRelative("value");

                // Dropdown for the effect name
                int currentIndex = System.Array.IndexOf(registryNames, nameProp.stringValue);
                if (currentIndex < 0) currentIndex = 0;

                int newIndex = EditorGUILayout.Popup("Effect", currentIndex, registryNames);
                if (newIndex != currentIndex)
                {
                    nameProp.stringValue = registryNames[newIndex];
                }

                // Float field for value
                EditorGUILayout.PropertyField(valueProp, new GUIContent("Value"));
                EditorGUILayout.Space();
            }

            // Button to add a new contribution
            if (GUILayout.Button("Add Contribution"))
            {
                listProp.arraySize++;
                var newElement = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                newElement.FindPropertyRelative("name").stringValue = registryNames.FirstOrDefault() ?? "";
                newElement.FindPropertyRelative("value").floatValue = 0f;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

