namespace Game.Editor.UberEnums
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using UnityEngine;
    using UnityEditor;
    using FilePathAttribute = UnityEditor.FilePathAttribute;

    [Serializable]
    [FilePath("Assets/UniGame.Generated/UberEnumConfig", FilePathAttribute.Location.ProjectFolder)]
    public class UberEnumConfig : ScriptableSingleton<UberEnumConfig>
    {
        public List<EnumData> enumData = new();
        public string currentIndex;

        public EnumData this[string key]
        {
            get => Find(key);
            set
            {
                var index = GetIndex(key);
                if (index < 0)
                {
                    enumData.Add(value);
                    return;
                }
                enumData[index] = value;
            }
        }

        public int GetIndex(string id)
        {
            for (var i = 0; i < enumData.Count; i++)
            {
                var item = enumData[i];
                if (item.name.Equals(id, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }
        
        public EnumData Find(string id)
        {
            return enumData.FirstOrDefault(x => 
                x.name.Equals(id,StringComparison.OrdinalIgnoreCase));
        }
        
        public bool Contains(string id)
        {
            var first = enumData
                .FirstOrDefault(x => x.name.Equals(id,StringComparison.OrdinalIgnoreCase));
            return first != null;
        }
        
        public void Remove(string id)
        {
            if(string.IsNullOrEmpty(id)) return;
            enumData.RemoveAll(x => x.name.Equals(id,StringComparison.OrdinalIgnoreCase));
        }
        
        public void SaveConfig()
        {
            Save(false);
        }
    }

    [Serializable]
    public class EnumData
    {
        [ReadOnly]
        public string name = string.Empty;
        
        [OnCollectionChanged(nameof(AfterValuesChanged))]
        [DisableIf(nameof(isReadOnly))]
        public EnumValue[] values = Array.Empty<EnumValue>();
        
        [DisableIf(nameof(isReadOnly))]
        public bool isFlags = false;
        
        [DisableIf(nameof(isReadOnly))]
        [OnValueChanged(nameof(IsStrictOrderChanged_Callback))]
        public bool isStrictlyOrdered = false;
        
        [DisableIf(nameof(isReadOnly))]
        public bool customPath = false;
        
        [DisableIf(nameof(isReadOnly))]
        [ShowIf(nameof(customPath))]
        public string path = "Assets/UniGame.Generated/Shared/Enums/";
        
        [DisableIf(nameof(isReadOnly))]
        public string @namespace = "Game.Shared.Generated";

        private bool _isReadOnly;
        
        [HideInInspector]
        public bool isReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                foreach (var v in values)
                {
                    v.isReadOnly = _isReadOnly;
                }
            }
        }

        private void AfterValuesChanged(CollectionChangeInfo info, object collection)
        {
            if (info.ChangeType != CollectionChangeType.Add || info.Index < 0) return;
            if (values == null || info.Index >= values.Length) return;

            var newValue = values[^1];
            ReinitializeNewValue(newValue, values.Length - 1);
        }

        private void IsStrictOrderChanged_Callback()
        {
            for (var i = 0; i < values.Length; i++)
            {
                values[i].index = i;
            }
            
            foreach (var t in values)
            {
                t.IsStrictOrderChanged_Callback(isStrictlyOrdered);
            }
        }
        
        private void ReinitializeNewValue(EnumValue value, int index)
        {
            if (!isStrictlyOrdered) return;
            
            value.value = index;
            value.index = index;
            value.isStrictlyOrdered = isStrictlyOrdered;
            value.isReadOnly = isReadOnly;
        }
    }

    [Serializable]
    public class EnumValue
    {
        [DisableIf(nameof(isReadOnly))]
        public string name;
        [DisableIf(nameof(isReadOnly))]
        [DisableIf(nameof(isStrictlyOrdered))]
        public int value;
        
        [HideInInspector]
        public int index;

        [HideInInspector]
        public bool isStrictlyOrdered = false;

        [HideInInspector]
        public bool isReadOnly = false;
        
        public void IsStrictOrderChanged_Callback(bool strictlyOrdered)
        {
            isStrictlyOrdered = strictlyOrdered;
            if (isStrictlyOrdered)
            {
                value = index;
            }
        }
    }
}