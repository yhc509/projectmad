using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class PrefabPivotSettingTool
{
    [MenuItem("Tools/Set Prefab Pivot", false)]
    public static void SetPrefabPivot()
    {
        Transform transform = Selection.activeTransform;
        Vector3 pivot = transform.position;
        if (transform.parent == null)
        {
            Debug.LogError("Parent is null");
            return;
        }
        var root = PrefabUtility.GetOutermostPrefabInstanceRoot(transform.parent).transform;
        Vector3 origin = root.position;
        root.position = pivot;
        for (int i = 0; i < root.childCount; i++)
        {
            root.GetChild(i).position -= pivot - origin;
        }
        EditorUtility.SetDirty(root);
    }
}
