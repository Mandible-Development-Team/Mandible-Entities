

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Mandible.Entities.Editor
{
    public class AIDecisionScriptCreator
    {
        [MenuItem("Assets/Create/AI Decision Script", false, 80)]
        public static System.Type CreateAIDecisionScript()
        {
            //Get path
            MethodInfo getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod(
                "GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

            string defaultFolder = (string)getActiveFolderPath.Invoke(null, null);
            if (string.IsNullOrEmpty(defaultFolder))
                defaultFolder = "Assets";

            string path = EditorUtility.SaveFilePanelInProject(
                "New AI Decision Script",
                "NewAIDecision.cs",
                "cs",
                "Choose location",
                defaultFolder
            );

            if (string.IsNullOrEmpty(path)) return null;

            //Create from template
            string template = Resources.Load<TextAsset>("Templates/AIDecisionTemplate").text;
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

            return System.Type.GetType(scriptName);
        }
    }
}
