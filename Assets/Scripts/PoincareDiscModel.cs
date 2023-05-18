using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System.Linq;
using System;

public class PoincareDiscModel : HyperbolicModelDisplay
{

    public override Vector2 PointFromPoincareDiscSpace(Vector2 pt)
    {
        return pt;
    }

    public override Vector2 PointToPoincareDiscSpace(Vector2 pt)
    {
        return pt;
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

    public override void TranslateMarkersAlongGeodesic(Vector2 pt1, Vector2 pt2)
    {
        Debug.Log(pt1 + " " + pt2);
        Vector2 translationArcCenter = PoincareArcCenterFromLine(pt1, pt2);
        double translationArcRadius = PoincareArcRadiusFromCenter(translationArcCenter);

        Vector2 bpt1, bpt2;
        (bpt1, bpt2) = PoincareInfinityPointsFromGeodesic(translationArcCenter, translationArcRadius);
        Complex a = MoebiusTranslate_a(pt1, pt2, bpt1, bpt2);
        Complex b = MoebiusTranslate_b(pt1, pt2, bpt1, bpt2);
        Complex c = MoebiusTranslate_c(pt1, pt2, bpt1, bpt2);
        Complex d = MoebiusTranslate_d(pt1, pt2, bpt1, bpt2);

        // poincareInfinityPointsFromGeodesic center radius
        foreach (GameObject marker in markers)
        {
            marker.transform.position = ConvertFromModelViewCoords(Moebius(a, b, c, d, ConvertToModelViewCoords(marker.transform.position)));
            foreach (GameObject g in marker.GetComponent<Marker>().geodesics)
            {
                g.GetComponent<Geodesic>().DrawGeodesic();
            }

            foreach (GameObject m in marker.GetComponent<Marker>().markerInOtherViews)
            {
                HyperbolicModelDisplay hmd = m.GetComponent<Marker>().modelView.GetComponent<HyperbolicModelDisplay>();
                Vector2 mpos = hmd.PointToPoincareDiscSpace(hmd.ConvertToModelViewCoords(m.transform.position));
                m.transform.position = hmd.ConvertFromModelViewCoords(hmd.PointFromPoincareDiscSpace(Moebius(a, b, c, d, mpos)));

                foreach (GameObject g in m.GetComponent<Marker>().geodesics)
                {
                    g.GetComponent<Geodesic>().DrawGeodesic();
                }
            }
        }
    }

    public Complex MoebiusTranslate_a(Vector2 startPt, Vector2 endPt, Vector2 bPt1, Vector2 bPt2)
    {
        Complex z1 = new Complex(bPt1.x, bPt1.y);
        Complex z2 = new Complex(bPt2.x, bPt2.y);
        Complex z3 = new Complex(startPt.x, startPt.y);
        Complex z4 = new Complex(endPt.x, endPt.y);
        return -2 * (((z1 * z2) - (z1 * z3) - (z2 * z3) + (z3 * z4)) / ((2 * (z1 * z2)) - (z1 * z3) - (z2 * z3) - (z1 * z4) - (z2 * z4) + (2 * z3 * z4)));
    }



    public Complex MoebiusTranslate_b(Vector2 startPt, Vector2 endPt, Vector2 bPt1, Vector2 bPt2)
    {
        Complex z1 = new Complex(bPt1.x, bPt1.y);
        Complex z2 = new Complex(bPt2.x, bPt2.y);
        Complex z3 = new Complex(startPt.x, startPt.y);
        Complex z4 = new Complex(endPt.x, endPt.y);

        return -2 * (z1 * z2 * (z3 - z4) / ((2 * z1 * z2) - (z1 * z3) - (z2 * z3) - (z1 * z4) - (z2 * z4) + (2 * z3 * z4)));


    }
    public Complex MoebiusTranslate_c(Vector2 startPt, Vector2 endPt, Vector2 bPt1, Vector2 bPt2)
    {
        Complex z1 = new Complex(bPt1.x, bPt1.y);
        Complex z2 = new Complex(bPt2.x, bPt2.y);
        Complex z3 = new Complex(startPt.x, startPt.y);
        Complex z4 = new Complex(endPt.x, endPt.y);

        return 2 * ((z3 - z4) / ((2 * z1 * z2) - (z1 * z3) - (z2 * z3) - (z1 * z4) - (z2 * z4) + (2 * (z3 * z4))));
    }

    public Complex MoebiusTranslate_d(Vector2 startPt, Vector2 endPt, Vector2 bPt1, Vector2 bPt2)
    {
        Complex z1 = new Complex(bPt1.x, bPt1.y);
        Complex z2 = new Complex(bPt2.x, bPt2.y);
        Complex z3 = new Complex(startPt.x, startPt.y);
        Complex z4 = new Complex(endPt.x, endPt.y);

        return -2 * (((z1 * z2) - (z1 * z4) - (z2 * z4) + (z3 * z4)) / ((2 * (z1 * z2)) - (z1 * z3) - (z2 * z3) - (z1 * z4) - (z2 * z4) + (2 * z3 * z4)));
    }

    public Vector2 Moebius(Complex a, Complex b, Complex c, Complex d, Vector2 pt)
    {
        Complex cpt = new Complex(pt.x, pt.y);
        Complex w = (a * cpt + b) / (c * cpt + d);
        return new Vector2((float)w.Real, (float)w.Imaginary);
    }

    public static Vector2 PoincareArcCenterFromLine(Vector2 pt1, Vector2 pt2)
    {

        double x1 = pt1.x;
        double y1 = pt1.y;

        double x2 = pt2.x;
        double y2 = pt2.y;

        double a = (y1 * (x2 * x2 + y2 * y2) - y2 * (x1 * x1 + y1 * y1) + y1 - y2) / (x1 * y2 - y1 * x2);

        double b = (x2 * (x1 * x1 + y1 * y1) - x1 * (x2 * x2 + y2 * y2) + x2 - x1) / (x1 * y2 - y1 * x2);

        return new Vector2((float)(a / -2.0), (float)(b / -2.0));

    }

    public static double PoincareArcRadiusFromCenter(Vector2 center)
    {

        double r = Math.Sqrt(Math.Abs(1.0 - center.x * center.x - center.y * center.y));

        return r;
    }


    public static Vector2 PoincareArcAngles(Vector2 pt1, Vector2 pt2, Vector2 center)
    {
        double a1 = Math.Atan2(pt1.y - center.y, pt1.x - center.x);
        double a2 = Math.Atan2(pt2.y - center.y, pt2.x - center.x);

        double b1 = Math.Atan2(pt1.y, pt1.x);
        double b2 = Math.Atan2(pt2.y, pt2.x);
        double bm = (b1 + b2) / 2.0;

        double angle1 = Math.Min(a1, a2);
        double angle2 = Math.Max(a1, a2);

        if (Math.Abs(angle1 - angle2) > Math.PI)
        {
            a1 = angle1 + (2 * Math.PI);
            angle1 = Math.Min(a1, angle2);
            angle2 = Math.Max(a1, angle2);
        }

        return new Vector2((float)angle1, (float)angle2);
    }


    public (Vector2, Vector2) PoincareInfinityPointsFromGeodesic(Vector2 center, double r)
    {
        double xc = center.x;
        double yc = center.y;
        double dSqrd = xc * xc + yc * yc;
        double d = Math.Sqrt(dSqrd);
        double inter = (dSqrd - r * r) + 1.0;
        double x = inter / (2 * d);
        double y = Math.Sqrt(1.0 - x * x);
        double angle = Math.Acos(xc / d);
        double xr1 = x * Math.Cos(angle) - y * Math.Sin(angle);
        double yr1 = x * Math.Sin(angle) + y * Math.Cos(angle);
        double xr2 = x * Math.Cos(angle) + y * Math.Sin(angle);
        double yr2 = x * Math.Sin(angle) - y * Math.Cos(angle);
        if (yc < 0)
        {
            return (new Vector2((float)xr1, (float)-yr1), new Vector2((float)xr2, (float)-yr2));
        }
        else
        {
            return (new Vector2((float)xr1, (float)yr1), new Vector2((float)xr2, (float)yr2));
        }
    }

    public override Vector3[] GenerateCoordinates(Geodesic geo)
    {
        Vector3[] coordinates = new Vector3[geo.segments];
        double angle;
        double strokeDirection;

        Vector3 startPt = new Vector3((float)(Math.Cos(Mathf.Deg2Rad * geo.startAngle)), (float)(Math.Sin(Mathf.Deg2Rad * geo.startAngle)), 0.0f);
        //Vector3 endPt = new Vector3((float)(Math.Cos(Mathf.Deg2Rad * endAngle) * radius + this.transform.position.x), (float)(Math.Sin(Mathf.Deg2Rad * endAngle) * radius + this.transform.position.y), 0.0f);
        
        if (Vector3.Distance(geo.marker1.transform.position, startPt) < Vector3.Distance(geo.marker2.transform.position, startPt))
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

            coordinates[i] = new Vector3((float)x*(gameObject.transform.localScale.x/2), (float)y*(gameObject.transform.localScale.y / 2), 0.1f);
            angle += strokeDirection * ((geo.endAngle - geo.startAngle) / (geo.segments - 1));
        }
        Debug.Log("GenCoords " + geo.segments);
        return coordinates;
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
