using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneIndex : MonoBehaviour
{
    public int sceneIndex;

    public void Load()
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void Load(int i)
    {
        SceneManager.LoadSceneAsync(i);
    }
}
