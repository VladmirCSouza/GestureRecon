using System.Collections;
using System.Collections.Generic;
using System.IO;
using PDollarGestureRecognizer;
using UnityEngine;
using UnityEngine.UI;

public class GesturesRecon : MonoBehaviour
{
    [SerializeField]
    private Transform gestureOnScreenPrefab;

    [SerializeField]
    private PlayerGestureRecon playerGestureRecon;

    [SerializeField]
    private Text trickNameHud;
    
    private List<Gesture> trainingSet = new List<Gesture>();

    private List<Point> points = new List<Point>();
    private int strokeId = -1;

    private Vector3 virtualKeyPosition = Vector2.zero;
    private Rect drawArea;

    [SerializeField]
    private RectTransform visualDrawArea;

    private RuntimePlatform platform;
    private int vertexCount = 0;

    private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
    private LineRenderer currentGestureLineRenderer;
    
    private bool recognized;
    
    // Start is called before the first frame update
    void Start()
    {
        platform = Application.platform;
        
        drawArea = visualDrawArea.rect;

        //Load pre-made gestures
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
        foreach (TextAsset gestureXml in gesturesXml)
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

        //Load user custom gestures
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (string filePath in filePaths)
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer) {
            if (Input.touchCount > 0) {
                virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
            }
        } else {
            if (Input.GetMouseButton(0)) {
                virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
            }
        }

        if (drawArea.Contains(virtualKeyPosition)) {

            if (Input.GetMouseButtonDown(0)) {

                SetupGestureRecognition();
            }
			
            if (Input.GetMouseButton(0))
            {
                UpdatePointsVector();
            }

            if (Input.GetMouseButtonUp(0) && !recognized)
            {
                StartVectorRecognition();
            }
        }
    }

    private void SetupGestureRecognition()
    {
        if (recognized) {

            recognized = false;
            strokeId = -1;

            points.Clear();

            foreach (LineRenderer lineRenderer in gestureLinesRenderer) {

                lineRenderer.SetVertexCount(0);
                Destroy(lineRenderer.gameObject);
            }

            gestureLinesRenderer.Clear();
        }

        ++strokeId;
				
        Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
        currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer>();
				
        gestureLinesRenderer.Add(currentGestureLineRenderer);
				
        vertexCount = 0;
    }

    private void UpdatePointsVector()
    {
        points.Add(new Point(virtualKeyPosition.x, -virtualKeyPosition.y, strokeId));
        currentGestureLineRenderer.SetVertexCount(++vertexCount);
        currentGestureLineRenderer.SetPosition(vertexCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(virtualKeyPosition.x, virtualKeyPosition.y, 10)));
    }

    private void StartVectorRecognition()
    {
        recognized = true;
        
        Gesture candidate = new Gesture(points.ToArray());
        Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());
			     
        trickNameHud.text = gestureResult.GestureClass;
        if (playerGestureRecon != null)
        {
            playerGestureRecon.SetPlayerAnimationByGesture(gestureResult.GestureClass);
        }
				    
        foreach (LineRenderer lineRenderer in gestureLinesRenderer) {
        
            lineRenderer.SetVertexCount(0);
            Destroy(lineRenderer.gameObject);
        }
        
        gestureLinesRenderer.Clear();
    }
}