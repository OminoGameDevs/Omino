﻿using UnityEngine;

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
            var instance = Instantiate(material);
            instance.hideFlags = HideFlags.DontSave;
            renderer.sharedMaterial = instance;
            renderer.sharedMaterial.name = "*" + material.name;
        }
    }
}
