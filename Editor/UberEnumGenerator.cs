﻿namespace Game.Editor.UberEnums
{
    using System.Text;
    using UnityCodeGen;
    using UnityEditor;

    [Generator]
    public class UberEnumGenerator : ICodeGenerator
    {
        public void Execute(GeneratorContext context)
        {
            var config = ScriptableSingleton<UberEnumConfig>.instance;
            var data = config[config.currentIndex];
            var name = data.name.Replace(" ", string.Empty);
            
            context.OverrideFolderPath(data.path);
            var sb = new StringBuilder();
            sb.AppendLine(@"// <auto-generated/>");
            sb.AppendLine($"namespace {data.@namespace}");
            sb.AppendLine("{");
            sb.AppendLine("\tusing System;");
            if (data.isFlags)
            {
                sb.AppendLine($"\t[Flags]");
            }
            
            sb.AppendLine($"\tpublic enum {name}");
            sb.AppendLine("\t{");
            for (int i = 0; i < data.values.Length; i++)
            {
                var entry = data.values[i];
                var value = data.isFlags ? $"1 << {entry.value}" : entry.value.ToString();
                sb.AppendLine($"\t\t{entry.name.Replace(" ", string.Empty)} = {value},");
            }
            
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            context.AddCode($"{name}.cs", sb.ToString());
        }
    }
}