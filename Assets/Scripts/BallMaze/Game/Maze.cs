using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Maze : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void BuildMaze(int level)
    {
        //TODO: build the maze
    }


    /// <summary>
    /// Deletes all descendants of the maze object.
    /// </summary>
    public void ClearMaze()
    {
        foreach (GameObject obj in gameObject.transform)
        {
            Destroy(obj);
        }
    }


    public void UpdateMazeOrientation()
    {
        //level.transform.rotation = Quaternion.Euler(joystick.Vertical * -10, 0f, joystick.Horizontal * 10);
    }
}
