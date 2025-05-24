using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
