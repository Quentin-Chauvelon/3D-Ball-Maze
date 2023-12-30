using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody _rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Freeze or unfreeze the ball.
    /// </summary>
    internal void FreezeBall(bool freeze)
    {
        _rigidbody.isKinematic = freeze;
    }
}
