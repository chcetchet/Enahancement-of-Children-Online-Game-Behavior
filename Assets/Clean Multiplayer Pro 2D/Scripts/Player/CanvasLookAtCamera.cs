using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvocadoShark
{
    public class CanvasLookAtCamera : MonoBehaviour
    {
        private GameObject mainCamera;
        private RectTransform rectTransform;
        private void Awake()
        {
            mainCamera = Camera.main.gameObject;
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            rectTransform.LookAt(mainCamera.transform);
        }
    }
}
