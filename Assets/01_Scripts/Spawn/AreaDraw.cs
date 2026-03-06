using UnityEngine;

public class AreaDraw : MonoBehaviour
{
    public Area area_1;
    public Area area_2;
    public Area area_3;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawCube(transform.position + area_1.area.center, area_1.area.size);

        Gizmos.color = Color.green;

        Gizmos.DrawCube(transform.position + area_2.area.center, area_2.area.size);

        Gizmos.color = Color.blue;

        Gizmos.DrawCube(transform.position + area_3.area.center, area_3.area.size);
    }
}
