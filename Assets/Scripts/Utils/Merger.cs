using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
	using UnityEditor;
#endif

public static class Merger
{
	
	public enum MergeType
	{
		Destroy,
		Inactivate,
		Hide
	}
	
    public static void Merge(this GameObject obj, MergeType type = MergeType.Inactivate)
	{
		if (obj.transform.childCount == 0)
		{
			Debug.LogError("Nothing to merge!");
			return;
		}
			
		foreach (Transform child in obj.transform)
		{
			MeshFilter temp = child.GetComponent<MeshFilter>();
			if (!temp)
			{
				Debug.LogError("One or more children lacks a mesh filter! Unable to merge!");
				return;
			}
		}
		
		#if UNITY_EDITOR
			string filePath = "";
			if (Application.isEditor && !Application.isPlaying)
			{
				filePath = EditorUtility.SaveFilePanelInProject
				(
					"Save Mesh in Assets",
					"Mesh" + ".asset", 
					"asset","Please enter a file name to save the Mesh to "
				);
			
				if (filePath.Equals(""))
					return;
			}
		#endif
		
		Vector3 pos = obj.transform.position;
		obj.transform.position = Vector3.zero;
		
		Quaternion rot = obj.transform.rotation;
		obj.transform.rotation = Quaternion.identity;
		
        CombineInstance[] combine = new CombineInstance[obj.transform.childCount];

        for (int i = 0; i < obj.transform.childCount; ++i)
		{
			MeshFilter temp = obj.transform.GetChild(i).GetComponent<MeshFilter>();
            combine[i].mesh = temp.sharedMesh;
            combine[i].transform = temp.transform.localToWorldMatrix;
			
			switch (type)
			{
				case MergeType.Destroy:
					GameObject.Destroy(temp.gameObject); break;
				case MergeType.Inactivate:
					temp.gameObject.SetActive(false); break;
				case MergeType.Hide:
					temp.GetComponent<MeshRenderer>().enabled = false; break;
			}
        }
		
		Mesh newMesh = new Mesh();
		newMesh.name = obj.name;
		newMesh.CombineMeshes(combine);
		
		MeshFilter mf = obj.GetComponent<MeshFilter>();
		if (!mf)
			mf = obj.AddComponent<MeshFilter>();
        mf.sharedMesh = newMesh;
		
		if (type != MergeType.Hide && obj.GetComponent<MeshCollider>())
			obj.GetComponent<MeshCollider>().sharedMesh = newMesh;
		
		MeshRenderer mr = obj.GetComponent<MeshRenderer>();
		if (!mr)
			mr = obj.AddComponent<MeshRenderer>();
		mr.sharedMaterial = obj.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
		
		obj.transform.position = pos;
		obj.transform.rotation = rot;
		
		#if UNITY_EDITOR
			if (Application.isEditor && !Application.isPlaying)
			{
				AssetDatabase.CreateAsset(newMesh, filePath);
				AssetDatabase.SaveAssets();
			}
		#endif
    }
}