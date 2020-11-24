using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OminoCreature : MonoBehaviour
{
    private GameObject _omino;
    private Vector3 dir;
    // Start is called before the first frame update
    void Start()
    {
        _omino= GameObject.Find("OminoGlass");
        dir = _omino.GetComponent<Omino>().lastDir;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Quaternion.identity.eulerAngles);
        this.transform.rotation = Quaternion.identity;
    }
}
