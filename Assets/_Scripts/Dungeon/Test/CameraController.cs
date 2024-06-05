using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera cam;

    [SerializeField] Slider zoomSlider;
    [SerializeField] float speed;

    Vector3 moveDir;

    private void Awake()
    {
        zoomSlider.onValueChanged.AddListener(ZoomChange);
    }

    void ZoomChange(float value)
    {
        
        cam.orthographicSize = value;   
    }

    private void Update()
    {
        moveDir = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);

        cam.transform.Translate(moveDir * Time.deltaTime* speed);
    }
}
