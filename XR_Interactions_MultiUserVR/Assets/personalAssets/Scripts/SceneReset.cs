using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//not used
public class SceneReset : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene("Menu");
    }
}
