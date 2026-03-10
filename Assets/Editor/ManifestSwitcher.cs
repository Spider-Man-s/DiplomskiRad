using System.IO;
using UnityEditor;

public static class ManifestSwitcher
{
    private const string Target = "Assets/Plugins/Android/AndroidManifest.xml";
    private const string MetaSource = "Assets/Plugins/MetaManifest/AndroidManifest.xml";

    [MenuItem("Build/Use Meta Manifest")]
    public static void UseMetaManifest()
    {
        Directory.CreateDirectory("Assets/Plugins/Android");
        File.Copy(MetaSource, Target, true);
        AssetDatabase.Refresh();
    }

    [MenuItem("Build/Use No Manifest (Xreal)")]
    public static void UseNoManifest()
    {
        if (File.Exists(Target))
            File.Delete(Target);

        if (File.Exists(Target + ".meta"))
            File.Delete(Target + ".meta");

        AssetDatabase.Refresh();
    }
}