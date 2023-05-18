using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System.Linq;
using System;

public class BeltramiKleinDiscModel : HyperbolicModelDisplay
{

    public override Vector2 PointFromPoincareDiscSpace(Vector2 pt)
    {
        float divisor = 1.0f + pt.x * pt.x + pt.y * pt.y;

        return new Vector2(2 * pt.x / divisor, 2 * pt.y / divisor);
    }

    public override Vector2 PointToPoincareDiscSpace(Vector2 pt)
    {
        double divisor = 1.0 + Math.Sqrt(1.0 - (pt.x * pt.x + pt.y * pt.y));

        return new Vector2((float)(pt.x / divisor), (float)(pt.y / divisor));
    }

    public override Vector2 ConvertToModelViewCoords(Vector2 pt)
    {
        Debug.Log("scale " + gameObject.transform.localScale.x);
        Vector3 modelViewCentre = gameObject.transform.position;
        Vector2 modelViewPt = new((pt.x - gameObject.transform.position.x) / (gameObject.transform.localScale.x / 2), (pt.y - gameObject.transform.position.y) / (gameObject.transform.localScale.y / 2));
        return modelViewPt;
    }

    public override Vector2 ConvertFromModelViewCoords(Vector2 pt)
    {
        Vector3 modelViewCentre = gameObject.transform.position;
        Vector2 modelViewPt = new(pt.x * (gameObject.transform.localScale.x / 2) + gameObject.transform.position.x, pt.y * (gameObject.transform.localScale.y / 2) + gameObject.transform.position.y);
        return modelViewPt;
    }

    public override Vector3[] GenerateCoordinates(Geodesic geo)
    {
        return null;
    }


    public override void TranslateMarkersAlongGeodesic(Vector2 pt1, Vector2 pt2)
    {

    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
