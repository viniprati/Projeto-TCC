using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsButton : MonoBehaviour
{
    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings"); 
    }
}
