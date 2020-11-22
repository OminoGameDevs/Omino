using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using Pixelplacement.TweenSystem;

public class Omino : MonoBehaviour
{
    private const float rollTime = 0.2f;
    private const float cooldown = 0.05f;
    private const float gravity = 0.05f;

    public static Omino instance { get; private set; }
	
//----------------------------------------------------------------------------------------------------------------------------------------
	
	public bool rolling { get; private set; }
    public bool dropping { get; private set; }

    public Vector3 center {
		get {
            if (!cubes || cubes.childCount == 0)
                return transform.position;

			Vector3 result = Vector3.zero;
			foreach (Transform cube in cubes)
				result += cube.position;
			return result / cubes.childCount;
		}
	}
	
//----------------------------------------------------------------------------------------------------------------------------------------
	
	private new Camera camera;
	private Transform cubes;
	
	private bool touching;
	private Vector3 touchPos;
	private Vector3 swipeDir;
	
	private Vector3 lastDir;
	private Vector3 lastPos;
	
	private bool rejected;

	//private Sticker _sticker;
	
	private int holed;
	private HashSet<GameObject> enteredHoles = new HashSet<GameObject>();

    private TweenBase dropTween;
    private TweenBase rollTween;

//----------------------------------------------------------------------------------------------------------------------------------------

    /*
	private CubeColor GetColor(Transform cube, Vector3 direction)
	{
		if (!cube.CompareTag("ColorCube"))
			return CubeColor.None;
		
		if (direction == cube.right) 	return CubeColor.Red;
		if (direction == cube.forward)  return CubeColor.Blue;
		if (direction == cube.up) 		return CubeColor.Green;
		if (direction == -cube.right) 	return CubeColor.Cyan;
		if (direction == -cube.forward) return CubeColor.Yellow;
		if (direction == -cube.up) 		return CubeColor.Magenta;
		
		return CubeColor.None;
	}
	*/



    private void Awake()
	{
		camera = Game.instance.transform.Find("Camera").GetComponent<Camera>();
		cubes = transform.Find("Cubes");
		
		foreach (Transform cube in cubes)
			cube.gameObject.layer = 0;
		
		//cubes.gameObject.Merge(Merger.MergeType.Hide);
	}



    private void Update()
	{
		if (!instance)
			instance = this;
		
		if (Input.GetMouseButton(0))
		{
			foreach (RaycastHit hit in Physics.RaycastAll(camera.ScreenPointToRay(Input.mousePosition)))
			{
				if (hit.collider.CompareTag("TouchSurface"))
				{
					if (!touching)
					{
						touching = true;
						touchPos = hit.point;
					}
					else
					{
						Vector3 delta = hit.point - touchPos;
						if (delta.magnitude > 1f)
						{
							touchPos = hit.point;
							swipeDir = delta.normalized;
						}
					}
				}
			}
		}
		else
		{
			touching = false;
			swipeDir = Vector3.zero;
		}
		
		Vector3 dir = Vector3.zero;
		
		if (swipeDir != Vector3.zero)
			dir = swipeDir.Orthogonalize();
		else if (Input.GetKey(KeyCode.RightArrow))
			dir = Vector3.right;
		else if (Input.GetKey(KeyCode.LeftArrow))
			dir = Vector3.left;
		else if (Input.GetKey(KeyCode.DownArrow))
			dir = Vector3.back;
		else if (Input.GetKey(KeyCode.UpArrow))
			dir = Vector3.forward;
		
		if (lastDir != dir)
		{
			lastDir = dir;
			rejected = false;
		}
		
		if (!rolling && !dropping)
		{
			if (dir != Vector3.zero)
			{
				if (!rejected)
					Roll(dir);
			}
			else
				rejected = false;
		}
	}



    public Transform GetClosestCube(Vector3 position)
    {
        float minSqDist = float.PositiveInfinity;
        Transform closest = null;
        foreach (Transform cube in cubes)
        {
            float sqDist = (position - cube.position).sqrMagnitude;
            if (sqDist < minSqDist)
            {
                minSqDist = sqDist;
                closest = cube;
            }
        }
        return closest;
    }



