using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace OminoUI.UI
{

    public class UIsystem : MonoBehaviour
    {

        #region Variables
        [Header("Main Properties")]
        public UIscreen _StartScreen;
        [Header("system Events")]
        public UnityEvent onSwitchedScreen = new UnityEvent();

        private Component[] screens = new Component[0];

        private UIscreen previousScreen;
        public UIscreen PreviousScreen { get { return previousScreen; } }

        public UIscreen currentScreen;
        public UIscreen CurrentScreen { get { return currentScreen; } }
        public float transitionTime;

        #endregion


        #region Main Methods
        // Start is called before the first frame update
        void Start()
        {
            screens = GetComponentsInChildren<UIscreen>(true);

            if (_StartScreen)
            {
                SwitchScreens(_StartScreen);
            }
        }
        #endregion

        #region Helper Methods
        public void SwitchScreens(UIscreen aScreen)
        {
            if (aScreen)
            {
                if (currentScreen)
                {
                    currentScreen.CloseScreen();
                    previousScreen = currentScreen;
                    previousScreen.gameObject.SetActive(false);
                }

                currentScreen = aScreen;
                currentScreen.gameObject.SetActive(true);
                currentScreen.StartScreen();

                if (onSwitchedScreen != null)
                {
                    onSwitchedScreen.Invoke();
                }

            }
        }
        public void GoToPreviousScreen()
        {
            if (previousScreen)
            {
                SwitchScreens(previousScreen);
            }
        }
        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(WaitToLoadScene(sceneIndex));

        }

        IEnumerator WaitToLoadScene(int sceneIndex)
        {

            //Add animation
            //Wait
            yield return new WaitForSeconds(transitionTime);
            //Load scene
            SceneManager.LoadScene(sceneIndex);
        }
        #endregion
    }
}
