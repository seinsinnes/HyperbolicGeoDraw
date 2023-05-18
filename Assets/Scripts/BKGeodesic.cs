using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BKGeodesic : Geodesic
{


    public override void DrawGeodesic()
    {
        Vector2 p1 = marker1.transform.position;
        Vector2 p2 = marker2.transform.position;

        Vector3[] coordinates = new Vector3[2];
        coordinates[0] = new Vector3(p1.x, p1.y, 0.1f);
        coordinates[1] = new Vector3(p2.x, p2.y, 0.1f);
        LineRenderer lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.positionCount = coordinates.Length;
        lineRenderer.SetPositions(coordinates);

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
