using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowGraph : MonoBehaviour 
{
    //To show the dots on the graph
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private Sprite dotConnection;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform gridTemplateY;
    private RectTransform gridTemplateX;
    //The container that holds all the graph data
    private RectTransform graphContainer;
    //List to store all instantiate game objects used in the graph
    private List<GameObject> gameObjectList;

    private float switchTimer = 2.0f;
    private float lastSwitchTime = 0.0f;
    private enum ChartToShow
    {
        Bar,
        Dot
    }
    private ChartToShow chartToShow = ChartToShow.Dot;

    private IGraphVisual lineGraphVisual;
    private IGraphVisual barChartVisual;
    private List<int> valueList;

    private void Awake()
    {
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
        gridTemplateY = graphContainer.Find("GridTemplateY").GetComponent<RectTransform>();
        gridTemplateX = graphContainer.Find("GridTemplateX").GetComponent<RectTransform>();
        gameObjectList = new List<GameObject>();

        valueList = new List<int>();

        valueList.Clear();

        for(int i = 0; i < 23; i++)
        {
            valueList.Add(UnityEngine.Random.Range(-300, 600));
        }

        lineGraphVisual = new LineGraphVisual(graphContainer, dotSprite);
        barChartVisual = new BarChartVisual(graphContainer, Color.blue, 0.9f);

        //Show the graph using day and $ labels for the axis (these can be anything)
        this.ShowGraph(valueList, lineGraphVisual, -1, (int _i) => "Day " + (_i + 1), (float _f) => "$" + Mathf.RoundToInt(_f));
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    private void Update()
    {
        lastSwitchTime += Time.deltaTime;
        //Switch between the two graphs after set time to show that both are working
        if(lastSwitchTime >= switchTimer)
        {
            if(chartToShow == ChartToShow.Dot)
            {
                chartToShow = ChartToShow.Bar;
                this.ShowGraph(valueList, barChartVisual, -1, (int _i) => "Day " + (_i + 1), (float _f) => "$" + Mathf.RoundToInt(_f));
            }
            else if(chartToShow == ChartToShow.Bar)
            {
                chartToShow = ChartToShow.Dot;
                this.ShowGraph(valueList, lineGraphVisual, -1, (int _i) => "Day " + (_i + 1), (float _f) => "$" + Mathf.RoundToInt(_f));
            }

            lastSwitchTime = 0.0f;
        }
    }

    /// <summary>
    /// Shows the graph.
    /// </summary>
    /// <param name="valueList">Value list.</param>
    /// <param name="getAxisLabelX">Get axis label x.</param>
    /// <param name="getAxisLabelY">Get axis label y.</param>
    private void ShowGraph(List<int> valueList, IGraphVisual graphVisual, int maxVisibleValueAmount = -1, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
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

        if(maxVisibleValueAmount <= 0)
        {
            maxVisibleValueAmount = valueList.Count;
        }

        //Cycle through the gameobject list and destroy then clear the list objects
        foreach (GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);
        }
        gameObjectList.Clear();

        //The maximum height and width of the graph container game object in scene
        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        //The max and min of the graph calculated against all values given
        float yMaximum = 0.0f;
        float yMinimum = 0.0f;

        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
        {
            int value = valueList[i];
            if(value > yMaximum)
            {
                yMaximum = value;
            }
            if(value < yMinimum)
            {
                yMinimum = value;
            }
        }

        //For error fixing if a single value is 0
        float yDifference = yMaximum - yMinimum;
        if(yDifference <= 0)
        {
            yDifference = 5.0f;
        }

        //Set the maximum and minimum to 20% difference betwen the max and min given (this is the buffer zone)
        yMaximum = yMaximum + (yDifference * 0.2f);
        yMinimum = yMinimum - (yDifference * 0.2f);

        //Distance between each point on the x-axis to scale to the number of points given to graph
        float xSize = graphWidth / (maxVisibleValueAmount + 1.0f);

        int xIndex = 0;

        //Go through each point to be graphed and create it, then connect it to the last point if possible
        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
        {
            float xPos = xSize + xIndex * xSize;
            float yPos = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;
            Color dotColor;

            //Create a new bar object for the data at the correct position with the size (multiply to space between bars)
            //gameObjectList.AddRange(barChartVisual.AddGraphVisual(new Vector2(xPos, yPos), xSize));
            gameObjectList.AddRange(graphVisual.AddGraphVisual(new Vector2(xPos, yPos), xSize));

            //Set the dot color based on if the value is a positive or negative number
            if (valueList[i] < 0)
            {
                dotColor = Color.red;
                //gameObjectList[i].GetComponent<Image>().color = Color.red;
            }
            else
            {
                dotColor = Color.green;
                //gameObjectList[i].GetComponent<Image>().color = Color.green;

            }

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

            //Increase the index for the x values
            xIndex++;
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

    private interface IGraphVisual
    {
        List<GameObject> AddGraphVisual(Vector2 graphPosition, float graphPositionWidth);
    }

    /// <summary>
    /// Bar chart visual display.
    /// </summary>
    private class BarChartVisual : IGraphVisual
    {
        private RectTransform graphContainer;
        private Color barColor;
        private float barWidthMultiplier;

        //Constructor for the bar chart
        public BarChartVisual(RectTransform graphContainer, Color barColor, float barWidthMultiplier)
        {
            this.graphContainer = graphContainer;
            this.barColor = barColor;
            this.barWidthMultiplier = barWidthMultiplier;
        }

        //Generic function for creating the bar
        public List<GameObject> AddGraphVisual(Vector2 graphPosition, float graphPositionWidth)
        {
            GameObject barGameObject = CreateBar(graphPosition, graphPositionWidth);
            return new List<GameObject>() { barGameObject };
        }

        private GameObject CreateBar(Vector2 graphPosition, float barWidth)
        {
            GameObject gameObject = new GameObject("Bar", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = this.barColor;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            //Set 0 for the graph position on the y so that it comes from the bottom
            rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0.0f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(barWidth * this.barWidthMultiplier, graphPosition.y);
            //Set the pivot to the center and bottom
            rectTransform.pivot = new Vector2(0.5f, 0.0f);
            return gameObject;
        }
    }

    private class LineGraphVisual :IGraphVisual
    {
        private RectTransform graphContainer;
        private Sprite dotSprite;
        private GameObject lastDotGameObject;

        public LineGraphVisual(RectTransform graphContainer, Sprite dotSprite)
        {
            this.graphContainer = graphContainer;
            this.dotSprite = dotSprite;
            this.lastDotGameObject = null;
        }

        public List<GameObject> AddGraphVisual(Vector2 graphPosition, float graphPositionWidth)
        {
            List<GameObject> gameObjectList = new List<GameObject>();

            GameObject dotGameObject = CreateDot(graphPosition);
            gameObjectList.Add(dotGameObject);
            if(lastDotGameObject != null)
            {
                GameObject dotConnectionGameObject = CreateDotConnections(lastDotGameObject.GetComponent<RectTransform>().anchoredPosition, 
                dotGameObject.GetComponent<RectTransform>().anchoredPosition);

                gameObjectList.Add(dotConnectionGameObject);
            }
            lastDotGameObject = dotGameObject;

            return gameObjectList;
        }

        /// <summary>
        /// Creates the dot on the graph.
        /// </summary>
        /// <returns>The dor.</returns>
        /// <param name="anchoredPosition">Anchored position.</param>
        private GameObject CreateDot(Vector2 anchoredPosition)
        {
            //Instantiate a new game object with the dot prefab
            GameObject gameObject = new GameObject("Dot", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().sprite = dotSprite;
            //gameObject.GetComponent<Image>().color = dotColor;

            //Adjust the rect transform to conform to the graphs box
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(11, 11);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            return gameObject;
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
            gameObject.GetComponent<Image>().sprite = dotSprite;
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
