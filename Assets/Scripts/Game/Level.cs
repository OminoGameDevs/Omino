using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
#endif

[ExecuteInEditMode]
public class Level : MonoBehaviour
{
    public Transform levelObjects {
        get {
            if (!_levelObjects)
                _levelObjects = transform.Find("Objects");
            return _levelObjects;
        }
    }
    private Transform _levelObjects;
    public Transform world {
        get {
            if (!_world)
                _world = transform.Find("World");
            return _world;
        }
    }
    private Transform _world;
    public Transform editorObjects
    {
        get
        {
            if (!_editorObjects)
                _editorObjects = transform.Find("EditorObjects");
            return _editorObjects;
        }
    }
    private Transform _editorObjects;

    public Omino omino {
        get {
            if (!_omino)
                _omino = levelObjects.GetComponentInChildren<Omino>();
            return _omino;
        }
    }
    private Omino _omino;

    public T[] GetObjectsOfType<T>() => (from obj in objects where obj.GetComponent<T>() != null select obj.GetComponent<T>()).ToArray<T>();
    public GameObject[] objects {
        get {
            var result = new List<GameObject>();
            foreach (Transform t in levelObjects)
                result.Add(t.gameObject);
            return result.ToArray();
        }
    }

    private HashSet<Material> shadedMaterials = new HashSet<Material>();
    private Dictionary<int, IceGroup> iceGroups = new Dictionary<int, IceGroup>();

    private void Awake()
    {
        if (!Application.isPlaying) return;

        RefreshWorld();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying && !GetComponent<MeshCollider>())
        {
            var c = gameObject.AddComponent<MeshCollider>();
            c.sharedMesh = ResourceLoader.Get<Mesh>("BigPlane");
            c.hideFlags = HideFlags.HideAndDontSave;
        }

