using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using Mandible.Registry;

namespace Mandible.Entities.StatusEffects
{
    internal static class StatusEffectEnum
    {
        private const string Namespace = "Mandible.Entities.StatusEffects";
        private const string RegistryFolder = "Entities/StatusEffects/Registry";

        private static string GetOutputPath()
        {
            string rootFolder = MandibleData.GetRootFolder();

            if (string.IsNullOrEmpty(rootFolder))
            {
                Debug.LogError("StatusEffectEnum: Mandible root folder not found.");
                return null;
            }

            string fullPath = Path.Combine(
                rootFolder,
                "Data",
                RegistryFolder,
                "StatusEffectId.cs"
            );

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

            foreach (var effect in effects)
            {
                if (string.IsNullOrWhiteSpace(effect.effectName))
                    continue;

                sb.AppendLine($"        {MakeSafe(effect.effectName)},");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            string outputPath = GetOutputPath();
            if (string.IsNullOrEmpty(outputPath)) return;

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            string newText = sb.ToString();

            if (File.Exists(outputPath))
            {
                string existing = File.ReadAllText(outputPath);
                if (existing == newText) return;
            }

            File.WriteAllText(outputPath, newText);
            AssetDatabase.Refresh();
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