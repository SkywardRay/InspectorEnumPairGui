using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace SkywardRay {
    public class InspectorEnumPairGui<TEnum, TValue> where TEnum : struct, IComparable, IConvertible, IFormattable {

        private readonly Func<TEnum, TValue> GetDefaultValue;
        private readonly int enumValueCount;
        private readonly SerializedProperty property;

        public InspectorEnumPairGui ([NotNull] SerializedProperty property, Func<TEnum, TValue> GetDefaultValue = null) {
            this.GetDefaultValue = GetDefaultValue;
            this.property = property;

            // How many values are in the Enum
            enumValueCount = Enum.GetValues(typeof(TEnum)).Length;
        }

        private void FixNames () {
            // How many elements are in the serialized array
            var size = property.arraySize;

            // Get all defined names in the Enum
            var namesInEnum = Enum.GetNames(typeof(TEnum)).ToList();

            // Get the names that are currently in the serialized array
            var namesInArray = Enumerable
                .Range(0, size)
                .Select(i => GetName(property.GetArrayElementAtIndex(i)))
                .ToList();

            // Get the names that are in the Enum but not in the serialized array
            var missingInArray = Enum
                .GetNames(typeof(TEnum))
                .Where(name => !namesInArray.Contains(name))
                .ToList();

            for (var i = 0; i < size; i++) {
                if (missingInArray.Count == 0) {
                    // No more names are missing from the serialized array
                    break;
                }

                var element = property.GetArrayElementAtIndex(i);
                var name = element.FindPropertyRelative("name");

                if (name.stringValue == null || !namesInEnum.Contains(name.stringValue)) {
                    // This element has an invalid name so it can be reused
                    // Assign the first missing name from the list
                    name.stringValue = missingInArray.First();

                    // A name needs to be assigned only once, remove it from the list
                    missingInArray.RemoveAt(0);
                }
            }
        }

        private TEnum GetKey (SerializedProperty element) {
            var name = GetName(element);

            TEnum result;

            if (name == null || !InspectorEnumPair<TEnum, TValue>.TryParseEnumValue(name, out result)) {
                // The name was invalid, we want a valid enum value so return a default one
                return default(TEnum);
            }

            // Return the enum value parsed from the valid name
            return result;
        }

        private string GetName (SerializedProperty element) {
            if (element == null) {
                return null;
            }

            // Get the name property of this element and return it's string value
            return element.FindPropertyRelative("name").stringValue;
        }

        private void MoveMissingNamesToEnd () {
            // All the names in the enum declaration
            var enumNames = Enum.GetNames(typeof(TEnum));

            // The current size of the serialized array
            var size = property.arraySize;

            // Get any values that are in the array but not in the enum declaration
            var missingInEnumDeclaration = Enumerable
                .Range(0, size)
                .Where(i => !enumNames.Contains(GetName(property.GetArrayElementAtIndex(i))))
                .ToList();

            for (var i = 0; i < missingInEnumDeclaration.Count; i++) {
                // Move each missing value to the end of the serialized array
                // By moving them to the end, Unity automatically removes them when the array is resized
                property.MoveArrayElement(missingInEnumDeclaration[i], size - 1 - i);
            }
        }

        public void OnInspectorGUI () {
            ResetElementsThatAreNotInEnum();

            var oldSize = property.arraySize;

            if (enumValueCount > oldSize) {
                // Set the array size so there are the same number of elements as there are enum values
                property.arraySize = enumValueCount;

                // One or more enum values were added to the TEnum type
                // Unity copies the last element when expanding the serialized array
                // Clear the names of the new elements
                ResetNewElementNames(oldSize);

                // Make sure all elements have a name and a key
                FixNames();

                // Clear the values of the new elements
                // This is done after setting the names because the default value may be different per enum value
                ResetNewElementValues(oldSize);
            }
            else if (enumValueCount < oldSize) {
                // One or more enum values were removed from the TEnum type
                MoveMissingNamesToEnd();

                // Set the array size so there are the same number of elements as there are enum values
                property.arraySize = enumValueCount;

                // Make sure all elements have a name and a key
                FixNames();
            }
            else {
                // Make sure all elements have a name and a key
                FixNames();
            }

            // Get all names in the enum declaration
            var namesInEnum = Enum.GetNames(typeof(TEnum)).ToList();

            // Get all elements in the serialized property as a list
            var elements = Enumerable
                .Range(0, property.arraySize)
                .Select(i => property.GetArrayElementAtIndex(i))
                .ToList();

            // Sort the elements by the index of the enum value in the list of all enum values
            elements.Sort((a, b) => {
                var aIndex = namesInEnum.IndexOf(GetName(a));
                var bIndex = namesInEnum.IndexOf(GetName(b));

                return aIndex.CompareTo(bIndex);
            });

            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, ObjectNames.NicifyVariableName(property.name));
            if (property.isExpanded) {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical();

                foreach (var element in elements) {
                    EditorGUILayout.BeginHorizontal();

                    // Draw a label showing the element's name
                    EditorGUILayout.LabelField(GetName(element), GUILayout.MinWidth(116));

                    // Draw a field that allows the user to assign a value to this element
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("value"), new GUIContent(""));

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
        }

        private void ResetElementsThatAreNotInEnum () {
            // Get all names from the enum declaration
            var namesInEnum = Enum.GetNames(typeof(TEnum)).ToList();

            for (var i = 0; i < property.arraySize; i++) {
                var name = GetName(property.GetArrayElementAtIndex(i));

                if (!namesInEnum.Contains(name)) {
                    // The enum does not contain a value for this name, so the value in this element
                    // should be reset. We cannot get the default value because the name is not in the enum
                    //
                    // Unity can throw errors when manually resetting a serialized value, so we just
                    // delete the array element. The array will be resized later because there are
                    // missing values. Unity will then reset the new element for us
                    property.DeleteArrayElementAtIndex(i);

                    // An element was removed so we want to visit the new element at this index
                    i--;
                }
            }
        }

        private void ResetNewElementNames (int oldSize) {
            for (var i = oldSize; i < property.arraySize; i++) {
                var element = property.GetArrayElementAtIndex(i);

                var nameProperty = element.FindPropertyRelative("name");

                if (nameProperty != null) {
                    // Assign the default string value so we know that this element
                    // can be reused for a missing enum name
                    nameProperty.stringValue = default(string);
                }
            }
        }

        private void ResetNewElementValues (int oldSize) {
            for (var i = oldSize; i < property.arraySize; i++) {
                // Assign the default value to this element
                SetDefaultValue(property.GetArrayElementAtIndex(i));
            }
        }

        private void SetDefaultValue (SerializedProperty element) {
            if (GetDefaultValue == null) {
                // No way to get a default value because no method was passed to the constructor
                return;
            }

            // Get the default value for this enum value as an object so casting is a little easier
            object value = GetDefaultValue(GetKey(element));

            // The property we want to assign the value to
            var elementValue = element.FindPropertyRelative("value");

            // Assign the value to the serialized property based on the property type
            switch (elementValue.propertyType) {
            case SerializedPropertyType.AnimationCurve:
                elementValue.animationCurveValue = (AnimationCurve)value;
                break;
            case SerializedPropertyType.ArraySize:
                elementValue.intValue = (int)value;
                break;
            case SerializedPropertyType.Boolean:
                elementValue.boolValue = (bool)value;
                break;
            case SerializedPropertyType.Bounds:
                elementValue.boundsValue = (Bounds)value;
                break;
            case SerializedPropertyType.BoundsInt:
                elementValue.boundsIntValue = (BoundsInt)value;
                break;
            case SerializedPropertyType.Color:
                elementValue.colorValue = (Color)value;
                break;
            case SerializedPropertyType.Enum:
                // Unity stores the enum value based on the index in the list of enum values
                elementValue.enumValueIndex = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList().IndexOf((TEnum)value);
                break;
            case SerializedPropertyType.ExposedReference:
                elementValue.objectReferenceValue = value as UnityEngine.Object;
                break;
            case SerializedPropertyType.Float:
                elementValue.doubleValue = (double)value;
                break;
            case SerializedPropertyType.Integer:
                elementValue.longValue = (long)value;
                break;
            case SerializedPropertyType.LayerMask:
                elementValue.intValue = ((LayerMask)value).value;
                break;
            case SerializedPropertyType.ObjectReference:
                elementValue.objectReferenceValue = value as UnityEngine.Object;
                break;
            case SerializedPropertyType.Quaternion:
                elementValue.quaternionValue = (Quaternion)value;
                break;
            case SerializedPropertyType.Rect:
                elementValue.rectValue = (Rect)value;
                break;
            case SerializedPropertyType.RectInt:
                elementValue.rectIntValue = (RectInt)value;
                break;
            case SerializedPropertyType.String:
                elementValue.stringValue = (string)value;
                break;
            case SerializedPropertyType.Vector2:
                elementValue.vector2Value = (Vector2)value;
                break;
            case SerializedPropertyType.Vector2Int:
                elementValue.vector2IntValue = (Vector2Int)value;
                break;
            case SerializedPropertyType.Vector3:
                elementValue.vector3Value = (Vector3)value;
                break;
            case SerializedPropertyType.Vector3Int:
                elementValue.vector3IntValue = (Vector3Int)value;
                break;
            case SerializedPropertyType.Vector4:
                elementValue.vector4Value = (Vector4)value;
                break;
            case SerializedPropertyType.Character:
            case SerializedPropertyType.Generic:
            case SerializedPropertyType.Gradient:
            case SerializedPropertyType.FixedBufferSize:
                Debug.LogWarning("Unable to assign default value to serialized property of type " + elementValue.propertyType);
                break;
            default:
                throw new NotImplementedException("Usage of type " + elementValue.propertyType + " is not supported by this class.");
            }
        }

    }
}
