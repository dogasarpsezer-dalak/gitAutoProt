using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dest : MonoBehaviour
{
    public int nextBuildSceneIndex;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene(nextBuildSceneIndex);
        }
    }
}
