using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Pixelplacement;
using Pixelplacement.TweenSystem;

public class Omino : MonoBehaviour
{
    public static Omino instance { get; private set; }

//----------------------------------------------------------------------------------------------------------------------------------------

	public bool rolling { get; private set; }
    public bool dropping { get; private set; }
    public bool sliding { get; private set; }
    public bool moving => rolling || dropping || sliding;
    public Vector3 push { get; private set; }
	public Vector3 slide { get; private set; }

	public Vector3 center {
		get {
            if (this && !cubes || cubes.childCount == 0)
                return transform.position;

            else if (!this)
                return default(Vector3);

			Vector3 result = Vector3.zero;
            foreach (Transform cube in cubes)
                result += cube.position;
            return result / cubes.childCount;
		}
	}

    public Vector3 direction { get; private set; } = Vector3.back;

    public Vector3 bottom { get; private set; }
    private float lastBottomY;

//----------------------------------------------------------------------------------------------------------------------------------------

	private new Camera camera;
    public Transform cubes { get; private set; }

	private bool touching;
	private Vector3 touchPos;
	private Vector3 swipeDir;

	public Vector3 lastDir;
	public Vector3 lastPos;

	private bool rejected;
    private bool dontEndMove;

    //private Sticker _sticker;

    private HashSet<GameObject> triggered = new HashSet<GameObject>();

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

