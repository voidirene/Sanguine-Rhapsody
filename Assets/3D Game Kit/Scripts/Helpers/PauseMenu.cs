using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject container;
    
    void Update()
    {
        if (input.GetKeyDown(KeyCode.Escape))
        {
            container.SetActive(true);
            time.timeScale = 0;
        }
    }

    public void ResumeButton()
    {
        container.SetActive(false);
        time.timeScale = 1;
    }

}
