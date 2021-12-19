
using UnityEngine;
using UnityEngine.SceneManagement;


public class GeneralSettings : MonoBehaviour {
    private void Awake() {
        
        Cursor.lockState = CursorLockMode.Locked;

        
#if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.None;
#endif
    }

    private void Update() {
       
        if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}