using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public const float fadeOutTime = 2f;

 //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static Game instance { get; private set; }
    public int levelNumber { get; private set; }
    public Level level { get; private set; }
    private Level[] levels;

//---------------------------------------------------------------------------------------------------------------------------------------------------------
	
	private void Awake()
	{
		instance = this;
        levels = ResourceLoader.GetAll<Level>();
        LoadLevel(3);
	}

	public void Win()
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
	}

    public void Restart() => LoadLevel(levelNumber);
}
