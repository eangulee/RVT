using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceTerrainMat : MonoBehaviour
{
    public Material vtMat;

    void Awake()
    {
        var ter = this.GetComponent<Terrain>();
        if(ter != null)
        {
            ter.materialTemplate = vtMat;
        }
    }
}
