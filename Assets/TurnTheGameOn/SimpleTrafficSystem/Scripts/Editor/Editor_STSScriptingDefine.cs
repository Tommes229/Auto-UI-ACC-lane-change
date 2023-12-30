using System.Collections.Generic;
using System.Linq;
using UnityEditor;

/// <summary>
/// Adds the given define symbols to PlayerSettings define symbols.
/// Just add your own define symbols to the Symbols property at the below.
/// </summary>
[InitializeOnLoad]
public class Editor_STSScriptingDefine : Editor
{

    /// <summary>
    /// Symbols that will be added to the editor
    /// </summary>
    public static readonly string[] Symbols = new string[] {
        "SimpleTrafficSystem",
    };

    /// <summary>
    /// Add define symbols as soon as Unity gets done compiling.
    /// </summary>
    static Editor_STSScriptingDefine()
    {
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();
        allDefines.AddRange(Symbols.Except(allDefines));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", allDefines.ToArray()));
    }

}
