using UnityEditor;
using UnityEngine;

namespace Framework
{
    namespace Interaction.Toolkit
    {
        namespace Editor
        {
            [CustomEditor(typeof(XRSimpleSliderConstraint)), CanEditMultipleObjects]
            public class XRSimpleSliderConstraintEditor : UnityEditor.Editor
            {
                protected SerializedProperty _sliderAxis;
                protected SerializedProperty _sliderSize;

                public static readonly GUIContent sliderAxis = EditorGUIUtility.TrTextContent("Slider Axis", "The local space axis the slider will move along.");
                public static readonly GUIContent sliderSize = EditorGUIUtility.TrTextContent("Slider Length", "The extent of the sliders movement range along its axis.");
                    
                protected virtual void OnEnable()
                {
                    _sliderAxis = serializedObject.FindProperty("_sliderAxis");
                    _sliderSize = serializedObject.FindProperty("_sliderSize");
                }

                protected virtual void DrawProperties()
                {
                    EditorGUILayout.Space();

                    //Draw slider properties
                    EditorGUILayout.PropertyField(_sliderAxis, sliderAxis);
                    EditorGUILayout.PropertyField(_sliderSize, sliderSize);
                }
            }
        }
    }
}