using UnityEngine;

public class goLobby : MonoBehaviour
{
    private TestSceneManager testSceneManager;

    void Start()
    {
        testSceneManager = FindObjectOfType<TestSceneManager>();
    }

    public void GoLobby()
    {
        if (testSceneManager != null)
        {
            testSceneManager.GoToLobby();
        }
    }
}
