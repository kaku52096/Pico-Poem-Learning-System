using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class PicoHandRay : MonoBehaviour
{
    public XRRayInteractor interactor;
    private RectTransform rectTransform;
    private Image image;
    private RaycastResult rayResult;


    void Start()
    {
        rectTransform = this.GetComponent<RectTransform>();
        image = this.GetComponent<Image>();
    }


    // Update is called once per frame
    void Update()
    {
        if (ChatController.Instance.IsChatPanelActive())
        {
            ShowRayPos();
        }
    }


    private void ShowRayPos()
    {
        if (interactor.TryGetCurrentUIRaycastResult(out rayResult) && rayResult.isValid)
        {
            image.enabled = true;
            rectTransform.position = rayResult.worldPosition;
        }
        else
        {
            image.enabled = false;
        }
    }
}
