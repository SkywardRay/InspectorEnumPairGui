using UnityEngine;

namespace SkywardRay {
    public class FruitBasket : MonoBehaviour {

        // We do want Unity to serialize our array but we don't want it shown in the inspector
        // because we use InspectorEnumPairGui to display the field
        [SerializeField, HideInInspector]
        private FruitColorPair[] fruitColors;

        private void Start () {
            // Specify a starting point so the object are aligned to the center
            var startingPoint = new Vector3(-fruitColors.Length / 2f, 0, 0);

            // Demostrate that everything works by creating a simple object for every enum value
            for (var i = 0; i < fruitColors.Length; i++) {
                var pair = fruitColors[i];

                // Create a simple sphere object
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                // Assign the fruit color to the object
                go.GetComponent<MeshRenderer>().material.color = pair.value;

                // Assign the fruit name to the object
                go.name = pair.name;

                // Move the objects so they're not all in one spot
                go.transform.position = startingPoint + new Vector3(i, 0, 0);

                // The objects are close together, so we scale them to create some space in between
                go.transform.localScale = Vector3.one * 0.9f;
            }

            // A simple extension method allows access to a specific value using the enum name
            Debug.Log("Apple has the color: " + fruitColors.Get(Fruit.Apple).value);
        }

    }
}
