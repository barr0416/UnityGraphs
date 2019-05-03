using System;
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
    private RectTransform gridTemplateY;
    private RectTransform gridTemplateX;
    //The container that holds all the graph data
    private RectTransform graphContainer;
    //List to store all instantiate game objects used in the graph
    private List<GameObject> gameObjectList;

    private void Awake()
    {
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
        gridTemplateY = graphContainer.Find("GridTemplateY").GetComponent<RectTransform>();
        gridTemplateX = graphContainer.Find("GridTemplateX").GetComponent<RectTransform>();
        gameObjectList = new List<GameObject>();

        List<int> valueList = new List<int>()
        { 5, -110, 22, 98, 32, 69, 88, 45, 52, 36, -1, -73 };

        valueList.Clear();

        for(int i = 0; i < 15; i++)
        {
            valueList.Add(UnityEngine.Random.Range(-300, 600));
        }

        //Show the graph using day and $ labels for the axis (these can be anything)
        this.ShowGraph(valueList, (int _i) => "Day " + (_i + 1), (float _f) => "$" + Mathf.RoundToInt(_f));
    }

    /// <summary>
    /// Creates the circle on the graph.
    /// </summary>
    /// <returns>The circle.</returns>
    /// <param name="anchoredPosition">Anchored position.</param>
    private GameObject CreateCircle(Vector2 anchoredPosition, Color dotColor)
    {
        //Instantiate a new game object with the circle/dot prefab
        GameObject gameObject = new GameObject("Circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        gameObject.GetComponent<Image>().color = dotColor;

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
    /// <param name="getAxisLabelX">Get axis label x.</param>
    /// <param name="getAxisLabelY">Get axis label y.</param>
    private void ShowGraph(List<int> valueList, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        //Set the default axis label to get
        if(getAxisLabelX == null)
        {
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }
        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }

        //Cycle through the gameobject list and destroy then clear the list objects
        foreach(GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);
        }
        gameObjectList.Clear();

        //The maximum height of the graph container game object in scene
        float graphHeight = graphContainer.sizeDelta.y;

        //The max and min of the graph calculated against all values given
        float yMaximum = 0.0f;
        float yMinimum = 0.0f;

        foreach (int value in valueList)
        {
            if(value > yMaximum)
            {
                yMaximum = value;
            }
            if(value < yMinimum)
            {
                yMinimum = value;
            }
        }

        //Set the maximum and minimum to 20% difference betwen the max and min given (this is the buffer zone)
        yMaximum = yMaximum + ((yMaximum - yMinimum) * 0.2f);
        yMinimum = yMinimum - ((yMaximum - yMinimum) * 0.2f);

        //Distance between each point on the x-axis
        float xSize = 50.0f;

        GameObject lastCircleGameObject = null;

        //Go through each point to be graphed and create it, then connect it to the last point if possible
        for(int i = 0; i < valueList.Count; i++)
        {
            float xPos = xSize + i * xSize;
            float yPos = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;
            Color dotColor;

            //Set the dot color based on if the value is a positive or negative number
            if(valueList[i] < 0)
            {
                dotColor = Color.red;
            }
            else
            {
                dotColor = Color.green;
            }

            GameObject circleGameObject = CreateCircle(new Vector2(xPos, yPos), dotColor);
            gameObjectList.Add(circleGameObject);
            if(lastCircleGameObject != null)
            {
                GameObject dotConnectionGameObject = CreateDotConnections(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, 
                circleGameObject.GetComponent<RectTransform>().anchoredPosition);

                gameObjectList.Add(dotConnectionGameObject);
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
            labelX.GetComponent<Text>().text = getAxisLabelX(i);
            gameObjectList.Add(labelX.gameObject);

            //Set the grid on the X axis the same as setting the label
            RectTransform gridX = Instantiate(gridTemplateX);
            gridX.SetParent(graphContainer, false);
            gridX.gameObject.SetActive(true);
            gridX.anchoredPosition = new Vector2(xPos, -3.0f);
            gameObjectList.Add(gridX.gameObject);
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
            //Show the value in text as a normalized value using the minimum and maximum
            labelY.GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedYValue * (yMaximum - yMinimum)));
            gameObjectList.Add(labelY.gameObject);

            //Set the grid on the Y axis the same as setting the label
            RectTransform gridY = Instantiate(gridTemplateY);
            gridY.SetParent(graphContainer, false);
            gridY.gameObject.SetActive(true);
            gridY.anchoredPosition = new Vector2(-4.0f, normalizedYValue * graphHeight);
            gameObjectList.Add(gridY.gameObject);
        }
    }

    /// <summary>
    /// Creates the dot connections.
    /// </summary>
    /// <param name="dotPosA">Dot position a.</param>
    /// <param name="dotPosB">Dot position b.</param>
    private GameObject CreateDotConnections(Vector2 dotPosA, Vector2 dotPosB)
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
        return gameObject;
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
