using UnityEngine;
using UnityEditor;
using System.Collections;

namespace OpenCVMarkerBasedAR
{
    [CustomPropertyDrawer(typeof(MarkerDesign))]
    public class MarkerDesignPropertyDrawer : PropertyDrawer
    {
        public bool showPosition = true;

        public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, GUIContent label)
        {
            //Debug.Log (position.ToString ());

            label = EditorGUI.BeginProperty(position, label, property);

            showPosition = EditorGUI.Foldout(new UnityEngine.Rect(position.x, position.y, position.width - 6, 18), showPosition, label);

            if (showPosition)
            {

                //EditorGUI.PrefixLabel (position, label);

                int oldIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

                position = EditorGUI.IndentedRect(position);
                EditorGUI.indentLevel = 0;


                UnityEngine.Rect newposition = position;

                //Debug.Log (newposition.ToString ());

                newposition.y += 18f;

                SerializedProperty gridSize = property.FindPropertyRelative("gridSize");
                EditorGUI.PropertyField(new UnityEngine.Rect(position.x, position.y + 18, position.width, 18), gridSize);
                if (gridSize.intValue <= 0)
                    gridSize.intValue = 1;

                newposition.y += 18f;
                SerializedProperty data = property.FindPropertyRelative("data");

                data.arraySize = gridSize.intValue * gridSize.intValue;

                newposition.width = 18f;
                newposition.height = 18f;

                for (int j = 0; j < gridSize.intValue; j++)
                {
                    newposition.x = position.x + (position.width - (newposition.width * gridSize.intValue)) / 2;
                    newposition.y += 18f;

                    for (int i = 0; i < gridSize.intValue; i++)
                    {

                        EditorGUI.PropertyField(newposition, data.GetArrayElementAtIndex(j * gridSize.intValue + i), GUIContent.none);
                        newposition.x += newposition.width;
                    }
                }

                EditorGUI.indentLevel = oldIndentLevel;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (showPosition)
            {

                SerializedProperty gridSize = property.FindPropertyRelative("gridSize");

                return 18f * (gridSize.intValue + 3);
            }
            else
            {
                return 18f;
            }
        }
    }
}