using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContinueButton : MonoBehaviour
{
    public void ContinueGame()
    {
        SceneManager.LoadScene("Lobby");
    }
}