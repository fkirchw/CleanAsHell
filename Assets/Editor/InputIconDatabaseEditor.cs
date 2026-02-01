using System.Linq;
using UI;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(InputIconDatabase))]
    public class InputIconDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        
            InputIconDatabase database = (InputIconDatabase)target;
        
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sprite Population Tools", EditorStyles.boldLabel);
        
            if (GUILayout.Button("Auto-Populate All Sprite Arrays"))
            {
                PopulateSprites(database.keyboardMouseSheet, "Keyboard/Mouse");
                PopulateSprites(database.xboxSheet, "Xbox");
                PopulateSprites(database.playstationSheet, "PlayStation");
                PopulateSprites(database.steamDeckSheet, "Steam Deck");
            
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                Debug.Log("All sprite arrays populated successfully!");
            }
        }
    
        private void PopulateSprites(InputIconDatabase.SpriteSheetReference sheetRef, string sheetName)
        {
            if (sheetRef == null || sheetRef.spriteSheet == null)
            {
                Debug.LogWarning($"No texture assigned for {sheetName} sheet");
                return;
            }
        
            string assetPath = AssetDatabase.GetAssetPath(sheetRef.spriteSheet);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        
            sheetRef.sprites = assets.OfType<Sprite>().ToArray();
        
            Debug.Log($"Populated {sheetRef.sprites.Length} sprites for {sheetName} sheet");
        }
    }
}