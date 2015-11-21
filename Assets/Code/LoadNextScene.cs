using UnityEngine;
using System.Collections;

public class LoadNextScene : MonoBehaviour {

    public void LoadMainScene(string name)
    {
        Application.LoadLevel(name);
    }
}
