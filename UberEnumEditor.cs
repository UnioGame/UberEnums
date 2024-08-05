namespace Game.Editor.UberEnums
{
    using System;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    public class UberEnumEditor : OdinMenuEditorWindow
    {
        private CreateNewEnum _createEnum;
        private UberEnumConfig _config;
        
        [MenuItem("Window/Uber Enum Editor")]
        public static void ShowExample()
        {
            GetWindow<UberEnumEditor>().Show();
        }
        
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            _createEnum = new CreateNewEnum(this);
            
            tree.Add("Constructor", _createEnum);
            
            _config = ScriptableSingleton<UberEnumConfig>.instance;
            _config = _config == null
                ? CreateInstance<UberEnumConfig>()
                : _config;
            
            foreach (var e in _config.enumData)
                tree.Add(e.name, e);
            
            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            var selected = MenuTree.Selection;

            if (selected == null)
            {
                return;
            }
            
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUILayout.FlexibleSpace();
                if (SirenixEditorGUI.ToolbarButton("Delete"))
                {
                    var data = selected.SelectedValue as EnumData;
                    _config.Remove(data?.name);
                    _config.SaveConfig();
                    ForceMenuTreeRebuild();
                }

                if (SirenixEditorGUI.ToolbarButton("Generate"))
                {
                    var data = selected.SelectedValue as EnumData;
                    UberEnumApi.Generate(data);
                }
            }
            
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }

    internal class CreateNewEnum
    {
        [Serializable]
        internal class NewEnumData
        {
            public string Name;
            public EnumValue[] Values;
        }
        
        private readonly OdinMenuEditorWindow _parent;
        
        public NewEnumData EnumData;
        
        public CreateNewEnum(OdinMenuEditorWindow parent)
        {
            _parent = parent;
            EnumData = new NewEnumData();
            EnumData.Name = "New Enum";
        }

        [Button("Create Enum")]
        public void CreateEnum()
        {
            var data = new EnumData
            {
                name = EnumData.Name,
                values = EnumData.Values
            };

            if (UberEnumApi.CreateEnum(data.name, data.values) == null)
            {
                return;
            }
            
            EnumData = new NewEnumData();
            EnumData.Name = "NewEnum";
            
            _parent.ForceMenuTreeRebuild();
        }
    }
}