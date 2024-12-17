using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleLogo : MonoBehaviour
{
    public Image fadeImage;

    public List<LineRenderer> currentLineRenderers;
    public List<LineRenderer> drawLineRenderers;
    public List<TrailRenderer> trailRenderers;
    private void Start()
    {
        StartCoroutine(FadeCoroutine());
    }
    IEnumerator FadeCoroutine()
    {
        Color color = Color.black;
        color.a = 1f;

        float t = 1;
        while (t >= 0)
        {
            t -= Time.deltaTime;

            color.a = t;

            fadeImage.color = color;

            yield return null;
        }

        color.a = 0;
        fadeImage.color = color;
        fadeImage.enabled = false;

        StartDraw();
    }
    void StartDraw()
    {
        StartCoroutine(DrawCoroutine());
    }
    IEnumerator DrawCoroutine()
    {

        for (int i = 0; i < currentLineRenderers.Count; i++)
        {
            trailRenderers[i].transform.position = currentLineRenderers[i].GetPosition(0);

            trailRenderers[i].enabled = true;

            Vector3[] positions = new Vector3[currentLineRenderers[i].positionCount];

            currentLineRenderers[i].GetPositions(positions);

            List<Vector3> drawPositions = new List<Vector3>();

            for (int j = 0; j < positions.Length; j++)
            {
                trailRenderers[i].transform.position = positions[j];

                drawPositions.Add(positions[j]);

                yield return new WaitForSeconds(0.007f);

                drawLineRenderers[i].positionCount = drawPositions.Count;

                drawLineRenderers[i].SetPositions(drawPositions.ToArray());
            }
        }
    }

    //public float minDistance;
    //public int currentLineRendererIndex = -1;
    //public LineRenderer lineRendererPrefab;
    //public InputActionReference mousePositionAction;
    //Vector3 beforePosition = Vector3.zero;
    //bool isClick = false;
    //Coroutine coroutine = null;
    //public void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        coroutine = StartCoroutine(StartLineDrawCoroutine());
    //    }
    //    if (Input.GetMouseButtonDown(1))
    //    {
    //        StopCoroutine(coroutine);
    //    }
    //}
    //IEnumerator StartLineDrawCoroutine()
    //{
    //    currentLineRendererIndex++;

    //    currentLineRenderers.Add(Instantiate(lineRendererPrefab, transform).GetComponent<LineRenderer>());

    //    isClick = true;

    //    List<Vector3> positions = new List<Vector3>();

    //    while (true)
    //    {
    //        Vector3 point = Vector3.zero;
    //        Vector2 mousePos = mousePositionAction.action.ReadValue<Vector2>();

    //        point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    //        //mousePos.x -= Screen.width * 0.5f;
    //        //mousePos.y -= Screen.height * 0.5f;

    //        if (isClick)
    //        {
    //            beforePosition = point;
    //            isClick = false;
    //        }

    //        if (point.DistanceTo(beforePosition) > minDistance)
    //        {
    //            beforePosition = point;

    //            positions.Add(point);

    //            currentLineRenderers[currentLineRendererIndex].positionCount = positions.Count;
    //            currentLineRenderers[currentLineRendererIndex].SetPositions(positions.ToArray());
    //        }

    //        yield return null;
    //    }
    //}
}
