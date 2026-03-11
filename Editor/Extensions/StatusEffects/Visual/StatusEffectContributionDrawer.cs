#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Mandible.Entities.StatusEffects
{
    [CustomPropertyDrawer(typeof(StatusEffectContribution))]
    public class StatusEffectContributionDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var nameProp = property.FindPropertyRelative("name");
            var valueProp = property.FindPropertyRelative("value");

            // Get effect names from registry
            var registryNames = new List<string> { "None" };
            registryNames.AddRange(StatusEffectRegistry.All.Select(d => d.effectName));

            int currentIndex = string.IsNullOrEmpty(nameProp.stringValue) ? 0 : registryNames.IndexOf(nameProp.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            EditorGUI.BeginProperty(position, label, property);

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var rect = new Rect(position.x, position.y, position.width, lineHeight);

            // EffectName (dropdown)
            int newIndex = EditorGUI.Popup(rect, "Effect", currentIndex, registryNames.ToArray());
            if (newIndex != currentIndex)
                nameProp.stringValue = newIndex == 0 ? "" : registryNames[newIndex];

            // Value
            rect.y += lineHeight + 2;
            EditorGUI.PropertyField(rect, valueProp, new GUIContent("Value"));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 2; // two lines
        }
    }
}
#endif
