# InspectorEnumPairGui

The serialization of arrays that are accessed using enums is a bit of an issue in Unity. If you use the integer value or the index of an enum value you cannot change the enum afterwards or the association between the index and value will be lost. This plugin is a solution to that problem.


## How does it work

The plugin consists of a class that stores the enum 'key' and a value. And a class that handles drawing of the custom inspector gui. As you can see in the video below the inspector will show a foldout with one entry for every name in the enum. When changing the enum declaration the inspector will be updated to show the new names and keep associations between existing names and values.

<img src="https://thumbs.gfycat.com/ImpressiveAllLarva-size_restricted.gif" width="854" height="480" />

The example scene shown in the video is included in the project, please use the scripts in the example folder to see if this plugin is a fit for your project.

## Features

- There is always one value associated with one enum name.
- Enum names will be sorted by their underlying integer values.
- Enum integer values do not need to be 0 based or continuous.
- Works with all values Unity normally serializes through the inspector.


## Caveats

- Unity does not serialize classes with generic parameters, so you need to create a [Serializable] class for every enum-value pair to satisfy the generic parameter declaration.
- The inspector gui class is not an editor class because it is possible to have multiple enum-value arrays in an object. So every object that uses this plugin has to have a custom editor class.
- Objects/arrays will only be updated when the object is shown in the inspector in Unity. The InspectorEnumPair class does provide a way to check if the enum name it uses is still valid. I personally feel this is helpful behavior since no values will be changed unless the user visits the object, which put's more control in the user's hands. This class was created specifically to provide a simpler way of using enum names to access values that were set using the object's inspector.


## Known Issues

There is no way to set a default value for the following SerializedProperty types: Character, Generic, Gradient and FixedBufferSize. Passing a GetDefaultValue method to the InspectorEnumPairGui will produce a warning when a new property of these types is assigned. I don't believe Generic or FixedBufferSize can be assigned anyway, but if anyone has any insight into this issue please let me know.