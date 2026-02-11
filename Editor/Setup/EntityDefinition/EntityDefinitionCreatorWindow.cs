using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

using System.Collections;
using System.Collections.Generic;

using Mandible.Entities;
using Mandible.Entities.AI;
using Mandible.Entities.Editor;

namespace Mandible.Entities.Editor
{
    public class EntityDefinitionCreatorWindow : EditorWindow
    {
        private List<EntityState> entityStates = new List<EntityState>();
        private List<AIDecision> aiDecisions = new List<AIDecision>();

        //Internal
        private System.Action<EntityDefinition> onCreated;
        List<EntityState> entityStatesForRemoval = new List<EntityState>();
        List<AIDecision> aiDecisionsForRemoval = new List<AIDecision>();

        public static void ShowWindow(System.Action<EntityDefinition> onCreated)
        {
            var window = GetWindow<EntityDefinitionCreatorWindow>("Entity Definition Creator");
            window.onCreated = onCreated;
            window.Show();
        }

        private void OnGUI()
        {
            //Header
            EditorGUILayout.LabelField("Entity Definition Creator", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("This tool helps you create new Entity Definition templates.");

            EditorGUILayout.Space();

            //States
            EditorGUILayout.LabelField("Define States", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("What is your Entity capable of doing?");
            
            for (int x = 0; x < entityStates.Count; x++)
            {
                EditorGUILayout.BeginVertical("box");

                //State Field
                EditorGUILayout.BeginHorizontal();

                string stateLabel = entityStates[x] != null ? entityStates[x].name : "Empty State";
                entityStates[x] = (EntityState)EditorGUILayout.ObjectField(
                    new GUIContent(stateLabel, "State object for " + stateLabel),
                    entityStates[x],
                    typeof(EntityState),
                    false
                );

                if(GUILayout.Button("x", GUILayout.Width(20))) 
                    entityStatesForRemoval.Add(entityStates[x]);

                EditorGUILayout.EndHorizontal();

                //Description
                EditorGUILayout.BeginHorizontal();
                string desc = entityStates[x] != null ? entityStates[x].description : "(No description)";
                EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(
                    new GUIContent(desc, "Description for " + stateLabel),
                    new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperLeft, wordWrap = true },
                    GUILayout.Height(40)
                );
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            foreach (var s in entityStatesForRemoval)
                entityStates.Remove(s);
            entityStatesForRemoval.Clear();

            if (GUILayout.Button("Add State"))
            {
                entityStates.Add(null);
            }

            if (GUILayout.Button("Create State Definition"))
            {
                EntityStateScriptCreator.CreateEntityStateScript();
            }

            if(GUILayout.Button("View Tutorial"))
            {
                Application.OpenURL("https://mandible-dev-docs.readthedocs.io/en/latest/entities/entity_setup_tool.html#creating-entity-definitions");
            }

            EditorGUILayout.Space();

            //Decisions
            EditorGUILayout.LabelField("Define Decisions (AI)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("How will your Entity decide what to do?");

            for (int x = 0; x < aiDecisions.Count; x++)
            {
                EditorGUILayout.BeginVertical("box");

                //Decision Field
                EditorGUILayout.BeginHorizontal();

                string stateLabel = aiDecisions[x] != null ? aiDecisions[x].name : "Empty Decision";

                aiDecisions[x] = (AIDecision)EditorGUILayout.ObjectField(
                    new GUIContent(stateLabel, "Decision object for " + stateLabel),
                    aiDecisions[x],
                    typeof(AIDecision),
                    false
                );

                if(GUILayout.Button("x", GUILayout.Width(20)))
                    aiDecisionsForRemoval.Add(aiDecisions[x]);
                
                EditorGUILayout.EndHorizontal();
                
                //Description
                EditorGUILayout.BeginHorizontal();
                string desc = aiDecisions[x] != null ? aiDecisions[x].description : "(No description)";
                EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(
                    new GUIContent(desc, "Description for " + stateLabel),
                    new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperLeft, wordWrap = true },
                    GUILayout.Height(40)
                );
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            foreach (var s in aiDecisionsForRemoval)
                aiDecisions.Remove(s);
            aiDecisionsForRemoval.Clear();

            if (GUILayout.Button("Add Decision"))
            {
                aiDecisions.Add(null);
            }

            if (GUILayout.Button("Create Decision Definition"))
            {
                AIDecisionScriptCreator.CreateAIDecisionScript();
            }

            if(GUILayout.Button("View Tutorial"))
            {
                Application.OpenURL("https://mandible-dev-docs.readthedocs.io/en/latest/entities/entity_setup_tool.html#creating-entity-definitions");
            }

            EditorGUILayout.Space();

            //Setup
            if (GUILayout.Button("Create New Entity Definition"))
            {
                EntityDefinition def = CreateDefinitionAsset();
                if(def == null) return;
                
                onCreated?.Invoke(def);
                Close();                    
            }
        }

        private EntityDefinition CreateDefinitionAsset()
        {
            var def = ScriptableObject.CreateInstance<EntityDefinition>();
            def.aiTemplate = aiDecisions;
            def.stateTemplate = entityStates;

            string path = EditorUtility.SaveFilePanelInProject(
                "Create Entity Definition",
                "NewEntityDefinition",
                "asset",
                "Choose location"
            );

            if (string.IsNullOrEmpty(path)) return null;

            AssetDatabase.CreateAsset(def, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return def;
        }
    }
}
