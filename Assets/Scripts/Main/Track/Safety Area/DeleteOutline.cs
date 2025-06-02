using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DeleteOutline : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(RemoveOutline), 0.05f);
    }

    private void RemoveOutline()
    {
        Renderer rd = GetComponent<Renderer>();
        Material[] materials = rd.sharedMaterials;
        int idx = -1;

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null && materials[i].name.StartsWith("Outline"))
            {
                idx = i;
                break;
            }
        }

        if (idx == -1)
            return;

        Material[] newMaterials = new Material[materials.Length - 2];
        for (int i = 0; i < newMaterials.Length; i++)
            newMaterials[i] = materials[i];
        rd.materials = newMaterials;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DeleteOutline))]
public class DeleteOutlineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("이 스크립트가 있는 오브젝트는 Outline이 그려지지 않습니다.", MessageType.Info);
    }
}
#endif