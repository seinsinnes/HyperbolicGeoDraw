using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Geodesic : MonoBehaviour
{
    public double radius;
    public int segments;
    public double startAngle;
    public double endAngle;
    public GameObject marker1, marker2;
    public HyperbolicModelDisplay modelView;


    public void SetMarkers(GameObject m1, GameObject m2)
    {
        marker1 = m1;
        marker2 = m2;
    }

    public abstract void DrawGeodesic();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
