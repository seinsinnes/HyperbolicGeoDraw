using UnityEngine;
using System.Collections;
using System;

public class HalfPlaneModel : HyperbolicModelDisplay
{

    public override Vector2 ConvertToModelViewCoords(Vector2 pt)
    {
        Vector3 modelViewCentre = gameObject.transform.position;
        Vector2 modelViewPt = new((pt.x - gameObject.transform.position.x) / (gameObject.transform.localScale.x / 4), (pt.y - gameObject.transform.position.y ) / (gameObject.transform.localScale.y / 2) + 1.0f);
        Debug.Log("hp " + modelViewPt);
        return modelViewPt;
    }

    public override Vector2 ConvertFromModelViewCoords(Vector2 pt)
    {
        Vector3 modelViewCentre = gameObject.transform.position;
        Vector2 modelViewPt = new(pt.x * (gameObject.transform.localScale.x / 4) + gameObject.transform.position.x, (pt.y - 1.0f) * (gameObject.transform.localScale.y / 2) + gameObject.transform.position.y );
        Debug.Log("hp2 " + modelViewPt);
        return modelViewPt;
    }

    public override Vector2 PointFromPoincareDiscSpace(Vector2 pt)
    {
        double divisor = (pt.x - 1.0) * (pt.x - 1.0) + pt.y * pt.y;

        return new Vector2((float)((-2 * pt.y) / divisor), (float)((-pt.x * pt.x - pt.y * pt.y + 1.0) / divisor));
    }

    public override Vector2 PointToPoincareDiscSpace(Vector2 pt)
    {
        double divisor = (pt.y + 1.0) * (pt.y + 1.0) + pt.x * pt.x;

        return new Vector2((float)((pt.x * pt.x + pt.y * pt.y - 1.0) / divisor), (float)((-2 * pt.x) / divisor));
    }

    public override Vector3[] GenerateCoordinates(Geodesic geo)
    {
        Vector3[] coordinates = new Vector3[geo.segments];
        double angle;
        double strokeDirection;

        Vector3 startPt = new Vector3((float)(Math.Cos(Mathf.Deg2Rad * geo.startAngle)), (float)(Math.Sin(Mathf.Deg2Rad * geo.startAngle)), 0.0f);
        //Vector3 endPt = new Vector3((float)(Math.Cos(Mathf.Deg2Rad * endAngle) * radius + this.transform.position.x), (float)(Math.Sin(Mathf.Deg2Rad * endAngle) * radius + this.transform.position.y), 0.0f);

        if (Vector3.Distance(ConvertToModelViewCoords(geo.marker1.transform.position), startPt) < Vector3.Distance(ConvertToModelViewCoords(geo.marker2.transform.position), startPt))
        {
            angle = geo.startAngle;
            strokeDirection = 1.0;
        }
        else
        {
            angle = geo.endAngle;
            strokeDirection = -1.0;
        }


        for (int i = 0; i < geo.segments; ++i)
        {
            double y = (Math.Sin(Mathf.Deg2Rad * angle) * geo.radius);
            double x = (Math.Cos(Mathf.Deg2Rad * angle) * geo.radius);

            coordinates[i] = new Vector3((float)(x * (gameObject.transform.localScale.x / 4)), (float)(y * (gameObject.transform.localScale.y / 2)), -0.1f);
            angle += strokeDirection * ((geo.endAngle - geo.startAngle) / (geo.segments - 1));
        }
        Debug.Log("GenCoords " + geo.segments);
        return coordinates;
    }


    public static double HalfPlaneArcRadiusFromCenter(Vector2 center, Vector2 pt1)
    {
        double x1 = pt1.x;
        double y1 = pt1.y;
        double r = Math.Sqrt((x1 - center.x) * (x1 - center.x) + y1 * y1);
        return r;
    }

     public static Vector2 HalfPlaneArcCenterFromLine(Vector2 pt1, Vector2 pt2)
    {
        double x1 = pt1.x;
        double y1 = pt1.y;
        double x2 = pt2.x;
        double y2 = pt2.y;

        double a = (x1 * x1 - x2 * x2 + y1 * y1 - y2 * y2) / (2.0 * x1 - 2.0 * x2);

        return new Vector2((float)a, 0);

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
