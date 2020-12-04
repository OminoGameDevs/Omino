using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{

    public static Game instance { get; private set; }
    public int levelNumber { get; private set; }
    public Level level { get; private set; }
    private Level[] levels;

//---------------------------------------------------------------------------------------------------------------------------------------------------------
	
	private void Awake()
	{
		instance = this;
        levels = ResourceLoader.GetAll<Level>();
        LoadLevel(1);
	}

	public void Win()
	{
        BroadcastMessage("OnLevelEnd", SendMessageOptions.DontRequireReceiver);
        Invoke("NextLevel", Constants.fadeOutTime);
	}

    private void NextLevel()
    {
        LoadLevel(levelNumber % levels.Length + 1);
    }
	
	public void LoadLevel(int number)
	{
        levelNumber = number;
        if (level)
            Destroy(level.gameObject);
        level = Instantiate(levels[number-1]);
        level.transform.SetParent(instance.transform);

        BroadcastMessage("OnLevelStart", SendMessageOptions.DontRequireReceiver);
	}

    public void Restart() => LoadLevel(levelNumber);
}
