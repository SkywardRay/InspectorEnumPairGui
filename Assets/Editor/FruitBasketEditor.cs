using SkywardRay;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FruitBasket))]
public class FruitBasketEditor : Editor {

	private InspectorEnumPairGui<Fruit, Color> fruitColors;

	private void OnEnable () {
		// Get the serialized property of our Fruit Color pair array
		var property = serializedObject.FindProperty("fruitColors");

		// This object will draw the inspector for our field and handle any changes to the Enum declaration
		// Pass the property and an anonymous function to specify a default value per enum value
		fruitColors = new InspectorEnumPairGui<Fruit, Color>(property, fruit => new Color(0.79f, 1f, 0.77f));
	}

	public override void OnInspectorGUI () {
		// Draw the default Inspector for the target class
		base.OnInspectorGUI();

		// Draw the InspectorEnumPairGui
		fruitColors.OnInspectorGUI();

		// Save any changed values
		serializedObject.ApplyModifiedProperties();
	}

}
