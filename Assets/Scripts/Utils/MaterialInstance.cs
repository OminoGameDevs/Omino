using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class MaterialInstance : MonoBehaviour
{
    public Material material => _material;
    [SerializeField] private Material _material;

    private void Awake()
    {
        if (Application.isPlaying)
            InstantiateMaterial();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (UnityEditor.SceneManagement.StageUtility.GetCurrentStageHandle().Contains(gameObject))
            InstantiateMaterial();
    }
#endif

    private void InstantiateMaterial()
    {
        var renderer = GetComponent<Renderer>();

        if (material)
        {
            renderer.sharedMaterial = Instantiate(material);
            renderer.sharedMaterial.name = "*" + material.name;
        }
    }
}
