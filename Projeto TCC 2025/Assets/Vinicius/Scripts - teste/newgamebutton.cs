using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorCenas : MonoBehaviour
{
    // M�todo 1: Troca por nome da cena
    public void CarregarCena(string nomeCena)
    {
        if (string.IsNullOrEmpty(nomeCena))
        {
            Debug.LogError("Nome da cena n�o especificado!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nomeCena))
        {
            Debug.LogError($"Cena '{nomeCena}' n�o encontrada no Build Settings!");
            return;
        }

        Debug.Log($"Carregando cena: {nomeCena}");
        SceneManager.LoadScene(nomeCena);
    }

}