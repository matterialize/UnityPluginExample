using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Linq;

public class BuildPostProcessor
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            // Get project into C#
            var projectPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);

            // Get targetGUID
            var targetName = project.GetUnityFrameworkTargetGuid();
            var targetGUID = project.TargetGuidByName(targetName);

            // Built in Frameworks
            project.AddFrameworkToProject(targetGUID, "Foundation.framework", false);

            Debug.Log("Added iOS frameworks to project");

            // Add Shell Script to copy folders and files after running successfully
            var shellScriptName = "Copy edited Objective C folders and files back to Unity";
            var shellPath = "/bin/sh";
            var shellScript = $"cp -r \"$PROJECT_DIR/Libraries/Plugins/iOS\" {Directory.GetCurrentDirectory()}/Assets/Plugins";

            var allBuildPhasesGUIDS = project.GetAllBuildPhasesForTarget(targetGUID);

            var foundShellScript = allBuildPhasesGUIDS.Where(buildPhasesGUID => project.GetBuildPhaseName(buildPhasesGUID) == shellScriptName).FirstOrDefault();
            if (foundShellScript == null)
            {
                project.AddShellScriptBuildPhase(targetGUID, shellScriptName, shellPath, shellScript);
                Debug.Log($"Added custom shell script: {shellScriptName}");
            }

            // Add `-ObjC` to "Other Linker Flags".
            project.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC");

            // Overwrite
            project.WriteToFile(projectPath);
        }
    }
}
