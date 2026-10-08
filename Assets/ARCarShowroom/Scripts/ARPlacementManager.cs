using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public Camera arCamera;
    public GameObject carPrefab;
    public GameObject placementIndicator;
    static readonly List<ARRaycastHit> Hits = new();
    GameObject currentCar;

    void Update()
    {
        if (!raycastManager) return;
        Vector2 p = default; bool pressed = false;
        if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began) { p = Input.touches[0].position; pressed = true; }
        else if (Input.GetMouseButtonDown(0)) { p = Input.mousePosition; pressed = true; }
        if (raycastManager.Raycast(p, Hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = Hits[0].pose;
            if (placementIndicator) { placementIndicator.SetActive(true); placementIndicator.transform.SetPositionAndRotation(pose.position, pose.rotation); }
            if (pressed && currentCar == null) Place(pose);
        }
    }
    void Place(Pose pose)
    {
        currentCar = Instantiate(carPrefab, pose.position, pose.rotation);
        currentCar.SetActive(true);
        currentCar.transform.localScale = Vector3.one * .55f;
        if (placementIndicator) placementIndicator.SetActive(false);
    }
    public GameObject CurrentCar => currentCar;
    public void ResetCar() { if (currentCar) Destroy(currentCar); currentCar = null; }
}
