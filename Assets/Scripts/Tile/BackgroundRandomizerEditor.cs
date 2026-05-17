#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BackgroundTilemapRandomizer))]
public class BackgroundRandomizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("── Editor Araçları ──", EditorStyles.boldLabel);

        BackgroundTilemapRandomizer randomizer = (BackgroundTilemapRandomizer)target;

        // Ana randomize butonu
        GUI.backgroundColor = new Color(0.4f, 0.85f, 0.4f);
        if (GUILayout.Button("▶  Tümünü Randomize Et  (BG + Columnlar)", GUILayout.Height(36)))
        {
            Undo.RecordObject(randomizer, "Randomize Background + Columns");
            randomizer.Randomize();
            EditorUtility.SetDirty(randomizer);
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.HelpBox(
            "Edit Mode'da çalışır — oynatmadan sonucu görürsün.\n" +
            "Beğenmezsen tekrar bas, her seferinde farklı yerleşim üretir.",
            MessageType.Info);
    }
}
#endif