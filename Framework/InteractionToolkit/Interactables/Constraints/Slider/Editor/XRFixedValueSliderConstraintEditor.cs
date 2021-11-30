using UnityEditor;
using UnityEngine;

namespace Framework
{
    namespace Interaction.Toolkit
    {
        namespace Editor
        {
            [CustomEditor(typeof(XRFixedValueSliderConstraint)), CanEditMultipleObjects]
            public class XRFixedValueSliderConstraintEditor : XRSimpleSliderConstraintEditor
            {
                protected SerializedProperty _allowedSliderPositions;
                protected SerializedProperty _movementCurve;
                protected SerializedProperty _snapToPositionTime;

                public static readonly GUIContent allowedSliderPositions = EditorGUIUtility.TrTextContent("Slider Positions", "The normalised positions (zero to one) the slider is allowed to be at along its lenght on the slider axis.");
                public static readonly GUIContent movementCurve = EditorGUIUtility.TrTextContent("Slider Movement Curve", "A curve used to make slider movement sticky around its allowed positions.");
                public static readonly GUIContent snapToPositionTime = EditorGUIUtility.TrTextContent("Slider Snap To Position Time", "The time it takes for the slider to move back to it's nearest allowed position when not grabbed.");

                protected override void OnEnable()
                {
                    base.OnEnable();

                    _allowedSliderPositions = serializedObject.FindProperty("_allowedSliderPositions");
                    _movementCurve = serializedObject.FindProperty("_movementCurve");
                    _snapToPositionTime = serializedObject.FindProperty("_snapToPositionTime");
                }

                protected override void DrawProperties()
                {
                    base.DrawProperties();

                    EditorGUILayout.Space();

                    //Draw slider properties
                    EditorGUILayout.PropertyField(_allowedSliderPositions, allowedSliderPositions);
                    EditorGUILayout.PropertyField(_movementCurve, movementCurve);
                    EditorGUILayout.PropertyField(_snapToPositionTime, snapToPositionTime);
                }
            }
        }
    }
}