using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ModelProcessor : AssetPostprocessor
{
    private string assetDir {
        get {
            var pathTokens = assetPath.Split('/');
            string path = "";
            for (int i = 0; i < pathTokens.Length - 1; ++i)
                path += pathTokens[i] + "/";
            return path;
        }
    }

    private string assetName {
        get {
            var pathTokens = assetPath.Split('/');
            return pathTokens[pathTokens.Length - 1];
        }
    }

    private void OnPreprocessModel()
    {
        var modelImporter = assetImporter as ModelImporter;

        modelImporter.importAnimation = false;
        modelImporter.animationType = ModelImporterAnimationType.None;
        modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;

        modelImporter.importBlendShapes = false;
        modelImporter.importVisibility = false;
        modelImporter.importCameras = false;
        modelImporter.importLights = false;
        modelImporter.generateAnimations = ModelImporterGenerateAnimations.None;
        modelImporter.addCollider = false;
    }
}
