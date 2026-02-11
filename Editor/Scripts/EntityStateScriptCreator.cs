

using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Mandible.Entities.Editor
{
    public class EntityStateScriptCreator
    {
        private static string pendingTypeName;

        [MenuItem("Assets/Create/Entity State Script", false, 80)]
        public static string CreateEntityStateScript()
        {
            //Get path
            MethodInfo getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod(
                "GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

            string defaultFolder = (string)getActiveFolderPath.Invoke(null, null);
            if (string.IsNullOrEmpty(defaultFolder))
                defaultFolder = "Assets";

            string path = EditorUtility.SaveFilePanelInProject(
                "New Entity State Script",
                "NewEntityState.cs",
                "cs",
                "Choose location",
                defaultFolder
            );

            if (string.IsNullOrEmpty(path)) return null;

            //Create from template
            string template = Resources.Load<TextAsset>("Templates/EntityStateTemplate").text;
            string scriptName = Path.GetFileNameWithoutExtension(path);

            string fileContent = template.Replace("#SCRIPT_NAME#", scriptName);
            File.WriteAllText(path, fileContent);

            //Open
            string fullPath = Path.GetFullPath(path);
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            };
            Process.Start(psi);

            return path;
        }

        
        public static void CreateEntityState(string typeName)
        {
            CreateAssetAfterReload(typeName);
        }

        private static void CreateAssetAfterReload(string typeName)
        {
            bool createAsset = EditorUtility.DisplayDialog(
                    "Create Default Asset?",
                    $"Do you want to create a default asset for '{typeName}' now?",
                    "Yes",
                    "No"
                );

            if (!createAsset) return;

            //Create Asset
            string lastFolder = EditorPrefs.GetString("EntityStateFolder", "Assets");
            string assetPath = EditorUtility.SaveFilePanelInProject(
                "Save Entity State",
                typeName + ".asset",
                "asset",
                "Choose location",
                lastFolder
            );

            //Locate Type
            Type type = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = "Mandible.Entities.Actions." + typeName;
                type = assembly.GetType(assemblyName);
                if (type != null) break;
            }
            if (type == null){
                UnityEngine.Debug.LogError("Could not find type: " + typeName);
                return;
            }

            if(!string.IsNullOrEmpty(assetPath))
            {
                var newState = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(newState, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}
