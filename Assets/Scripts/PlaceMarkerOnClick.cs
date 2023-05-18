using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Numerics;
using System.Linq;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;


public class PlaceMarkerOnClick : MonoBehaviour
{
    public GameObject poincareModelDisplayPrefab;
    public GameObject bkModelDisplayPrefab;
    public GameObject halfPlaneModelDisplayPrefab;

    Vector2 mouseDownStartPos;
    bool checkMouseDrag = false;
    public float mouseDragThreshold = 0.01f;

    List<GameObject> modelDisplays = new List<GameObject>();
    GameObject currentModelDisplay;
    GameObject poincareModelDisplay;


    void Start()
    {
        GameObject modelDisplay = Instantiate(poincareModelDisplayPrefab, new Vector3(-0.8f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
        modelDisplays.Add(modelDisplay);
        poincareModelDisplay = modelDisplay;
        modelDisplay = Instantiate(bkModelDisplayPrefab, new Vector3(1.0f, 0.4f, 0.0f), Quaternion.identity) as GameObject;
        modelDisplays.Add(modelDisplay);
        modelDisplay = Instantiate(halfPlaneModelDisplayPrefab, new Vector3(0.9f, -0.7f, -0.2f), Quaternion.identity) as GameObject;
        modelDisplays.Add(modelDisplay);
    }

    void Update()
    {
        Collider2D[] col = {};
        WebGLInput.captureAllKeyboardInput = true;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            mouseDownStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            col = Physics2D.OverlapPointAll(mouseDownStartPos);
            Collider2D[] modelView = col.Where(c => c.gameObject.name.Contains("ModelView")).ToArray();
            if (modelView.Length > 0)
            {
                currentModelDisplay = modelView[0].gameObject;
            }
            Collider2D[] markerObjects = col.Where(c => c.gameObject.name.Contains("Marker")).ToArray();
    
            if (markerObjects.Length > 0)
            {
                currentModelDisplay.GetComponent<HyperbolicModelDisplay>().SetDraggedMarker(markerObjects[0].gameObject);
            }
            checkMouseDrag = true;
        }

        if (checkMouseDrag)
        {
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 InModelClickedCoords = currentModelDisplay.GetComponent<HyperbolicModelDisplay>().ConvertToModelViewCoords(new Vector2(ray.x, ray.y));
            Vector2 worldCoords = currentModelDisplay.GetComponent<HyperbolicModelDisplay>().PointToPoincareDiscSpace(InModelClickedCoords);

            if ((worldCoords.magnitude < 1.0) && ((ray - mouseDownStartPos).magnitude > mouseDragThreshold))
            {
                if (currentModelDisplay.GetComponent<HyperbolicModelDisplay>().GetDraggedMarker() != null)
                {

                    currentModelDisplay.GetComponent<HyperbolicModelDisplay>().GetDraggedMarker().transform.position = ray;
                    foreach (GameObject geodesic in currentModelDisplay.GetComponent<HyperbolicModelDisplay>().GetDraggedMarker().GetComponent<Marker>().geodesics)
                    {
                        geodesic.GetComponent<Geodesic>().DrawGeodesic();
                    }
                    foreach(GameObject m in currentModelDisplay.GetComponent<HyperbolicModelDisplay>().GetDraggedMarker().GetComponent<Marker>().markerInOtherViews)
                    {
                        //.PointFromPoincareDiscSpace(
                        Vector2 pt = currentModelDisplay.GetComponent<HyperbolicModelDisplay>().PointToPoincareDiscSpace(currentModelDisplay.GetComponent<HyperbolicModelDisplay>().ConvertToModelViewCoords(ray));
                        pt = m.GetComponent<Marker>().modelView.GetComponent<HyperbolicModelDisplay>().PointFromPoincareDiscSpace(pt);
                        m.transform.position = m.GetComponent<Marker>().modelView.GetComponent<HyperbolicModelDisplay>().ConvertFromModelViewCoords(pt);
                        foreach (GameObject geodesic in m.GetComponent<Marker>().geodesics)
                        {
                            geodesic.GetComponent<Geodesic>().DrawGeodesic();
                        }
                    }
                }

            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {


            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            bool didDrag = false;
            if (checkMouseDrag)
            {

                if ((ray - mouseDownStartPos).magnitude > mouseDragThreshold)
                {
                    didDrag = true;


                }
                checkMouseDrag = false;
                currentModelDisplay.GetComponent<HyperbolicModelDisplay>().SetDraggedMarker(null);
            }

            if (!didDrag)
            {
                Collider2D[] modelView = { };
                Collider2D[] markerObjects = { };
                col = Physics2D.OverlapPointAll(ray);
                if (col.Length > 0)
                {
                    modelView = col.Where(c => c.gameObject.name.Contains("ModelView")).ToArray();
                    if (modelView.Length > 0)
                    {
                        currentModelDisplay = modelView[0].gameObject;
                    }
                    markerObjects = col.Where(c => c.gameObject.name.Contains("Marker")).ToArray();
                }
                if (markerObjects.Length > 0)
                {
                    foreach (Collider2D c in markerObjects)
                    {
                        currentModelDisplay.GetComponent<HyperbolicModelDisplay>().ToggleMarker(c.gameObject);
                    }
                }
                else
                {
                    //currentModelDisplay.GetComponent<HyperbolicModelDisplay>().CreateMarker(ray.x, ray.y);
                    Vector2 InModelClickedCoords = currentModelDisplay.GetComponent<HyperbolicModelDisplay>().ConvertToModelViewCoords(new Vector2(ray.x, ray.y));
                    List<GameObject> markerInAllViews = new List<GameObject>(); 
                    foreach (GameObject md in modelDisplays)
                    {
                        var hmd = md.GetComponent(typeof(HyperbolicModelDisplay)) as HyperbolicModelDisplay;
                        Vector2 worldCoords = currentModelDisplay.GetComponent<HyperbolicModelDisplay>().PointToPoincareDiscSpace(InModelClickedCoords);
                        if (worldCoords.magnitude > 1.0) break;
                        worldCoords = hmd.PointFromPoincareDiscSpace(worldCoords);

                        Debug.Log("ConvToWorld");
                        worldCoords = hmd.ConvertFromModelViewCoords(worldCoords);
                        Debug.Log("EndConvToWorld");

                        GameObject marker = hmd.CreateMarker(worldCoords.x, worldCoords.y);
                        markerInAllViews.Add(marker);
                    }
                    foreach (GameObject m in markerInAllViews)
                    {
                        List<GameObject> markerInOtherViews = new List<GameObject>(markerInAllViews);
                        markerInOtherViews.Remove(m);
                        m.GetComponent<Marker>().markerInOtherViews = markerInOtherViews;

                    }
                }
            }

        }
        if (Input.GetKeyUp(KeyCode.C) || (col.Length > 0 && col[0].gameObject.name == "hyper_connect"))
        {
            foreach (GameObject marker1 in currentModelDisplay.GetComponent<HyperbolicModelDisplay>().GetSelectedMarkers())
            {
                foreach (GameObject marker2 in currentModelDisplay.GetComponent<HyperbolicModelDisplay>().GetSelectedMarkers())
                {
                    if (marker1 != marker2)
                    {
                        if (currentModelDisplay.GetComponent<HyperbolicModelDisplay>().FindGeodesic(marker1, marker2) == null)
                        {
                            currentModelDisplay.GetComponent<HyperbolicModelDisplay>().CreateGeodesic(marker1, marker2);
                        }
                    }
                }
            }

        }
        if (Input.GetKeyUp(KeyCode.D) || (col.Length > 0 && col[0].gameObject.name == "hyper_disconnect"))
        {
            foreach (GameObject marker1 in currentModelDisplay.GetComponent<HyperbolicModelDisplay>().GetSelectedMarkers())
            {
                foreach (GameObject marker2 in currentModelDisplay.GetComponent<HyperbolicModelDisplay>().GetSelectedMarkers())
                {
                    if (marker1 != marker2)
                    {
                        currentModelDisplay.GetComponent<HyperbolicModelDisplay>().DeleteGeodesicFromMarkers(marker1, marker2);

                    }
                }
            }

        }
        if (Input.GetKeyUp(KeyCode.Backspace) || (col.Length > 0 &&  col[0].gameObject.name == "hyper_delete"))
        {
            foreach (GameObject marker in new List<GameObject>(currentModelDisplay.GetComponent<HyperbolicModelDisplay>().GetSelectedMarkers()))
            {
                currentModelDisplay.GetComponent<HyperbolicModelDisplay>().DeleteMarker(marker);

            }
            
        }
        if (Input.GetKeyUp(KeyCode.X) || (col.Length > 0 && col[0].gameObject.name == "hyper_forward"))
        {
            if (poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().GetSelectedMarkers().Count > 1)
            {
                Vector2 pt1 = poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().ConvertToModelViewCoords(poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().GetSelectedMarkers()[^2].transform.position);
                Vector2 pt2 = poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().ConvertToModelViewCoords(poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().GetSelectedMarkers()[^1].transform.position);

                poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().TranslateMarkersAlongGeodesic(pt1, pt2);
            }

        }
        if (Input.GetKeyUp(KeyCode.Z) || (col.Length > 0 &&  col[0].gameObject.name == "hyper_back"))
        {
            if (poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().GetSelectedMarkers().Count > 1)
            {
                Vector2 pt2 = poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().ConvertToModelViewCoords(poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().GetSelectedMarkers()[^2].transform.position);
                Vector2 pt1 = poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().ConvertToModelViewCoords(poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().GetSelectedMarkers()[^1].transform.position);

                poincareModelDisplay.GetComponent<HyperbolicModelDisplay>().TranslateMarkersAlongGeodesic(pt1, pt2);
            }
        }

    }
}