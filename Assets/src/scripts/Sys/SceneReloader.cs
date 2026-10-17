using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    [SerializeField] private List<KeyCode> InputsToReload;
    [SerializeField] private string SceneNameToReload;
    void Update()
    {
        if (InputsToReload.Count >= 2 && Input.GetKey(InputsToReload[0]) && Input.GetKeyDown(InputsToReload[1]))
        {
            ReloadScene();
        }
    }


    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneNameToReload);
    }
        
    
    
}
