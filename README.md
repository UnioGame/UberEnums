# Uber Enums

Unity Enum generation tool

- [Uber Enums](#uber-enums)
- [How to Install](#how-to-install)
- [Overview](#overview)
  - [Editor View](#editor-view)
  - [Generate Enum from Code](#generate-enum-from-code)

# How to Install

Add to your project manifiest by path [%UnityProject%]/Packages/manifiest.json new Scope:

```json
{
  "dependencies": {
    "com.unigame.uberenum" : "https://github.com/UnioGame/UberEnums.git",
    "com.annulusgames.unity-codegen": "https://github.com/UnioGame/UnityCodeGen.git?path=/Assets/UnityCodeGen"
  }
}
```

# Overview

## Editor View

![](https://github.com/UnioGame/UberEnums/blob/main/GitAssets/uberenum1.png)

![](https://github.com/UnioGame/UberEnums/blob/main/GitAssets/uberenum2.png)

Generated Enum

![](https://github.com/UnioGame/UberEnums/blob/main/GitAssets/uberenum3.png)

## Generate Enum from Code

```cs

UberEnumApi.CreateAndGenerateEnum(
    $"{nameof(CharacterInfo)}Id", map.value, overwrite: true, 
                isStrictlyOrdered: true, isFlags: false, isReadOnly: true);

```
