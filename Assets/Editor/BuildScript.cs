using UnityEditor;
using System.IO;
using System.Linq;

public class BuildScript
{
    [MenuItem("Build/BuildWindows")]
    public static void BuildWindows()
    {
        string buildPath = "Builds/Windows";
        if (!Directory.Exists(buildPath))
            Directory.CreateDirectory(buildPath);

        // 🔹 Toma automáticamente TODAS las escenas que están marcadas en Build Settings
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = Path.Combine(buildPath, "MiJuego.exe");
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64; // Fuerza 64 bits
        buildPlayerOptions.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(buildPlayerOptions);
        UnityEngine.Debug.Log("Build completado en: " + Path.GetFullPath(buildPath));
    }
}