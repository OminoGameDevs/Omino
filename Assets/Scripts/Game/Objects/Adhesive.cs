using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adhesive : MonoBehaviour
{
    private void OnOminoEnter(Omino.CubeStack stack)
    {
        if (Omino.instance.GetAdjacentCubes(stack.bottom.transform).Length == 1)
            Omino.instance.Detach(stack.bottom.transform);
    }
}
