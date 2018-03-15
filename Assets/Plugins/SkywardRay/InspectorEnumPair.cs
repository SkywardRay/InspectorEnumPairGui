using System;
using System.Collections.Generic;
using System.Linq;

namespace SkywardRay {
    public abstract class InspectorEnumPair<TEnum, TValue> where TEnum : struct, IComparable, IConvertible, IFormattable {

        #region static

        public static bool TryParseEnumValue (string stringValue, out TEnum enumValue) {
            // Does the string have a corresponding value in the Enum?
            if (!Enum.IsDefined(typeof(TEnum), stringValue)) {
                // The name was not found
                enumValue = default(TEnum);

                return false;
            }

            // The name exists, convert it to the Enum value
            enumValue = (TEnum)Enum.Parse(typeof(TEnum), stringValue);

            return true;
        }

        #endregion

        public bool IsKeyValid { get; private set; }

        public TEnum Key {
            get {
                if (name != _nameUsedToParseKey) {
                    ParseKey();
                }

                return _key;
            }
        }

        public string name;
        public TValue value;

        [NonSerialized]
        private TEnum _key;

        [NonSerialized]
        private string _nameUsedToParseKey;

        protected InspectorEnumPair (TEnum key, TValue value) {
            this.value = value;

            name = Enum.GetName(typeof(TEnum), key);
        }

        private void ParseKey () {
            // Get the Enum value based on the name field and store whether it even exists in the Enum declaration
            IsKeyValid = TryParseEnumValue(name, out _key);

            // Store the name we used to parse so we know if the key needs to be parsed again
            _nameUsedToParseKey = name;
        }

    }

    public static class EnumValuePairExtensionMethods {

        public static InspectorEnumPair<TEnum, TValue> Get<TEnum, TValue> (
            this IEnumerable<InspectorEnumPair<TEnum, TValue>> collection,
            TEnum key
        ) where TEnum : struct, IComparable, IConvertible, IFormattable {
            // Get the first element from the collection that has the requested key
            return collection.FirstOrDefault(item => item.Key.CompareTo(key) == 0);
        }

    }
}
