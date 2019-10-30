using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeskChair : MonoBehaviour
{
    [SerializeField]
    private GameObject _camera;

    private void Update()
    {
        transform.position = new Vector3(_camera.transform.position.x, 0, _camera.transform.position.z);
        transform.rotation = Quaternion.Euler(0, _camera.transform.rotation.z, 0);
    }
}
