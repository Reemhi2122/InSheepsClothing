using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.PostProcessing;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Assets.Editor
{
    [CustomEditor(typeof(PostProcessProfileAnimation))]
    public class PostProcessProfileAnimationEditor: UnityEditor.Editor
    {
        private SerializedProperty Target;
        private SerializedProperty Progress;
        private SerializedProperty InitialState;
        private SerializedProperty TargetState;

        private UnityEditor.Editor InitialStateEditor;
        private UnityEditor.Editor TargetStateEditor;

        private PostProcessProfileAnimation _object;

        private int PickerID;
        private bool FirstOrSecond;

        void OnEnable()
        {
            _object = (PostProcessProfileAnimation) target;

            Target = serializedObject.FindProperty("Target");
            Progress = serializedObject.FindProperty("Progress");
            InitialState = serializedObject.FindProperty("InitialState");
            TargetState = serializedObject.FindProperty("TargetState");

            if (_object.InitialState == null)
            {
                _object.InitialState = CreateInstance<PostProcessProfile>();
            }
            InitialStateEditor = CreateEditor(_object.InitialState);

            if (_object.TargetState == null)
            {
                _object.TargetState = CreateInstance<PostProcessProfile>();
            }
            TargetStateEditor = CreateEditor(_object.TargetState);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            EditorGUILayout.PropertyField(Target);
            EditorGUILayout.PropertyField(Progress);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            GUILayout.BeginHorizontal(GUI.skin.box);

            EditorGUIUtility.labelWidth = 5;

            EditorGUILayout.LabelField("Initial state", new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter
            });

            EditorGUIUtility.labelWidth = 0;

            if (GUILayout.Button("Import from assets"))
            {
                PickerID = GUIUtility.GetControlID(FocusType.Passive);
                EditorGUIUtility.ShowObjectPicker<PostProcessProfile>(null, false, "", PickerID);
                FirstOrSecond = false;
            }

            if (GUILayout.Button("Reset"))
            {
                _object.InitialState = CreateInstance<PostProcessProfile>();
                InitialStateEditor = CreateEditor(_object.InitialState);
            }

            if (GUILayout.Button("Save"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Save the profile", "", "asset", "Select where to save the profile");

                if (!string.IsNullOrEmpty(path))
                {
                    var savingCopy = CreateInstance<PostProcessProfile>();

                    DeepCopy(_object.InitialState, savingCopy);

                    AssetDatabase.CreateAsset(savingCopy, path);
                }
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (Event.current.commandName == "ObjectSelectorUpdated" &&
                EditorGUIUtility.GetObjectPickerControlID() == PickerID)
            {
                var picked = EditorGUIUtility.GetObjectPickerObject();
                var copy = CreateInstance<PostProcessProfile>();

                if (picked != null)
                    DeepCopy((PostProcessProfile)picked, copy);

                if (!FirstOrSecond)
                {
                    _object.InitialState = copy;
                    InitialStateEditor = CreateEditor(_object.InitialState);
                }
                else
                {
                    _object.TargetState = copy;
                    TargetStateEditor = CreateEditor(_object.TargetState);
                }
            }

            InitialStateEditor.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            

            GUILayout.BeginHorizontal(GUI.skin.box);

            EditorGUIUtility.labelWidth = 5;

            EditorGUILayout.LabelField("Target state", new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter
            });

            EditorGUIUtility.labelWidth = 0;

            if (GUILayout.Button("Import from assets"))
            {
                PickerID = GUIUtility.GetControlID(FocusType.Passive);
                EditorGUIUtility.ShowObjectPicker<PostProcessProfile>(null, false, "", PickerID);
                FirstOrSecond = true;
            }

            if (GUILayout.Button("Reset"))
            {
                _object.TargetState = CreateInstance<PostProcessProfile>();
                TargetStateEditor = CreateEditor(_object.TargetState);
            }

            if (GUILayout.Button("Save"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Save the profile", "", "asset", "Select where to save the profile");

                if (!string.IsNullOrEmpty(path))
                {
                    var savingCopy = CreateInstance<PostProcessProfile>();

                    DeepCopy(_object.TargetState, savingCopy);

                    AssetDatabase.CreateAsset(savingCopy, path);
                }
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            TargetStateEditor.OnInspectorGUI();

            

            serializedObject.ApplyModifiedProperties();
        }

        private void DeepCopy(PostProcessProfile source, PostProcessProfile target)
        {
            foreach (var setting in source.settings)
            {
                var newInstance = target.AddSettings(setting.GetType());

                var parameters = setting.GetType().GetFields()
                    .Where(f => f.FieldType.IsSubclassOf(typeof(ParameterOverride))).ToArray();

                foreach (var parameter in parameters)
                {
                    var valField = parameter.FieldType.GetField("value");

                    var sParameter = (ParameterOverride)parameter.GetValue(setting);
                    var tParameter = (ParameterOverride)parameter.GetValue(newInstance);

                    valField.SetValue(tParameter, valField.GetValue(sParameter));

                    tParameter.overrideState = sParameter.overrideState;
                }
            }
        }
    }
}