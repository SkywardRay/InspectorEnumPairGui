using System;
using UnityEngine;

namespace SkywardRay {
    // This class exists because Unity doesn't serialize classes with generic parameters
    // It has to be marked as Serializable
    [Serializable]
    public class FruitColorPair : InspectorEnumPair<Fruit, Color> {

        public FruitColorPair (Fruit key, Color value) : base(key, value) { }

    }
}
