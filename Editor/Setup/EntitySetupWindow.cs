using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Mandible.Entities;

namespace Mandible.Entities.Editor
{
    public class EntitySetupWindow : EditorWindow
    {
        //Application
        static string iconPath = "Packages/com.unity.dt.app-ui/PackageResources/Icons/Regular/LegoSmiley.png";
        static string defaultIcon = "d_PreMatCube";

        //Settings
        private GameObject target;
        private bool keepCurrentValues;
        private bool useRigidbody;
        private EntityDefinition entityTemplate;

        //Dependencies
        private Entity entity;
        private EntityMovement movement;
        private EntityAI ai;
        private EntityStateMachine stateMachine;

        //Components
        private Rigidbody rigidBody;
 
        [MenuItem("Mandible/Entities/Entity Setup Tool", false, priority = 1000)]
        [MenuItem("GameObject/Mandible/Entities/Entity Setup Tool", false, priority = 100)]
        public static void ShowWindow()
        {
            EntitySetupWindow window = GetWindow<EntitySetupWindow>("Entity Setup");
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (icon == null) 
                icon = EditorGUIUtility.IconContent(defaultIcon).image as Texture2D;

            window.titleContent = new GUIContent("Entity Setup Tool", icon);

            //Initialize
            window.target = Selection.activeGameObject;
            window.keepCurrentValues = true;
            window.useRigidbody = true;

            window.Show();
        }

        private void OnGUI()
        {
            //Header
            GUILayout.Label("Entity Setup Tool", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("This tool lets you setup a GameObject as an Entity.");

            EditorGUILayout.Space();

            //General
            target = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("Target", "Target GameObject to setup as an Entity"),
                target,
                typeof(GameObject),
                true
            );

            keepCurrentValues = EditorGUILayout.Toggle(
                new GUIContent("Keep Current Values", "Keep current component values"),
                keepCurrentValues
            );

            useRigidbody = EditorGUILayout.Toggle(
                new GUIContent("Use Rigidbody", "Use Rigidbody component on this entity"),
                useRigidbody
            );

            EditorGUILayout.Space();

            entityTemplate = (EntityDefinition)EditorGUILayout.ObjectField(
                new GUIContent("Entity Definition", "Template to use for setting up this entity"),
                entityTemplate,
                typeof(EntityDefinition),
                false
            );

            GUILayout.Label("Can't find a template?", EditorStyles.boldLabel);
            if(GUILayout.Button("View Tutorial"))
            {
                Application.OpenURL("https://mandible-dev-docs.readthedocs.io/en/latest/entities/entity_setup_tool.html#creating-entity-definitions");
            }

            if (GUILayout.Button("Create New Template"))
            {
                EntityDefinitionCreatorWindow.ShowWindow(def =>
                {
                    if (def != null) entityTemplate = def;
                });
            }

            EditorGUILayout.Space();

            //Setup
            if (GUILayout.Button("Setup"))
            {
                if(CanCreateEntity()){
                    CreateEntity();
                    EditorApplication.delayCall += () =>
                    {
                        Close();
                        EditorUtility.DisplayDialog("Success", "Entity setup complete!", "OK");
                    };
                }
            }
        }

        private void CreateEntity()
        {
            CreateComponents();
            AssignComponents();

            //Setup
            entityTemplate.ApplyToEntity(entity);
        }

        private void CreateComponents()
        {
            //Entity
            if(!target.TryGetComponent<Entity>(out entity))
            {
                entity = target.AddComponent<Entity>();
                entity.usedSetupTool = true;
            }

            //Dependencies
            if (!target.TryGetComponent<EntityMovement>(out movement))
            {
                movement = target.AddComponent<EntityMovement>();
            }
            if (!target.TryGetComponent<EntityAI>(out ai))
            {
                ai = target.AddComponent<EntityAI>();
            }
            if (!target.TryGetComponent<EntityStateMachine>(out stateMachine))
            {
                stateMachine = target.AddComponent<EntityStateMachine>();
            }

            //Components
            if (useRigidbody && target.TryGetComponent<Rigidbody>(out rigidBody) == false)
            {
                rigidBody = target.AddComponent<Rigidbody>();
            }
            else if(!useRigidbody && target.TryGetComponent<Rigidbody>(out rigidBody) != false)
            {
                DestroyImmediate(target.GetComponent<Rigidbody>());
                rigidBody = null;
            }     

            //Reset Component Values
            if(!keepCurrentValues)
            {
                ResetComponentValues();
            }

            //Set Necessary Component References
            if(rigidBody != null) 
            {
                rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            }
            if(movement != null)
            {
                movement.rigidBody = rigidBody;
            }
            if(stateMachine != null)
            {
                stateMachine.animator = target.GetComponentInChildren<Animator>();
            }
        }

        private void ResetComponentValues()
        {
            if(movement != null)
            {
                DestroyImmediate(movement);
                movement = target.AddComponent<EntityMovement>();
            }

            if(ai != null)
            {
                DestroyImmediate(ai);
                ai = target.AddComponent<EntityAI>();
            }

            if(stateMachine != null)
            {
                DestroyImmediate(stateMachine);
                stateMachine = target.AddComponent<EntityStateMachine>();
            }
        }

        private void AssignComponents()
        {
            if(entity != null)
            {
                entity.movement = movement;
                entity.ai = ai;
                entity.stateMachine = stateMachine;
            }
        }

        private bool CanCreateEntity()
        {
            if(target == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a target GameObject.", "OK");
                return false;
            }

            if(entityTemplate == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign or create an Entity Definition template.", "OK");
                return false;
            }

            return true;
        }
    }
}
