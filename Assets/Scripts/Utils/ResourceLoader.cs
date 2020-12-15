using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader
{
    public static string Path<T>(bool full = false)
    {
        string root = (full ? "Assets/Resources/" : "");

        if (typeof(T) == typeof(AudioClip))      return root + "Audio/";
        if (typeof(T) == typeof(Font))           return root + "Fonts/";
        if (typeof(T) == typeof(Material))       return root + "Materials/";
        if (typeof(T) == typeof(Mesh))           return root + "Meshes/";
        if (typeof(T) == typeof(PhysicMaterial)) return root + "PhysMaterials/";
        if (typeof(T) == typeof(Shader))         return root + "Shaders/";
        if (typeof(T) == typeof(Texture2D))      return root + "Textures/";

        if (typeof(T) == typeof(Level))          return root + "Prefabs/Levels/";

        return root;
    }

    private static Dictionary<string, Object> cache;
    private static Dictionary<System.Type, Object[]> allCache;

    public static T Get<T>(string path) where T : Object
    {
        if (cache == null)
            cache = new Dictionary<string, Object>();

        path = Path<T>() + path;

        if (!cache.ContainsKey(path))
        {
            var asset = Resources.Load<T>(path);
            if (asset == null)
            {
                Debug.LogError("\"" + path + "\" not found.");
                return null;
            }
            cache.Add(path, asset);
        }

        return cache[path] as T;
    }

    public static T[] GetAll<T>() where T : Object => Resources.LoadAll<T>(Path<T>());
}