        RefreshWorld();
        RenderPipelineManager.beginCameraRendering += UpdateCameras;
    }

    private void OnDisable()
    {
        foreach (var kv in iceGroups)
        {
            DestroyImmediate(kv.Value.reflectionCamera.gameObject);
            DestroyImmediate(kv.Value.material);
        }
        iceGroups.Clear();

        RenderPipelineManager.beginCameraRendering -= UpdateCameras;
    }

    private void UpdateCameras(ScriptableRenderContext context, Camera mainCam)
    {
        if (mainCam.cameraType == CameraType.Reflection || mainCam.cameraType == CameraType.Preview) return;

        var iceMat = ResourceLoader.Get<Material>("Ice");
        var iceTiles = from obj in objects where obj.CompareTag("Ice") group obj by (int)obj.transform.position.y into g select g;
        foreach (var key in iceGroups.Keys.ToArray())
        {
            if (!iceTiles.Any(kv => kv.Key == key))
            {
                DestroyImmediate(iceGroups[key].reflectionCamera.gameObject);
                DestroyImmediate(iceGroups[key].material);
                iceGroups.Remove(key);
            }
        }
        foreach (var kv in iceTiles)
        {
            if (!iceGroups.ContainsKey(kv.Key))
            {
                var reflectionCam = new GameObject("IceReflectionCamera").AddComponent<Camera>();
                reflectionCam.cameraType = CameraType.Reflection;
                var reflectionTex = new RenderTexture(256, 256, 16);
                reflectionCam.targetTexture = reflectionTex;
                //reflectionCam.cullingMask = ~(1 << LayerMask.NameToLayer("Ice"));
                reflectionCam.gameObject.hideFlags = HideFlags.HideAndDontSave;

                var mat = Instantiate(iceMat);
                mat.name = iceMat.name + kv.Key;
                mat.SetTexture("_ReflectionTex", reflectionTex);

                iceGroups.Add(kv.Key, new IceGroup(mat, reflectionCam));
            }
            foreach (var obj in kv)
                obj.GetComponent<Renderer>().sharedMaterial = iceGroups[kv.Key].material;
        }

        Matrix4x4 getReflection(Vector3 p)
        {
            Vector3 n = Vector3.up;
            float d = -Vector3.Dot(n, p);

            return new Matrix4x4()
            {
                m00 = (1f  - 2f * n.x * n.x),
                m01 = (-2f      * n.x * n.y),
                m02 = (-2f      * n.x * n.z),
                m03 = (-2f      * d   * n.x),
                m10 = (-2f      * n.y * n.x),
                m11 = (1f  - 2f * n.y * n.y),
                m12 = (-2f      * n.y * n.z),
                m13 = (-2f      * d   * n.y),
                m20 = (-2f      * n.z * n.x),
                m21 = (-2f      * n.z * n.y),
                m22 = (1f - 2f  * n.z * n.z),
                m23 = (-2f      * d   * n.z),
                m30 = 0f,
                m31 = 0f,
                m32 = 0f,
                m33 = 1f,
            };
        }

        var mainCamPos = mainCam.transform.position;
        var mainCamEuler = mainCam.transform.eulerAngles;
        foreach (var kv in iceGroups)
        {
            float y = kv.Key - 0.498f;
            Camera reflectionCam = kv.Value.reflectionCamera;
            reflectionCam.transform.position = new Vector3(mainCamPos.x, 2*y - mainCamPos.y, mainCamPos.z);
            reflectionCam.transform.eulerAngles = new Vector3(-mainCamEuler.x, mainCamEuler.y, mainCamEuler.z);

            var reflection = getReflection(new Vector3(mainCamPos.x, y, mainCamPos.z));
            reflectionCam.worldToCameraMatrix = mainCam.worldToCameraMatrix * reflection;

            var p = reflectionCam.worldToCameraMatrix.MultiplyPoint(new Vector3(mainCamPos.x, y, mainCamPos.z));
            var n = reflectionCam.worldToCameraMatrix.MultiplyVector(Vector3.up).normalized;
            reflectionCam.projectionMatrix = mainCam.CalculateObliqueMatrix(new Vector4(n.x, n.y, n.z, -Vector3.Dot(p, n)));

            GL.invertCulling = true;
            UniversalRenderPipeline.RenderSingleCamera(context, reflectionCam);
            GL.invertCulling = false;
        }
    }

    private void RefreshWorld()
    {
        float minWallY = float.PositiveInfinity;
        float maxWallY = float.NegativeInfinity;
        foreach (Transform child in world)
        {
            child.localPosition = child.localPosition.Round();
            if (child.CompareTag("World"))
            {
                //child.gameObject.hideFlags = (editWorldDirectly ? HideFlags.None : HideFlags.HideInHierarchy);
                if (child.localPosition.y - 0.5f < minWallY) minWallY = child.localPosition.y - 0.5f;
                if (child.localPosition.y + 0.5f > maxWallY) maxWallY = child.localPosition.y + 0.5f;
                child.Reset(position: false);
            }
            else 
                child.SetParent(levelObjects);
        }

        /*
        foreach (Transform child in levelObjects)
        {
            if (child.CompareTag("EditObject"))
            {
                child.SetParent(transform);
            }
        }
        */
        Shader.SetGlobalFloat("_BottomY", minWallY);
        Shader.SetGlobalFloat("_TopY", maxWallY);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Application.isPlaying) return;

        transform.Reset();
        levelObjects.Reset();
        world.Reset();

        foreach (Transform child in transform)
            if (child != world && child != levelObjects && child != editorObjects)
            {
                if (child.CompareTag("EditObject"))
                    child.SetParent(editorObjects);
                else child.SetParent(child.CompareTag("World") ? world : levelObjects);
            }

        foreach (Transform child in levelObjects)
        {
            child.localPosition = child.localPosition.Round();
            child.localRotation = child.localRotation.Round();
            child.Reset(position: false, rotation: false);

            if (child.CompareTag("World"))
                child.SetParent(world);

            if (child.CompareTag("EditObject"))
                child.SetParent(transform);
        }
        RefreshWorld();
    }

#endif

    private struct IceGroup
    {
        public readonly Material material;
        public readonly Camera reflectionCamera;

        public IceGroup(Material material, Camera reflectionCamera)
        {
            this.material = material;
            this.reflectionCamera = reflectionCamera;
        }
    }
}
