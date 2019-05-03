using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowGraph : MonoBehaviour 
{
    //To show the dots on the graph
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private Sprite dotConnection;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform gridTemplate;
    //The container that holds all the graph data
    private RectTransform graphContainer;

    private void Awake()
    {
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
        gridTemplate = graphContainer.Find("GridTemplate").GetComponent<RectTransform>();

        List<int> valueList = new List<int>()
        { 5, 11, 22, 98, 32, 69, 88, 45, 52, 36, 1, 73 };

        this.ShowGraph(valueList);
    }

    /// <summary>
    /// Creates the circle on the graph.
    /// </summary>
    /// <returns>The circle.</returns>
    /// <param name="anchoredPosition">Anchored position.</param>
    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        //Instantiate a new game object with the circle/dot prefab
        GameObject gameObject = new GameObject("Circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;

        //Adjust the rect transform to conform to the graphs box
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;
    }

    /// <summary>
    /// Shows the graph.
    /// </summary>
    /// <param name="valueList">Value list.</param>
    private void ShowGraph(List<int> valueList)
    {
        //The maximum height of the graph container game object in scene
        float graphHeight = graphContainer.sizeDelta.y;

        //The max (top) of the graph
        float yMaximum = 100.0f;
        //Distance between each point on the x-axis
        float xSize = 50.0f;

        GameObject lastCircleGameObject = null;

        //Go through each point to be graphed and create it, then connect it to the last point if possible
        for(int i = 0; i < valueList.Count; i++)
        {
            float xPos = xSize + i * xSize;
            float yPos = (valueList[i] / yMaximum) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPos, yPos));
            if(lastCircleGameObject != null)
            {
                CreateDotConnections(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, 
                circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObject = circleGameObject;

            //Create a new instance of a label on the x axis,
            //Set the parent to the graph container
            //Activate it, and adjust spacing
            //Set the string name to represent the value
            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer, false);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPos, -7.0f);
            labelX.GetComponent<Text>().text = i.ToString();
        }

        //For each value on the Y create a new instance of the label,
        //Set the parent
        //Set it to be active
        int seperatorCount = 10;
        for(int i = 0; i <= seperatorCount; i++)
        {
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            //For the Y values normalize the value using the current count
            //And anchor the position times the graph height otherwise all labels will stack ontop of eachother
            float normalizedYValue = i * 1.0f / seperatorCount;
            labelY.anchoredPosition = new Vector2(-7.0f, normalizedYValue * graphHeight);
            //Show the value in text as a normalized value to the maximum
            labelY.GetComponent<Text>().text = "" + (Mathf.RoundToInt(normalizedYValue * yMaximum));


            //Set the grid on the Y axis the same as setting the label
            RectTransform gridY = Instantiate(gridTemplate);
            gridY.SetParent(graphContainer, false);
            gridY.gameObject.SetActive(true);
            gridY.anchoredPosition = new Vector2(-4.0f, normalizedYValue * graphHeight);
        }
    }

    /// <summary>
    /// Creates the dot connections.
    /// </summary>
    /// <param name="dotPosA">Dot position a.</param>
    /// <param name="dotPosB">Dot position b.</param>
    private void CreateDotConnections(Vector2 dotPosA, Vector2 dotPosB)
    {
        GameObject gameObject = new GameObject("DotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = dotConnection;
        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPosB - dotPosA).normalized;
        float distance = Vector2.Distance(dotPosA, dotPosB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3.0f);
        //Place between A and B
        rectTransform.anchoredPosition = dotPosA + dir * distance * 0.5f;
        //Set the angle to be correct to connect the two points
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));
    }

    /// <summary>
    /// Gets the angle from vector float between 0 and 360.
    /// </summary>
    /// <returns>The angle from vector float.</returns>
    /// <param name="dir">Dir.</param>
    private static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        return n;
    }
}