    private void Roll(Vector3 dir)
	{
		Vector3 hPos = dir * -9000f;
		Quaternion rot = Quaternion.Inverse(Quaternion.LookRotation(dir));
		
		// Get appropriate center point
		foreach (Transform cube in cubes)
		{
			for (int i = 0; i < 2; ++i)
			{
				foreach (RaycastHit hit in Physics.RaycastAll(cube.position + dir*i, Vector3.down, 0.55f))
				{
					if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
					{
						Vector3 temp = cube.position + Vector3.down * 0.5f + dir * 0.5f;
						
						Vector3 nTemp = (rot * temp).Round(1);
						Vector3 nhPos = (rot * hPos).Round(1);
						
						if (nTemp.z > nhPos.z)
							hPos = temp;
						else if (nTemp.z == nhPos.z && nTemp.y > nhPos.y)
							hPos = temp;
					}
				}
			}
		}
		
		// Center cubes on center point
		cubes.parent = transform.parent;
		transform.rotation = Quaternion.identity;
		transform.position = hPos;
		cubes.parent = transform;
		
		// Roll about center point
		rolling = true;
		Vector3 euler = new Vector3(dir.z, 0f, -dir.x);
		//transform.eulerAngles = euler * 10f;

        rollTween = Tween.Rotate(
            target:           transform,
            space:            Space.World,
            amount:           euler * 90f,
            duration:         rollTime,
            delay:            0f,
            easeCurve:        Tween.EaseIn,
            completeCallback: OnRollSucceed
        );
	}
	
	
	
	private void Detect()
	{
		// Snap
		transform.position = transform.position.Round(1);
		
		if (transform.rotation != Quaternion.identity)
		{
			Vector3 temp = transform.eulerAngles.normalized * 180f;
			Vector3 temp2 = transform.eulerAngles - temp;
			temp2 *= (90f/temp2.magnitude);
			transform.eulerAngles = temp2 + temp;
		}
		
		// Find and assimilate cubes, if none found, find ground, if no ground, drop
		if (DetectNewCubes() || DetectGround())
		{
			dropping = false;
			holed = 0;
			enteredHoles.Clear();
			Blip();
		}
		else
		{
            dropTween = Tween.Position(
                target:           transform,
                endValue:         Vector3.Scale(transform.position, new Vector3(1,0,1)) + Vector3.up * (transform.position.y - 1f),
                duration:         dropping ? gravity : gravity * 1.04577f,
                delay:            0f,
                easeCurve:        dropping ? Tween.EaseLinear : Tween.EaseIn,
                completeCallback: Detect
            );
			dropping = true;
		}
	}
	
	
	
	// Find and assimilate cubes
	private bool DetectNewCubes()
	{
		bool found = false;
		bool repeat = false;
		do
		{
			repeat = false;
			foreach (Transform cube in cubes)
			{
				for (int i = 0; i < 6; ++i)
				{
					Vector3 dir = Vector3.zero;
					switch (i)
					{
						case 0: dir = Vector3.forward;	break;
						case 1: dir = Vector3.back;		break;
						case 2: dir = Vector3.right; 	break;
						case 3: dir = Vector3.left; 	break;
						case 4: dir = Vector3.down; 	break;
						case 5: dir = Vector3.up; 		break;
					}
					foreach (RaycastHit hit in Physics.RaycastAll(cube.position, dir, 1f))
					{
						// Add other cubes to this shape
						Transform obj = hit.collider.transform;
						if (obj.gameObject.layer == LayerMask.NameToLayer("Obstacle") && (obj.CompareTag("Cube")))
						{

							obj.gameObject.layer = 0;
							obj.SetParent(cubes);
                            obj.GetComponent<Renderer>().sharedMaterial = ResourceLoader.Get<Material>("Cube");
							
							found = true;
							repeat = true;
						}
					}
				}
			}
		}
		while (repeat);
		
		//if (found)
		//	cubes.gameObject.Merge(Merger.MergeType.Hide);
		
		return found;
	}
	
	
	
	private bool DetectGround()
	{
		foreach (Transform cube in cubes)
			foreach (RaycastHit hit in Physics.RaycastAll(cube.position, Vector3.down, 1f))
				if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
					return true;
		return false;
	}
	
	
	
	private bool IsValid()
	{
		// Omino must have ground under it
		foreach (Transform cube in cubes)
			foreach (RaycastHit hit in Physics.RaycastAll(cube.position, Vector3.down))
				if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle") ||
					hit.collider.CompareTag("Hole"))
					return true;

