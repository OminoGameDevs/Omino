using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public const float fadeOutTime = 2f;

//---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static Game instance { get; private set; }
	public static int levelNumber { get; private set; }
    //public static LevelInfo Level { get; private set; }

//---------------------------------------------------------------------------------------------------------------------------------------------------------
	
	void Awake()
	{
		instance = this;
	}
	
	
	
	void Win()
	{
		LoadLevel(levelNumber + 1);
	}
	
	
	
	public static void LoadLevel(int number)
	{
		levelNumber = number;
		Application.LoadLevel("Scene1");
	}



    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
