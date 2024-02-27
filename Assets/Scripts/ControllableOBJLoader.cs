using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllableOBJLoader : IModelLoader
{
    [SerializeField] private Material material;
    public override GameObject Load(string path)
    {
        GameObject loading = new Dummiesman.OBJLoader().Load(path);
        GameObject loadingChild = loading.transform.GetChild(0).gameObject;
        MeshFilter mf = loadingChild.GetComponent<MeshFilter>();
        MeshCollider mc = loadingChild.AddComponent<MeshCollider>();
        mc.convex = true;
        mc.sharedMesh = mf.mesh;
        Rigidbody rb = loadingChild.AddComponent<Rigidbody>();
        MeshRenderer mr = loadingChild.GetComponent<MeshRenderer>();
        mr.material = material;
        GameObject loaded = Instantiate(loadingChild, Vector3.zero, Quaternion.identity);
        DestroyImmediate(loading);
        return loaded;
    }
}
