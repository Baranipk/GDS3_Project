#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BossHealthBarAutoUI))]
public class BossHealthBarAutoUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BossHealthBarAutoUI target = (BossHealthBarAutoUI)this.target;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Hızlı Aksiyonlar", EditorStyles.boldLabel);

        // Büyük "Rebuild" butonu
        GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
        if (GUILayout.Button("REBUILD (Frame + Fill + Layout)", GUILayout.Height(34)))
        {
            Undo.RegisterFullObjectHierarchyUndo(target.gameObject, "Rebuild Boss Health Bar");
            target.Rebuild();
            EditorUtility.SetDirty(target);
        }

        GUI.backgroundColor = new Color(0.7f, 0.85f, 1f);
        if (GUILayout.Button("Sadece Layout'u Uygula", GUILayout.Height(26)))
        {
            Undo.RegisterCompleteObjectUndo(target.GetComponent<RectTransform>(), "Apply Layout");
            target.ApplyLayout();
            EditorUtility.SetDirty(target);
        }

        GUI.backgroundColor = Color.white;
    }
}
#endif
