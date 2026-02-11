using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mandible.Registry;

namespace Mandible.Entities.StatusEffects
{
    public class StatusEffectRegistryWindow : EditorWindow
    {
        List<StatusEffectEntry> statusEffects = new List<StatusEffectEntry>();
        List<StatusEffectEntry> statusEffectsForRemoval = new List<StatusEffectEntry>();

        static string iconPath = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/Fire.png";
        static string defaultIcon = "d_PreMatCube";

        bool showError = false;
        string errorMessage = "";

        [MenuItem("Mandible/Entities/Status Effects/Status Effect Registry", false, priority = 1100)]
        public static void Open()
        {
            var window = GetWindow<StatusEffectRegistryWindow>("Status Effect Registry", false);
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (icon == null) 
                icon = EditorGUIUtility.IconContent(defaultIcon).image as Texture2D;

            window.titleContent = new GUIContent("Status Effect Registry", icon);
            window.statusEffects = GetStatusEffectEntries();

            window.Show();
        }

        void OnGUI()
        {
                //Header
                EditorGUILayout.LabelField("Status Effect Registry", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("This tool helps you manage Status Effects.");

                EditorGUILayout.Space();

                if(MandibleData.IsFolderStructureValid())
                {
                    StatusEffectUI();
                }
                else
                {
                    EditorGUILayout.HelpBox("Mandible folder structure is not set up. Please run the Mandible Setup tool.", MessageType.Warning);
                    if(GUILayout.Button("Open Mandible Setup"))
                    {
                        MandibleData.HandleSetup();
                        EditorReload();
                    }
                }
        }

        public void OnEnable()
        {
            AssemblyReloadEvents.afterAssemblyReload += EditorReload;
            EditorReload();
        }

        public void OnDisable()
        {
            AssemblyReloadEvents.afterAssemblyReload -= EditorReload;
            EditorReload();
        }

        void EditorReload()
        {
            StatusEffectRegistry.EditorReload();
            statusEffects = GetStatusEffectEntries();
        }

        void SyncData()
        {
            //Unused for now, need to figure out how to handle deleting Mandible Data folder
            List<StatusEffectData> currentData = statusEffects.Where(x => x.data != null).Select(x => x.data).ToList();
            List<StatusEffectData> registryData = StatusEffectRegistry.GetAllStatusEffectData().ToList();

            int currentCount = currentData.Count;
            int registryCount = registryData.Count;

            if(currentCount != registryCount)
            {
                statusEffects = GetStatusEffectEntries(); //Gets rid of empty entries in UI, maybe will fix later
            }
        }

        //UI

        public void StatusEffectUI()
        {
            //Status Effects List
            EditorGUILayout.LabelField("Status Effects", EditorStyles.boldLabel);

            for (int x = 0; x < statusEffects.Count; x++)
            {
                EditorGUILayout.BeginVertical("box");

                //State Field
                EditorGUILayout.BeginHorizontal();
                string stateLabel = statusEffects[x]?.data != null ? statusEffects[x].data.effectName : "Empty";
                StatusEffectEntry before = new StatusEffectEntry {
                    data = statusEffects[x].data,
                    description = statusEffects[x].description
                };

                EditorGUI.BeginChangeCheck();
                statusEffects[x].data = (StatusEffectData)EditorGUILayout.ObjectField(
                    statusEffects[x].data,
                    typeof(StatusEffectData),
                    false
                );

                if(EditorGUI.EndChangeCheck()){
                    if(statusEffects[x].data == null)
                    {
                        RemoveState(before);
                    }
                    else{
                        Type beforeType = before.data != null ? before.data.CreateRuntimeEffect().GetType() : null;
                        Type type = statusEffects[x].data.CreateRuntimeEffect().GetType();
                        
                        if(type != beforeType && StatusEffectRegistry.HasRegisteredEffectType(type))
                        {
                            errorMessage = $"{statusEffects[x].data.effectName} is already registered. Duplicate types are not allowed.";
                            showError = true;
                            statusEffects[x].data = before.data; //Revert field
                        }
                        else ModifyState(before, statusEffects[x]);
                    }
                }

                if(GUILayout.Button("x", GUILayout.Width(20))) 
                    RemoveState(statusEffects[x]);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(stateLabel, new GUIStyle(EditorStyles.boldLabel) { fontSize = 14});
                EditorGUILayout.EndHorizontal();

                //Description
                EditorGUILayout.BeginHorizontal();
                string desc = statusEffects[x].data != null ? statusEffects[x].data.description : "(No description)";
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
            
            if(showError)
            {
                EditorUtility.DisplayDialog("Error", errorMessage, "OK");
                showError = false;
            }

            foreach (var s in statusEffectsForRemoval){
                if(statusEffects.Contains(s)) statusEffects.Remove(s);
            }
            statusEffectsForRemoval.Clear();

            if (GUILayout.Button("Add State"))
            {
                AddState();
            }
        }

        //API

        void AddState()
        {
            var entry = new StatusEffectEntry();
            if(entry != null)
            {
                statusEffects.Add(entry);
            }
        }

        void RemoveState(StatusEffectEntry entry)
        {
            if(entry != null)
            {
                StatusEffectRegistry.RemoveStatusEffectData(entry.data);
                statusEffectsForRemoval.Add(entry);
            }
        }

        void ModifyState(StatusEffectEntry before, StatusEffectEntry entry)
        {
            if(entry != null)
            {
                StatusEffectRegistry.UpdateStatusEffectData(before.data, entry.data);
            }
        }

        //Helpers

        static List<StatusEffectEntry> GetStatusEffectEntries()
        {
            var entries = new List<StatusEffectEntry>();
            var allData = StatusEffectRegistry.GetAllStatusEffectData();
            foreach (var data in allData)
            {
                var entry = new StatusEffectEntry
                {
                    data = data,
                    description = data != null ? data.description : ""
                };
                entries.Add(entry);
            }
            return entries;
        }
    }

    [System.Serializable]
    public class StatusEffectEntry
    {
        public StatusEffectData data = null;
        public string description = "";
    }
}
