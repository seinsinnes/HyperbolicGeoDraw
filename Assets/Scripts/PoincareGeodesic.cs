using UnityEngine;
using System.Collections;

public class PoincareGeodesic : Geodesic
{

    public override void DrawGeodesic()
    {
        Vector2 p1 = modelView.ConvertToModelViewCoords(marker1.transform.position);
        Vector2 p2 = modelView.ConvertToModelViewCoords(marker2.transform.position);

        Vector2 center = PoincareDiscModel.PoincareArcCenterFromLine(p1, p2);
        radius = PoincareDiscModel.PoincareArcRadiusFromCenter(center);
        Vector2 angles = PoincareDiscModel.PoincareArcAngles(p1, p2, center);
        startAngle = Mathf.Rad2Deg * angles.x;
        endAngle = Mathf.Rad2Deg * angles.y;
        this.transform.position = modelView.ConvertFromModelViewCoords(center);
        Vector3[] coordinates = modelView.GenerateCoordinates(this);
        LineRenderer lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.positionCount = coordinates.Length;
        lineRenderer.SetPositions(coordinates);

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
