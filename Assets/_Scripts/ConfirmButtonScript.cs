using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfirmButtonScript : MonoBehaviour
{
    [SerializeField] private GameObject button;
    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != "_ColocationSpace")
        {
            Debug.Log("We are not in the colocation scene.");

            button.SetActive(false);
        }
    }
}
