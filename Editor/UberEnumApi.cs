namespace Game.Editor.UberEnums
{
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityCodeGen;
    using UnityEditor;
    using UnityEngine;

    public static class UberEnumApi
    {
        private const string ScriptFileGeneratorTypeName = "ScriptFileGenerator";
        private const string IsGeneratingMemberName = "isGenerating";

        private static UberEnumConfig _instance;

        static UberEnumApi()
        {
            _instance = ScriptableSingleton<UberEnumConfig>.instance;
        }

        internal static EnumData CreateEnum(string name, EnumValue[] values, bool overwrite = false)
        {
            var instance = GetConfig();
            values = values ?? Array.Empty<EnumValue>();

            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogError("Enum name cannot be empty");
                return null;
            }

            var contains = instance.Contains(name);
            if (contains && !overwrite)
            {
                Debug.LogError("Enum with the same name already exists");
                return null;
            }
            
            for (var i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    Debug.LogError($"Enum '{name}' has null value at index {i}");
                    return null;
                }

                values[i].index = i;
            }

            var enumData = new EnumData
            {
                name = name,
                values = values
            };

            instance[name] = enumData;
            instance.SaveConfig();

            AssetDatabase.SaveAssets();

            return enumData;
        }

        internal static bool Generate(EnumData data)
        {
            var instance = GetConfig();
            ResetUnityCodeGenState();
            ClearCurrentIndex(instance);

            if (!TryValidateEnumData(data, out var error))
            {
                Debug.LogError(error);
                return false;
            }

            var values = data.values;
            for (var i = 0; i < values.Length; i++)
            {
                var duplicateCount = 0;
                for (var j = i; j < values.Length; j++)
                {
                    duplicateCount += values[i].value == values[j].value ? 1 : 0;
                }

                if (duplicateCount <= 1) continue;
                Debug.LogError("Enum cannot contain similar values");
                return false;
            }

            instance.currentIndex = data.name;
            instance.SaveConfig();

            try
            {
                UnityCodeGenUtility.Generate();
                AssetDatabase.SaveAssets();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return false;
            }
            finally
            {
                ResetUnityCodeGenState();
                if (instance.currentIndex == data.name)
                {
                    ClearCurrentIndex(instance);
                }
            }
        }

        public static bool CreateAndGenerateEnum(string name, IUberEnumConvertible convertible, string path = null,
            string @namespace = null, bool overwrite = false, bool isStrictlyOrdered = true, bool isFlags = false,
            bool isReadOnly = true)
        {
            if (convertible == null)
            {
                Debug.LogError("Enum source cannot be null");
                return false;
            }

            var sourceValues = convertible.Values;
            if (sourceValues == null)
            {
                Debug.LogError("Enum source values cannot be null");
                return false;
            }

            var convertibleCollection = sourceValues.ToArray();
            var values = new EnumValue[convertibleCollection.Length];
            for (var i = 0; i < convertibleCollection.Length; i++)
            {
                var convertibleValue = convertibleCollection[i];
                if (convertibleValue == null)
                {
                    Debug.LogError($"Enum source has null value at index {i}");
                    return false;
                }

                values[i] = new EnumValue
                {
                    name = convertibleValue.Name,
                    index = i,
                    value = i,
                    isReadOnly = isReadOnly,
                    isStrictlyOrdered = isStrictlyOrdered
                };

                convertibleValue.Value = i;
            }

            var enumData = CreateEnum(name, values, overwrite);
            if (enumData == null)
            {
                return false;
            }

            enumData.isStrictlyOrdered = isStrictlyOrdered;
            enumData.isFlags = isFlags;
            enumData.isReadOnly = isReadOnly;
            if (!string.IsNullOrEmpty(path))
            {
                enumData.path = path;
            }

            if (!string.IsNullOrEmpty(@namespace))
            {
                enumData.@namespace = @namespace;
            }

            return Generate(enumData);
        }

        internal static bool TryValidateEnumData(EnumData data, out string error)
        {
            if (data == null)
            {
                error = "Enum data cannot be null";
                return false;
            }

            var enumName = NormalizeIdentifier(data.name);
            if (!IsValidIdentifier(enumName))
            {
                error = $"Enum name '{data.name}' is not a valid C# identifier";
                return false;
            }

            if (!IsValidNamespace(data.@namespace))
            {
                error = $"Enum namespace '{data.@namespace}' is not a valid C# namespace";
                return false;
            }

            if (string.IsNullOrWhiteSpace(data.path))
            {
                error = $"Output path for enum '{data.name}' cannot be empty";
                return false;
            }

            if (data.values == null)
            {
                error = $"Enum '{data.name}' values cannot be null";
                return false;
            }

            for (var i = 0; i < data.values.Length; i++)
            {
                var value = data.values[i];
                if (value == null)
                {
                    error = $"Enum '{data.name}' has null value at index {i}";
                    return false;
                }

                var valueName = NormalizeIdentifier(value.name);
                if (!IsValidIdentifier(valueName))
                {
                    error = $"Enum '{data.name}' value '{value.name}' is not a valid C# identifier";
                    return false;
                }
            }

            error = null;
            return true;
        }

        internal static string NormalizeIdentifier(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace(" ", string.Empty);
        }

        private static UberEnumConfig GetConfig()
        {
            if (_instance == null)
            {
                _instance = ScriptableSingleton<UberEnumConfig>.instance;
            }

            return _instance;
        }

        private static void ClearCurrentIndex(UberEnumConfig instance)
        {
            if (instance.currentIndex == string.Empty)
            {
                return;
            }

            instance.currentIndex = string.Empty;
            instance.SaveConfig();
        }

        private static bool IsValidNamespace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var parts = value.Split('.');
            return parts.All(IsValidIdentifier);
        }

        private static bool IsValidIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !IsIdentifierStart(value[0]))
            {
                return false;
            }

            for (var i = 1; i < value.Length; i++)
            {
                if (!IsIdentifierPart(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsIdentifierStart(char value)
        {
            return value == '_' || char.IsLetter(value);
        }

        private static bool IsIdentifierPart(char value)
        {
            return value == '_' || char.IsLetterOrDigit(value);
        }

        private static void ResetUnityCodeGenState()
        {
            try
            {
                var type = FindScriptFileGeneratorType();
                if (type == null)
                {
                    return;
                }

                const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                var property = type.GetProperty(IsGeneratingMemberName, flags);
                var setter = property?.GetSetMethod(true);
                if (property?.PropertyType == typeof(bool) && setter != null)
                {
                    setter.Invoke(null, new object[] { false });
                    return;
                }

                var field = type.GetField(IsGeneratingMemberName, flags);
                if (field?.FieldType == typeof(bool))
                {
                    field.SetValue(null, false);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Unable to reset UnityCodeGen generation state: {exception.Message}");
            }
        }

        private static Type FindScriptFileGeneratorType()
        {
            Type fallback = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in GetTypes(assembly))
                {
                    if (type.Name != ScriptFileGeneratorTypeName)
                    {
                        continue;
                    }

                    if (ContainsUnityCodeGen(type.FullName) ||
                        ContainsUnityCodeGen(type.Assembly.GetName().Name))
                    {
                        return type;
                    }

                    fallback = fallback ?? type;
                }
            }

            return fallback;
        }

        private static Type[] GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types
                    .Where(x => x != null)
                    .ToArray();
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static bool ContainsUnityCodeGen(string value)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf("UnityCodeGen", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}