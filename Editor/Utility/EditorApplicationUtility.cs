using System.Text;
using UnityEditor;

namespace EUTK
{
    public class EditorApplicationUtility
    {
        public static void AddMacroToPlayerSettings(string macro)
        {
            var symbolText = PlayerSettings.GetScriptingDefineSymbolsForGroup(GetCurrentBuildTargetGroup());
            var symbols = symbolText.Split(';');
            foreach (var symbol in symbols)
            {
                if (macro == symbol)
                    return;
            }
            symbolText += ";" + macro;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(GetCurrentBuildTargetGroup(), symbolText);
        }

        public static void RemoveMacroFromPlayerSettings(string macro)
        {
            var symbolText = PlayerSettings.GetScriptingDefineSymbolsForGroup(GetCurrentBuildTargetGroup());
            var symbols = symbolText.Split(';');
            macro = macro.Trim();
            for (int i = 0; i < symbols.Length; i++)
            {
                var symbol = symbols[i].Trim();
                if (symbol == macro)
                {
                    symbols[i] = null;
                }
            }

            var sb = new StringBuilder();
            for (int i = 0; i < symbols.Length; i++)
            {
                var symbol = symbols[i];
                if (symbol != null)
                {
                    sb.Append(symbol + ";");
                }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(GetCurrentBuildTargetGroup(), sb.ToString());
        }

        public static BuildTargetGroup GetCurrentBuildTargetGroup()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneOSX:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;
            }
            return BuildTargetGroup.Unknown;
        }
    }
}