		return false;
	}



    private void Reject()
	{
        // Roll back
        float t = rollTween.Percentage;
        rollTween.Stop();
        Tween.Rotation(
            target:           transform,
            endValue:         Vector3.zero,
            duration:         rollTime * t,
            delay:            0f,
            easeCurve:        Tween.EaseLinear,
            completeCallback: OnRollFail
        );
		
		rejected = true;
		dropping = false;

		Buzz();
	}
		
	
		
	private void OnRollSucceed()
	{
		if (IsValid())
		{
			Detect();
			lastPos = transform.position;
			Invoke("EndRoll", cooldown);
		}
		else
			Reject();
	}

    private void OnRollFail()
    {
		Invoke("EndRoll", cooldown);
    }



	private void EndRoll()
	{
		rolling = false;
		foreach (Transform cube in cubes)
		{
			foreach (RaycastHit hit in Physics.RaycastAll(cube.position, Vector3.down, 1f))
			{
				if (hit.transform.CompareTag("World") || hit.transform.parent == cubes)
					continue;

				// Find out if a sticker should be used
				//RaycastHit hit2;
				//if (Physics.Raycast(hit.transform.position, hit.transform.up, out hit2, 1f))
				//{
				//	Sticker sticker = hit2.collider.GetComponent<Sticker>();
				//	if (sticker)
				//	{
				//		hit.transform.SendMessage("OnOminoEnteredWithSticker", sticker, SendMessageOptions.DontRequireReceiver);
				//		return;
				//	}
				//}
				hit.transform.SendMessage("OnOminoEntered", this, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	
	
	private void OnSpitEnd()
	{
		Reject();
	}
	
	
	
	private void Win()
	{
		Tween.StopAll();
		
		//cubes.GetComponent<Renderer>().enabled = false;
		//foreach (Transform cube in cubes)
		//{
		//	cube.GetComponent<ParticleSystem>().Play();
  //          Tween.Position(
  //              target:    cube,
  //              endValue:  Vector3.Scale(cube.position, new Vector3(1, 0, 1)) + Vector3.up * (Camera.main.transform.parent.GetComponent<CameraTarget>().startY + 10f),
  //              duration:  Game.fadeOutTime * 1.5f,
  //              delay:     0f,
  //              easeCurve: Tween.EaseIn
  //          );
		//}
		
		//Game.instance.Invoke("Win", Game.fadeOutTime);
		//Camera.main.transform.parent.SendMessage("FadeOut");
		Game.instance.Win();
	}
	
	
	
	private void OnTriggerEnter(Collider other)
	{
		// Obstacles
		if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
		{
			Reject();
			return;
		}
		
		// Holes
		else if (other.CompareTag("Hole"))
		{
            float lowestY = 999f;
            Transform lowestCube = null;
            foreach (Transform cube in cubes)
            {
                if (cube.position.y < lowestY)
                {
                    lowestY = cube.position.y;
                    lowestCube = cube;
                }
            }
            if (lowestCube)
                Destroy(lowestCube.gameObject);
            if (cubes.childCount == 1)
                Win();
			//++holed;
			//enteredHoles.Add(other.gameObject);
			//if (holed == cubes.childCount)
			//{
				//int totalHoles = other.transform.parent.childCount;
				//if (enteredHoles.Count == totalHoles)
					//Win();
				//else
				//{
    //                if (dropTween != null) dropTween.Stop();
    //                Tween.Position(
    //                    target:           transform,
    //                    endValue:         lastPos,
    //                    duration:         gravity * 0.5f,
    //                    delay:            0f,
    //                    easeCurve:        Tween.EaseOut,
    //                    completeCallback: OnSpitEnd
    //                );
				//}
				
				//holed = 0;
				//enteredHoles.Clear();
				//return;
			//}
		}

		if (other.CompareTag("World"))
			return;

		// Others
		// Find out if a sticker should be used
		//RaycastHit hit;
		//if (Physics.Raycast(other.transform.position, other.transform.up, out hit, 1f))
		//{
		//	Sticker sticker = hit.collider.GetComponent<Sticker>();
		//	if (sticker)
		//	{
		//		other.SendMessage("OnOminoEnterWithSticker", sticker, SendMessageOptions.DontRequireReceiver);
		//		return;
		//	}
		//}
		other.SendMessage("OnOminoEnter", GetClosestCube(other.transform.position), SendMessageOptions.DontRequireReceiver);
	}



    private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("World"))
			return;

		// Find out if a sticker should be used
		//RaycastHit hit;
		//if (Physics.Raycast(other.transform.position, other.transform.up, out hit, 1f))
		//{
		//	Sticker sticker = hit.collider.GetComponent<Sticker>();
		//	if (sticker)
		//	{
		//		other.SendMessage("OnOminoExitWithSticker", sticker, SendMessageOptions.DontRequireReceiver);
		//		return;
		//	}
		//}
		other.SendMessage("OnOminoExit", GetClosestCube(other.transform.position), SendMessageOptions.DontRequireReceiver);
	}



	private void Blip()
	{
		//audio.Stop();
		//GetComponent<AudioSource>().clip = AudioUtility.Sine(Random.Range(220f, 440f), 0.002f, 0f, 0.0015f, 0.0005f);
		//GetComponent<AudioSource>().Play();
	}

	private void Buzz()
	{
		//audio.Stop();
		//GetComponent<AudioSource>().clip = AudioUtility.Square(440f, 0.002f, 0f, 0.0015f, 0.0005f);
		//GetComponent<AudioSource>().Play();
	}
	
}