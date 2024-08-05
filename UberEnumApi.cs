namespace Game.Editor.UberEnums
{
    using System;
    using System.Linq;
    using UnityCodeGen;
    using UnityEditor;
    using UnityEngine;

    public static class UberEnumApi
    {
        private static UberEnumConfig _instance;
        
        static UberEnumApi()
        {
            _instance = ScriptableSingleton<UberEnumConfig>.instance;
        }

        internal static EnumData CreateEnum(string name, EnumValue[] values, bool overwrite = false)
        {
            var contains = _instance.Contains(name);
            if (contains && !overwrite)
            {
                Debug.LogError("Enum with the same name already exists");
                return null;
            }

            for (var i = 0; i < values.Length; i++)
            {
                values[i].index = i;
            }

            var enumData = new EnumData
            {
                name = name,
                values = values
            };
            
            _instance[name] = enumData;
            _instance.SaveConfig();
            
            AssetDatabase.SaveAssets();

            return enumData;
        }

        internal static bool Generate(EnumData data)
        {
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

            _instance.currentIndex = data.name;
            _instance.SaveConfig();
            UnityCodeGenUtility.Generate(); 
            
            Type.GetType("ScriptFileGenerator")?.GetProperty("isGenerating")?.SetValue(null, false);
                
            AssetDatabase.SaveAssets();
            return true;
        }

        public static bool CreateAndGenerateEnum(string name, IUberEnumConvertible convertible, string path = null, 
            string @namespace = null, bool overwrite = false, bool isStrictlyOrdered = true, bool isFlags = false, 
            bool isReadOnly = true)
        {
            var convertibleCollection = convertible.Values.ToArray();
            var values = new EnumValue[convertibleCollection.Length];
            for (var i = 0; i < convertibleCollection.Length; i++)
            {
                var convertibleValue = convertibleCollection[i];
                values[i] = new EnumValue
                {
                    name = convertibleValue.Name,
                    index = i,
                    value = i,
                    isReadOnly = isReadOnly,
                    isStrictlyOrdered = isStrictlyOrdered
                };

                convertible.Values.ElementAt(i).Value = i;
            }
            
            var enumData = CreateEnum(name, values, overwrite);
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
    }
}