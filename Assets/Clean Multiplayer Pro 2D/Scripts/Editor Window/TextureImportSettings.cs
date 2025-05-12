#if UNITY_EDITOR
using UnityEditor;

namespace AvocadoShark
{
    public class TextureImporterSettings : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            string folderPath = "Assets/SunnyLand Artwork";

            if (assetPath.StartsWith(folderPath))
            {
                TextureImporter importer = (TextureImporter)assetImporter;
                importer.spritePixelsPerUnit = 16;
            }
        }
    }
}
#endif