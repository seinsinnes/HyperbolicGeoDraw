using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System.Linq;
using System;

public abstract class HyperbolicModelDisplay : MonoBehaviour
{
    public GameObject markerPrefab;
    public GameObject geodesicPrefab;

    public Color markerColor;
    public Color markerSelectedColor;

    public List<GameObject> selectedMarkers = new List<GameObject>();
    public List<GameObject> markers = new List<GameObject>();
    public List<GameObject> geodesics = new List<GameObject>();
    public GameObject draggedMarker = null;

    public GameObject GetDraggedMarker()
    {
        return draggedMarker;
    }

    public void SetDraggedMarker(GameObject dm)
    {
        draggedMarker = dm;
    }

    public List<GameObject> GetSelectedMarkers()
    {
        return selectedMarkers;
    }

    public abstract Vector2 ConvertToModelViewCoords(Vector2 pt);
    public abstract Vector2 ConvertFromModelViewCoords(Vector2 pt);

    public abstract Vector2 PointFromPoincareDiscSpace(Vector2 pt);

    public abstract Vector2 PointToPoincareDiscSpace(Vector2 pt);

    public abstract Vector3[] GenerateCoordinates(Geodesic geo);

    public void CreateGeodesic(GameObject m1, GameObject m2, bool inOtherViews = true)
    {
        //Vector3 modelViewPos = 
        GameObject geodesic = Instantiate(geodesicPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;

        //geodesic.GetComponent<Geodesic>().modelViewCentre.x = modelViewPos.x;
        //geodesic.GetComponent<Geodesic>().modelViewCentre.y = modelViewPos.y;
        geodesic.GetComponent<Geodesic>().modelView = this;

        geodesic.GetComponent<Geodesic>().SetMarkers(m1, m2);
        geodesic.GetComponent<Geodesic>().DrawGeodesic();

        geodesics.Add(geodesic);
        m1.GetComponent<Marker>().geodesics.Add(geodesic);
        m2.GetComponent<Marker>().geodesics.Add(geodesic);

        if (inOtherViews)
        {
            foreach (Tuple<GameObject, GameObject> m in m1.GetComponent<Marker>().markerInOtherViews.Zip(m2.GetComponent<Marker>().markerInOtherViews, (a, b) => Tuple.Create(a, b)))
            {
                m.Item1.GetComponent<Marker>().modelView.CreateGeodesic(m.Item1, m.Item2, false);
            }
        }

    }

    public abstract void TranslateMarkersAlongGeodesic(Vector2 pt1, Vector2 pt2);

    public GameObject CreateMarker(float x, float y)
    {
        DeselectAllMarkers();

        Debug.Log(x + " " + y);
        GameObject marker = Instantiate(markerPrefab, new Vector3(x, y, 0.0f), Quaternion.identity) as GameObject;

        marker.GetComponent<Marker>().modelView = this;

        if (markers.Count > 0)
        {
            CreateGeodesic(marker, markers[markers.Count - 1]);
        }
        markers.Add(marker);
        return marker;
    }

    public void SelectMarker(GameObject marker)
    {
        selectedMarkers.Add(marker);
        Debug.Log(marker);
        Debug.Log(markerSelectedColor);
        marker.GetComponent<SpriteRenderer>().color = markerSelectedColor;

    }

    public void DeselectMarker(GameObject marker)
    {
        selectedMarkers.Remove(marker);
        marker.GetComponent<SpriteRenderer>().color = markerColor;
    }

    public void DeselectAllMarkers()
    {
        foreach (GameObject marker in new List<GameObject>(selectedMarkers))
        {
            DeselectMarker(marker);
        }
    }

    public void ToggleMarker(GameObject marker, bool inOtherViews = true)
    {
        if (selectedMarkers.Contains(marker))
        {
            DeselectMarker(marker);
        }
        else
        {
            SelectMarker(marker);
        }
        if (inOtherViews)
        {
            foreach (GameObject m in marker.GetComponent<Marker>().markerInOtherViews)
            {
                m.GetComponent<Marker>().modelView.ToggleMarker(m, false);
            }
        }
    }

    public GameObject FindGeodesic(GameObject m1, GameObject m2)
    {
        GameObject foundGeodesic = null;
        foreach (GameObject geodesic in geodesics)
        {
            if (geodesic.GetComponent<Geodesic>().marker1 == m1 && geodesic.GetComponent<Geodesic>().marker2 == m2)
            {
                foundGeodesic = geodesic;
                break;
            }
            else if (geodesic.GetComponent<Geodesic>().marker1 == m2 && geodesic.GetComponent<Geodesic>().marker2 == m1)
            {
                foundGeodesic = geodesic;
                break;
            }
        }
        return foundGeodesic;
    }

    public void DeleteGeodesicFromMarkers(GameObject marker1, GameObject marker2, bool inOtherViews = true)
    {
        GameObject geo = FindGeodesic(marker1, marker2);
        if (geo != null)
        {

            Marker m1 = marker1.GetComponent<Marker>();
            Marker m2 = marker2.GetComponent<Marker>();

            m1.geodesics.Remove(geo);
            m2.geodesics.Remove(geo);

            if (inOtherViews)
            {
                foreach (Tuple<GameObject, GameObject> m in m1.markerInOtherViews.Zip(m2.markerInOtherViews, (a, b) => Tuple.Create(a, b)))
                {
                    m.Item1.GetComponent<Marker>().modelView.DeleteGeodesicFromMarkers(m.Item1, m.Item2, false);
                }
            }

            geodesics.Remove(geo);
            Destroy(geo);
        }
    }

    public void DeleteMarker(GameObject marker, bool inOtherViews = true)
    {
        foreach (GameObject geodesic in new List<GameObject>(marker.GetComponent<Marker>().geodesics))
        {
            geodesic.GetComponent<Geodesic>().marker1.GetComponent<Marker>().geodesics.Remove(geodesic);
            geodesic.GetComponent<Geodesic>().marker2.GetComponent<Marker>().geodesics.Remove(geodesic);
            geodesics.Remove(geodesic);
            Destroy(geodesic);
        }

        if (inOtherViews)
        {
            foreach (GameObject m in marker.GetComponent<Marker>().markerInOtherViews)
            {
                m.GetComponent<Marker>().modelView.DeleteMarker(m, false);
            }
        }



        markers.Remove(marker);
        DeselectMarker(marker);
        Destroy(marker);
    }

}