        RecalculateBottom();
    }

    private void Update()
	{
		if (!instance)
			instance = this;

        RecalculateBottom();

        // Send trigger stay messages
        if (!moving)
            foreach (var obj in triggered)
                obj.SendMessage("OnOminoStay", GetCubeStackAt(obj.transform.position), SendMessageOptions.DontRequireReceiver);

        if (Input.GetMouseButton(0))
		{
			foreach (RaycastHit hit in Physics.RaycastAll(camera.ScreenPointToRay(Input.mousePosition)))
			{
				if (hit.collider.CompareTag("TouchSurface"))
				{
                    var hitpoint = hit.point + Vector3.Scale(Camera.main.transform.position, new Vector3(1,0,1));

                    if (!touching)
					{
						touching = true;
						touchPos = hitpoint;
					}
					else
					{
						Vector3 delta = hitpoint - touchPos;
						if (delta.magnitude > 1f)
						{
							touchPos = hitpoint;
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
		if (!moving)
		{
			if (dir != Vector3.zero)
			{
				if (!rejected)
                {
                    foreach (Transform cube in cubes)
                    {
                        var hits = Physics.RaycastAll(cube.position, -Vector3.up, 1f);
                        if (hits.Any(hit => IsObstacle(hit.collider.gameObject.layer)) && !hits.Any(hit => hit.collider.CompareTag("Ice")))
                        {
                            Roll(dir);
                            break;
                        }
                    }
                    if (!rolling)
                        Slide(dir);
                }
			}
			else
				rejected = false;
		}
	}

    private void RecalculateBottom()
    {
        float minDist = float.PositiveInfinity;
        Vector3? closestFloor = null;
        foreach (Transform cube in cubes)
        {
            foreach (var hit in Physics.SphereCastAll(cube.position, 0.25f, -Vector3.up))
            {
                if (IsObstacle(hit.collider.gameObject.layer) && hit.distance < minDist)
                {
                    minDist = hit.distance;
                    closestFloor = hit.point;
                }
            }
        }
        if (closestFloor.HasValue)
            lastBottomY = closestFloor.Value.y;
        bottom = new Vector3(center.x, lastBottomY, center.z);
    }

    public CubeStack GetCubeStackAt(Vector3 position)
    {
        var rest = new List<GameObject>();
        float minY = float.PositiveInfinity;
        GameObject bottom = null;
        foreach (Transform cube in cubes)
        {
            if (Mathf.Abs(position.x - cube.position.x) > 0.5f || Mathf.Abs(position.z - cube.position.z) > 0.5f)
                continue;

            rest.Add(cube.gameObject);

            if (cube.position.y < minY)
            {
                minY = cube.position.y;
                bottom = cube.gameObject;
            }
        }
        rest.Remove(bottom);
        return new CubeStack(bottom, rest.ToArray());
    }
 //   private void Slide(Vector3 dir)
	//{
	//	//Debug.Log("Slide");
	//	//lastPos = gameObject.transform.position;

	//	rolling = true;
	//	float amount = 0;
	//	foreach (Transform cube in cubes)
	//	{
	//		foreach (RaycastHit hit in Physics.RaycastAll(cube.position + dir, Vector3.down, 0.55f))
	//		{
	//			if (IsObstacle(hit.collider.gameObject.layer))
	//			{
	//				amount = 1;
	//			}
	//		}
	//	}

	//	Tween.Position(transform, gameObject.transform.position + dir * 1, 0.3f, 0.0f, completeCallback: OnRollSucceed);
	//}

	private void Roll(Vector3 dir)
	{
        direction = dir;

		Vector3 hPos = dir * -9000f;
		Quaternion rot = Quaternion.Inverse(Quaternion.LookRotation(dir));

		// Get appropriate center point
		foreach (Transform cube in cubes)
		{
			for (int i = 0; i < 2; ++i)
			{
				foreach (RaycastHit hit in Physics.RaycastAll(cube.position + dir*i, Vector3.down, 0.55f))
				{
					if (IsObstacle(hit.collider.gameObject.layer))
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
		cubes.SetParent(transform.parent);
		transform.rotation = Quaternion.identity;
		transform.position = hPos;
		cubes.SetParent(transform);

		// Roll about center point
		rolling = true;
		Vector3 euler = new Vector3(dir.z, 0f, -dir.x);
		//transform.eulerAngles = euler * 10f;

        rollTween = Tween.Rotate(
            target:           transform,
            space:            Space.World,
            amount:           euler * 90f,
            duration:         Constants.transitionTime,
            delay:            0f,
            easeCurve:        Tween.EaseIn,
            completeCallback: () =>
            {
                if (IsValid())
                {
                    Detect();
                    lastPos = transform.position;
                    EndMove();
                }
                else
                    Reject();
            }
        );
	}

    public bool Slide(Vector3 dir, AnimationCurve ease)
    {
        direction = dir;

        bool safe = false;
        foreach (Transform cube in cubes)
        {
            // Detect obstacles in front
            foreach (var hit in Physics.RaycastAll(cube.position, dir, 1f))
                if (IsObstacle(hit.collider.gameObject.layer))
                    return false;

            // Detect bottomless pits
            foreach (var hit in Physics.RaycastAll(cube.position + dir, Vector3.down))
            {
                if (IsObstacle(hit.collider.gameObject.layer) || hit.collider.CompareTag("Hole"))
                {
                    safe = true;
                    break;
                }
            }
        }
        if (!safe) return false;

        Tween.Stop(GetInstanceID());
        sliding = true;
        Tween.Position(
            target: transform,
            endValue: transform.position + dir,
            duration: Constants.transitionTime,
            delay: 0f,
            easeCurve: ease,
            completeCallback: () =>
            {
                Detect();
                lastPos = transform.position;
                EndMove();
            }
        );
        return true;
    }
    public bool Slide(Vector3 dir) => Slide(dir, Tween.EaseLinear);

    private void Drop()
    {
        dropTween = Tween.Position(
            target:           transform,
            endValue:         Vector3.Scale(transform.position, new Vector3(1,0,1)) + Vector3.up * (transform.position.y - 1f),
            duration:         dropping ? Constants.transitionTime * 0.25f : Constants.transitionTime * 0.25f * 1.04577f,
            delay:            0f,
            easeCurve:        dropping ? Tween.EaseLinear : Tween.EaseIn,
            completeCallback: Detect
        );
		dropping = true;
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
        var newCubes = DetectNewCubes();
        if (newCubes.Length > 0 || DetectGround())
        {
            dropping = false;
            holed = 0;
            enteredHoles.Clear();

            // Send trigger exit messages
            foreach (var obj in triggered)
                obj.SendMessage("OnOminoExit", GetCubeStackAt(obj.transform.position), SendMessageOptions.DontRequireReceiver);
            triggered.Clear();

            // Send trigger enter messages from old cubes
            foreach (Transform cube in cubes)
            {
                if (newCubes.Contains(cube)) continue;
                foreach (var collider in Physics.OverlapBox(cube.position, Vector3.one * 0.499f))
                {
                    if (collider.transform == cube || collider.transform.parent == cubes || collider.CompareTag("World"))
                        continue;

                    triggered.Add(collider.gameObject);
                    collider.transform.SendMessage("OnOminoEnter", GetCubeStackAt(cube.position - Vector3.down * 0.5f), SendMessageOptions.DontRequireReceiver);
                }
            }

            Blip();
            DetectPush();
            DetectSlide();
        }
        else Drop();
	}

	// Find and assimilate cubes
	private Transform[] DetectNewCubes()
	{
        var result = new List<Transform>();
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
						if (IsObstacle(obj.gameObject.layer) && (obj.CompareTag("Cube")))
						{
							obj.gameObject.layer = 0;
							obj.SetParent(cubes);
                            //obj.GetComponent<Renderer>().sharedMaterial = ResourceLoader.Get<Material>("Cube");

                            result.Add(obj);
							repeat = true;
						}
					}
				}
			}
		}
		while (repeat);

		//if (found)
		//	cubes.gameObject.Merge(Merger.MergeType.Hide);

		return result.ToArray();
	}

	private bool DetectGround(Vector3 offset)
	{
		foreach (Transform cube in cubes)
			foreach (RaycastHit hit in Physics.RaycastAll(cube.position + offset, Vector3.down, 1f))
				if (IsObstacle(hit.collider.gameObject.layer))
					return true;
		return false;
	}
    private bool DetectGround() => DetectGround(Vector3.zero);

	private void DetectPush()
	{
		foreach (Transform cube in cubes)
			foreach (RaycastHit hit in Physics.RaycastAll(cube.position, Vector3.down, 1f))
				if (hit.collider.gameObject.name == "Push")
				{
					push = hit.collider.gameObject.transform.forward;
				}
	}

	private void DetectSlide()
	{
		foreach (Transform cube in cubes)
			foreach (RaycastHit hit in Physics.RaycastAll(cube.position, Vector3.down, 1f))
				if (hit.collider.gameObject.name == "Slide")
				{
					foreach (RaycastHit hit2 in Physics.RaycastAll(cube.position + hit.collider.gameObject.transform.forward, Vector3.down, 1f))
                    {
						if (IsObstacle(hit2.collider.gameObject.layer) ||
                    hit2.collider.CompareTag("Hole"))
                        {
							slide = hit.collider.gameObject.transform.forward;
						}
                    }
				}
	}

	private bool IsValid()
	{
		// Omino must have ground under it
		foreach (Transform cube in cubes)
			foreach (RaycastHit hit in Physics.RaycastAll(cube.position, Vector3.down))
				if (IsObstacle(hit.collider.gameObject.layer) ||
					hit.collider.CompareTag("Hole"))
					return true;

		return false;
	}

    private bool IsObstacle(int layer) => layer == LayerMask.NameToLayer("Obstacle");

    private void Reject()
	{
        // Roll back
        float t = rollTween.Percentage;
        rollTween.Stop();
        Tween.Rotation(
            target:           transform,
            endValue:         Vector3.zero,
            duration:         Constants.transitionTime * t,
            delay:            0f,
            easeCurve:        Tween.EaseLinear,
            completeCallback: EndMove
        );

		rejected = true;
		dropping = false;

		Buzz();
	}

	private void EndMove()
	{
        if (dontEndMove)
            dontEndMove = false;
        else
        {
		    rolling = false;
            sliding = false;
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

    public Transform[] GetAdjacentCubes(Transform cube)
    {
        var result = new List<Transform>();
        if (cube.CompareTag("Cube"))
            foreach (var dir in new Vector3[] { Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, Vector3.down })
                foreach (var hit in Physics.RaycastAll(cube.position, dir, 1f))
                    if (hit.transform != cube && hit.collider.CompareTag("Cube"))
                        result.Add(hit.transform);
        return result.ToArray();
    }

    public void Detach(Transform cube)
    {
        if (!cube.CompareTag("Cube")) return;
        cube.SetParent(transform.parent);
        cube.gameObject.layer = LayerMask.NameToLayer("Obstacle");
    }

	private void OnTriggerEnter(Collider other)
	{
		// Obstacles
		if (IsObstacle(other.gameObject.layer))
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
		//other.SendMessage("OnOminoEnter", GetClosestCube(other.transform.position), SendMessageOptions.DontRequireReceiver);
	}

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("World"))
            return;

        //other.SendMessage("OnOminoStay", GetClosestCube(other.transform.position), SendMessageOptions.DontRequireReceiver);
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
		//other.SendMessage("OnOminoExit", GetClosestCube(other.transform.position), SendMessageOptions.DontRequireReceiver);
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

    public struct CubeStack
    {
        public readonly int size;
        public readonly GameObject bottom;
        public readonly GameObject[] rest;

        public CubeStack(GameObject bottom, params GameObject[] rest)
        {
            this.bottom = bottom;
            this.rest = rest;
            size = rest.Length + 1;
        }
    }
}
