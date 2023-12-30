namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using System.Collections.Generic;

    public class Editor_STSWindow : EditorWindow
    {
        #region Initialization
        [MenuItem("Tools/Simple Traffic System/STS Tools Window", false, 2)]
        public static void ShowWindow()
        {
            Editor_STSWindow window = (Editor_STSWindow)GetWindow(typeof(Editor_STSWindow));
            window.minSize = new Vector2(300, 200);
            window.titleContent.text = "STS Tools";
            window.Show();
        }
        #endregion

        #region WindowVariables
        public static Editor_STSWindow editorWindow;
        Vector2 scrollPos = new Vector2();
        public enum EditMode
        {
            Off,
            LaneConnector,
            RouteConnector,
            RouteEditor,
            SignalConnector,
            SpawnPoints,
            StopConnector,
            YieldTriggers,
            Cidy2
        }
        [SerializeField] EditMode editMode;
        bool showDebug = false;
        bool requireRepaint;
        bool showSpawnButtons = true;
        bool showImportButtons = false;
        bool showImportDemosButtons = false;
        public AITrafficWaypointRoute[] _routes;
        // Signal Connector
        public AITrafficLight[] SC_lights;
        [SerializeField] public AITrafficLight SC_light;
        [SerializeField] public AITrafficWaypointRoute SC_route;
        public int SC_lightIndex = -1;
        public int SC_routeIndex = -1;
        // Route Connector
        public AITrafficWaypoint RC_fromPoint;
        public AITrafficWaypoint RC_toPoint;
        public int RC_fromRouteIndex = -1;
        public int RC_fromPointIndex = -1;
        public int RC_toRouteIndex = -1;
        public int RC_toPointIndex = -1;
        // Lane Connector
        public AITrafficWaypointRoute LC_routeA;
        public AITrafficWaypointRoute LC_routeB;
        public int LC_routeIndexA = -1;
        public int LC_routeIndexB = -1;
        // Route Editor
        public AITrafficWaypointRoute RE_route;
        public List<AITrafficWaypoint> RE_connectedWaypoints;
        public int RE_routeIndex = -1;
        public enum RouteEditorProperty
        {
            ObjectOnly,
            All,
            SpeedLimit,
            StopDriving,
            NewRoutePoints,
            LaneChangePoints,
            OnReachWaypointEvent
        }
        public RouteEditorProperty routeEditorProperty;
        // Yield Triggers
        public AITrafficWaypointRoute YT_route;
        public AITrafficWaypoint YT_waypoint;
        public AITrafficWaypointRouteInfo YT_routeInfo;
        public List<AITrafficWaypointRouteInfo> YT_routeInfoList = new List<AITrafficWaypointRouteInfo>();
        public int YT_configureRouteIndex = -1;
        public int YT_configureWaypointIndex = -1;
        public int YT_routeInfoIndex = -1;
        // Stop Connector
        public AITrafficStop[] STC_stops;
        [SerializeField] public AITrafficStop STC_stop;
        [SerializeField] public AITrafficWaypointRoute STC_route;
        public int STC_stopIndex = -1;
        public int STC_routeIndex = -1;
        // Handle Settings
        float handleSize = 1f;
        float handleMaxSizeDistance = 50f;
        Vector3 handleTextoffset = new Vector3(0, 0.5f, 0);
        #endregion

#if CiDy
        CiDy.CiDyGraph cidyGraph;
#endif


        #region InspectorWindow GUI
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxWidth(1000), GUILayout.MaxHeight(2000));
            SerializedObject serialObj = new SerializedObject(this);
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 18 };
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            showImportDemosButtons = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showImportDemosButtons, "   Import Additional Demos", true);
            if (showImportDemosButtons)
            {
                if (RenderPipeline.IsHDRP)
                {
                    if (GUILayout.Button("HDRP"))
                    {
                        AssetDatabase.ImportPackage(STSPrefs.HDRP_DemosPath, true);
                    }
                    if (GUILayout.Button("Update Project Materials for HDRP"))
                    {
                        EditorApplication.ExecuteMenuItem("Edit/Render Pipeline/Upgrade Project Materials to High Definition Materials");
                    }
                }
                else if (RenderPipeline.IsURP)
                {
                    if (GUILayout.Button("URP"))
                    {
                        AssetDatabase.ImportPackage(STSPrefs.URP_DemosPath, true);
                    }
                    if (GUILayout.Button("Update Project Materials for URP"))
                    {
                        EditorApplication.ExecuteMenuItem("Edit/Render Pipeline/Universal Render Pipeline/Upgrade Project Materials to UniversalRP Materials");
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Current rendering pipeline is default. HDRP and URP demos are available if the project is using one of those pipelines.", MessageType.Info);
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            showImportButtons = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showImportButtons, "   Import Integration Package", true);
            if (showImportButtons)
            {
                EditorGUILayout.HelpBox("The 3rd party packages are required for each integration.", MessageType.Info);
                if (GUILayout.Button("CiDy 2 Integration"))
                {
                    AssetDatabase.ImportPackage(STSPrefs.CiDyIntegrationPath, true);
                }
                if (GUILayout.Button("Stylized Vehicles Pack"))
                {
                    AssetDatabase.ImportPackage(STSPrefs.StylizedVehiclesIntegrationPath, true);
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            showSpawnButtons = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showSpawnButtons, "   Spawn", true);
            if (showSpawnButtons)
            {
                if (GUILayout.Button("AI Traffic Controller"))
                {
                    GameObject _objectToSpawn = Instantiate(STSRefs.AssetReferences._AITrafficController);
                    _objectToSpawn.name = "AITrafficController";
                    GameObject[] newSelection = new GameObject[1];
                    newSelection[0] = _objectToSpawn;
                    Selection.objects = newSelection;
                    Undo.RegisterCreatedObjectUndo(_objectToSpawn, "SpawnAITrafficController");
                }
                if (STSRefs.AssetReferences._AITrafficController_StylizedVehiclesPack != null)
                {
                    if (GUILayout.Button("AI Traffic Controller - Stylized Vehicles Pack"))
                    {
                        GameObject _objectToSpawn = Instantiate(STSRefs.AssetReferences._AITrafficController_StylizedVehiclesPack);
                        _objectToSpawn.name = "AITrafficController";
                        GameObject[] newSelection = new GameObject[1];
                        newSelection[0] = _objectToSpawn;
                        Selection.objects = newSelection;
                        Undo.RegisterCreatedObjectUndo(_objectToSpawn, "SpawnAITrafficController");
                    }
                }
                if (GUILayout.Button("AI Traffic Waypoint Route"))
                {
                    GameObject _objectToSpawn = Instantiate(STSRefs.AssetReferences._AITrafficWaypointRoute);
                    _objectToSpawn.name = "AITrafficWaypointRoute";
                    GameObject[] newSelection = new GameObject[1];
                    newSelection[0] = _objectToSpawn;
                    Selection.objects = newSelection;
                    Undo.RegisterCreatedObjectUndo(_objectToSpawn, "SpawnWaypointRoute");
                }
                if (GUILayout.Button("AI Traffic Light Manager"))
                {
                    GameObject _objectToSpawn = Instantiate(STSRefs.AssetReferences._AITrafficLightManager);
                    _objectToSpawn.name = "AITrafficLightManager";
                    GameObject[] newSelection = new GameObject[1];
                    newSelection[0] = _objectToSpawn;
                    Selection.objects = newSelection;
                    Undo.RegisterCreatedObjectUndo(_objectToSpawn, "SpawnTrafficLightManager");
                }
                if (GUILayout.Button("AI Traffic Stop Manager"))
                {
                    GameObject _objectToSpawn = Instantiate(STSRefs.AssetReferences._AITrafficStopManager);
                    _objectToSpawn.name = "AITrafficStopManager";
                    GameObject[] newSelection = new GameObject[1];
                    newSelection[0] = _objectToSpawn;
                    Selection.objects = newSelection;
                    Undo.RegisterCreatedObjectUndo(_objectToSpawn, "SpawnTrafficLStopManager");
                }
                if (GUILayout.Button("Spline Route Creator"))
                {
                    GameObject _objectToSpawn = Instantiate(STSRefs.AssetReferences._SplineRouteCreator);
                    _objectToSpawn.name = "SplineRouteCreator";
                    GameObject[] newSelection = new GameObject[1];
                    newSelection[0] = _objectToSpawn;
                    Selection.objects = newSelection;
                    Undo.RegisterCreatedObjectUndo(_objectToSpawn, "SplineRouteCreator");
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUI.BeginChangeCheck();
            EditMode newEditMode = (EditMode)EditorGUILayout.EnumPopup("Configure Mode", editMode);
            if (EditorGUI.EndChangeCheck())
            {
                UndoRecordTargetObject(this, "Mode Changed");
                ClearData(true);
                editMode = newEditMode;
                SceneView.RepaintAll();
            }

            if (requireRepaint)
            {
                requireRepaint = false;
                Repaint();
            }

            switch (editMode)
            {
                case EditMode.LaneConnector:
                    #region LaneConnectorGUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("LANE CONNECTOR", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(40));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        GUI.enabled = false;
                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);

                        SerializedProperty LC_routeAProperty = serialObj.FindProperty("LC_routeA");
                        EditorGUILayout.PropertyField(LC_routeAProperty, true);

                        SerializedProperty LC_routeBProperty = serialObj.FindProperty("LC_routeB");
                        EditorGUILayout.PropertyField(LC_routeBProperty, true);

                        SerializedProperty LC_routeIndexAProperty = serialObj.FindProperty("LC_routeIndexA");
                        EditorGUILayout.PropertyField(LC_routeIndexAProperty, true);

                        SerializedProperty LC_routeIndexBProperty = serialObj.FindProperty("LC_routeIndexB");
                        EditorGUILayout.PropertyField(LC_routeIndexBProperty, true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);



                    if (_routes.Length > 0)
                    {
                        if (LC_routeIndexA == -1 || LC_routeIndexB == -1)
                            EditorGUILayout.HelpBox("Press R handle button to select routes A/B", MessageType.Info);
                        GUI.enabled = false;
                        SerializedProperty LC_routeAProperty = serialObj.FindProperty("LC_routeA");
                        EditorGUILayout.PropertyField(LC_routeAProperty, true);
                        SerializedProperty LC_routeBProperty = serialObj.FindProperty("LC_routeB");
                        EditorGUILayout.PropertyField(LC_routeBProperty, true);
                        GUI.enabled = true;

                        if (LC_routeIndexA != -1 && LC_routeIndexB != -1)
                        {
                            if (GUILayout.Button("Setup Lane Change Points"))
                            {
                                List<Object> objList = new List<Object> { this, LC_routeA, LC_routeB };
                                for (int i = 0; i < LC_routeA.waypointDataList.Count; i++)
                                {
                                    objList.Add(LC_routeA.waypointDataList[i]._waypoint);
                                }
                                for (int i = 0; i < LC_routeB.waypointDataList.Count; i++)
                                {
                                    objList.Add(LC_routeB.waypointDataList[i]._waypoint);
                                }
                                Object[] objs = objList.ToArray();
                                UndoRecordTargetObject(objs, "Setup Lane Change Points");
                                AssignLaneChangePoints(LC_routeA, LC_routeB);
                                EditorUtility.SetDirty(LC_routeA);
                                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                AssignLaneChangePoints(LC_routeB, LC_routeA);
                                EditorUtility.SetDirty(LC_routeB);
                                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                ClearData(false);
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.RouteConnector:
                    #region RouteConnectorGUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("ROUTE CONNECTOR", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(40));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        GUI.enabled = false;
                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);

                        SerializedProperty RC_fromRouteIndexProperty = serialObj.FindProperty("RC_fromRouteIndex");
                        EditorGUILayout.PropertyField(RC_fromRouteIndexProperty, true);

                        SerializedProperty RC_fromPointIndexProperty = serialObj.FindProperty("RC_fromPointIndex");
                        EditorGUILayout.PropertyField(RC_fromPointIndexProperty, true);

                        SerializedProperty RC_toRouteIndexProperty = serialObj.FindProperty("RC_toRouteIndex");
                        EditorGUILayout.PropertyField(RC_toRouteIndexProperty, true);

                        SerializedProperty RC_toPointIndexProperty = serialObj.FindProperty("RC_toPointIndex");
                        EditorGUILayout.PropertyField(RC_toPointIndexProperty, true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);



                    if (_routes.Length > 0)
                    {
                        if (RC_fromRouteIndex == -1 || RC_toRouteIndex == -1)
                            EditorGUILayout.HelpBox("Press F/T handle buttonss to select From/To points", MessageType.Info);
                        GUI.enabled = false;
                        SerializedProperty fromPointProperty = serialObj.FindProperty("RC_fromPoint");
                        EditorGUILayout.PropertyField(fromPointProperty, true);

                        SerializedProperty toPointProperty = serialObj.FindProperty("RC_toPoint");
                        EditorGUILayout.PropertyField(toPointProperty, true);
                        GUI.enabled = true;

                        if (RC_fromPoint != null && RC_toPoint != null)
                        {
                            if (GUILayout.Button("Connect Route Points"))
                            {
                                Object[] objs = new Object[] { this, RC_fromPoint };
                                UndoRecordTargetObject(objs, "Connect Route Points");

                                AITrafficWaypoint[] currentArray = new AITrafficWaypoint[RC_fromPoint.onReachWaypointSettings.newRoutePoints.Length + 1];
                                for (int i = 0; i < RC_fromPoint.onReachWaypointSettings.newRoutePoints.Length; i++)
                                {
                                    currentArray[i] = RC_fromPoint.onReachWaypointSettings.newRoutePoints[i];
                                }
                                currentArray[currentArray.Length - 1] = RC_toPoint;
                                RC_fromPoint.onReachWaypointSettings.newRoutePoints = currentArray;

                                EditorUtility.SetDirty(RC_fromPoint);
                                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                                ClearData(false);
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.RouteEditor:
                    #region RouteEditorGUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("ROUTE EDITOR", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(45));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        GUI.enabled = false;
                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);

                        SerializedProperty RE_connectedWaypoints = serialObj.FindProperty("RE_connectedWaypoints");
                        EditorGUILayout.PropertyField(RE_connectedWaypoints, true);

                        SerializedProperty RE_route = serialObj.FindProperty("RE_route");
                        EditorGUILayout.PropertyField(RE_route, true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (_routes.Length > 0)
                    {




                        if (RE_route == null)
                        {
                            EditorGUILayout.HelpBox("Press R handle button to load route waypoints", MessageType.Info);
                            EditorGUILayout.HelpBox("Button is located above final route points.", MessageType.Info);
                        }
                        else
                        {
                            if (GUILayout.Button("Unload Waypoints"))
                            {
                                UndoRecordTargetObject(this, "Unload Route Waypoints");
                                RE_route = null;
                                RE_routeIndex = -1;
                            }

                            if (RE_route != null)
                            {
                                if (GUILayout.Button("Clear All Lane Change Points"))
                                {
                                    List<Object> objList = new List<Object> { this, RE_route };
                                    for (int i = 0; i < RE_route.waypointDataList.Count; i++)
                                    {
                                        objList.Add(RE_route.waypointDataList[i]._waypoint);
                                    }
                                    Object[] objs = objList.ToArray();
                                    UndoRecordTargetObject(objs, "Clear All Lane Change Points");
                                    RE_route.ClearAllLaneChangePoints();
                                }
                                if (GUILayout.Button("Clear All New Route Points"))
                                {
                                    List<Object> objList = new List<Object> { this, RE_route };
                                    for (int i = 0; i < RE_route.waypointDataList.Count; i++)
                                    {
                                        objList.Add(RE_route.waypointDataList[i]._waypoint);
                                    }
                                    Object[] objs = objList.ToArray();
                                    UndoRecordTargetObject(objs, "Clear All New Rout Points");
                                    RE_route.ClearAllNewRoutePoints();
                                }

                                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                                SerializedProperty _routeEditorProperty = serialObj.FindProperty("routeEditorProperty");
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(_routeEditorProperty, new GUIContent("Property"), true);
                                if (EditorGUI.EndChangeCheck())
                                    serialObj.ApplyModifiedProperties();

                                EditorGUILayout.Space();

                                SerializedObject routeObj = new SerializedObject(serialObj.FindProperty("RE_route").objectReferenceValue);
                                style.fontSize = 12;
                                style.alignment = TextAnchor.MiddleLeft;
                                for (int i = 0; i < RE_route.waypointDataList.Count; i++)
                                {
                                    SerializedObject routeWaypointObj = new SerializedObject(routeObj.FindProperty("waypointDataList").GetArrayElementAtIndex(i).FindPropertyRelative("_waypoint").objectReferenceValue);
                                    SerializedObject routeWaypointObject = new SerializedObject(RE_route.waypointDataList[i]._waypoint.gameObject);

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("WAYPOINT " + (i + 1).ToString(), style, GUILayout.Width(110));

                                    if (GUILayout.Button("Select", GUILayout.Width(45)))
                                    {
                                        UndoRecordTargetObject(this, "Select");
                                        Selection.activeObject = routeWaypointObj.targetObject;
                                        EditorGUIUtility.PingObject(routeWaypointObj.targetObject);
                                    }
                                    if (GUILayout.Button("Frame", GUILayout.Width(45)))
                                    {
                                        UndoRecordTargetObject(this, "Frame");
                                        Selection.activeObject = routeWaypointObj.targetObject;
                                        EditorGUIUtility.PingObject(routeWaypointObj.targetObject);
                                        SceneView.lastActiveSceneView.FrameSelected();
                                    }
                                    Color prevColor = GUI.color;
                                    GUI.color = Color.red;
                                    if (GUILayout.Button("Delete", GUILayout.Width(50)))
                                    {
                                        RE_GetPointsConnectedToRoutes();
                                        List<Object> objList = new List<Object> { this, RE_route };
                                        for (int j = 0; j < RE_connectedWaypoints.Count; j++)
                                        {
                                            objList.Add(RE_connectedWaypoints[j]);
                                        }
                                        Object[] objs = objList.ToArray();
                                        UndoRecordTargetObject(objs, "Delete Waypoint");
                                        Undo.RegisterFullObjectHierarchyUndo(RE_route.gameObject, "Delete Waypoint");
                                        Undo.DestroyObjectImmediate(routeWaypointObject.targetObject);//routeWaypointObj.targetObject);
                                        RE_route.waypointDataList.RemoveAt(i);
                                        for (int j = 0; j < RE_connectedWaypoints.Count; j++)
                                        {
                                            RE_connectedWaypoints[j].RemoveMissingLaneChangePoints();
                                            RE_connectedWaypoints[j].RemoveMissingNewRoutePoints();
                                        }
                                        RE_route.RefreshPointIndexes();
                                        // remove point from connection array
                                    }
                                    GUI.color = prevColor;
                                    EditorGUILayout.EndHorizontal();

                                    if (routeEditorProperty != RouteEditorProperty.ObjectOnly) EditorGUILayout.BeginVertical("box");
                                    /// Speed Limit
                                    if (routeEditorProperty == RouteEditorProperty.All || routeEditorProperty == RouteEditorProperty.SpeedLimit)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        SerializedProperty speedLimit = routeWaypointObj.FindProperty("onReachWaypointSettings").FindPropertyRelative("speedLimit");
                                        EditorGUILayout.PropertyField(speedLimit, true);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            routeWaypointObj.ApplyModifiedProperties();
                                        }
                                    }
                                    /// Stop Driving
                                    if (routeEditorProperty == RouteEditorProperty.All || routeEditorProperty == RouteEditorProperty.StopDriving)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        SerializedProperty stopDriving = routeWaypointObj.FindProperty("onReachWaypointSettings").FindPropertyRelative("stopDriving");
                                        EditorGUILayout.PropertyField(stopDriving, true);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            routeWaypointObj.ApplyModifiedProperties();
                                        }
                                    }
                                    /// New Route Point
                                    if (routeEditorProperty == RouteEditorProperty.All || routeEditorProperty == RouteEditorProperty.NewRoutePoints)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        SerializedProperty newRoutePoints = routeWaypointObj.FindProperty("onReachWaypointSettings").FindPropertyRelative("newRoutePoints");
                                        EditorGUILayout.PropertyField(newRoutePoints, true);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            routeWaypointObj.ApplyModifiedProperties();
                                        }
                                    }
                                    /// Lane Change Points
                                    if (routeEditorProperty == RouteEditorProperty.All || routeEditorProperty == RouteEditorProperty.LaneChangePoints)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        SerializedProperty laneChangePoints = routeWaypointObj.FindProperty("onReachWaypointSettings").FindPropertyRelative("laneChangePoints");
                                        EditorGUILayout.PropertyField(laneChangePoints, true);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            routeWaypointObj.ApplyModifiedProperties();
                                        }
                                    }
                                    /// On Reach Waypoint Event
                                    if (routeEditorProperty == RouteEditorProperty.All || routeEditorProperty == RouteEditorProperty.OnReachWaypointEvent)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        SerializedProperty OnReachWaypointEvent = routeWaypointObj.FindProperty("onReachWaypointSettings").FindPropertyRelative("OnReachWaypointEvent");
                                        EditorGUILayout.PropertyField(OnReachWaypointEvent, true);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            routeWaypointObj.ApplyModifiedProperties();
                                        }
                                    }
                                    if (routeEditorProperty != RouteEditorProperty.ObjectOnly) EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                }
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.SignalConnector:
                    #region SignalConnector GUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("SIGNAL CONNECTOR", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Lights & Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SC_lights = FindObjectsOfType<AITrafficLight>();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(40));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Lights & Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Lights & Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SC_lights = FindObjectsOfType<AITrafficLight>();
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        GUI.enabled = false;
                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);

                        SerializedProperty SC_lightsProperty = serialObj.FindProperty("SC_lights");
                        EditorGUILayout.PropertyField(SC_lightsProperty, true);

                        SerializedProperty SC_lightProperty = serialObj.FindProperty("SC_light");
                        EditorGUILayout.PropertyField(SC_lightProperty, true);

                        SerializedProperty SC_routeProperty = serialObj.FindProperty("SC_route");
                        EditorGUILayout.PropertyField(SC_routeProperty, true);

                        SerializedProperty SC_lightIndexProperty = serialObj.FindProperty("SC_lightIndex");
                        EditorGUILayout.PropertyField(SC_lightIndexProperty, true);

                        SerializedProperty SC_routeIndexProperty = serialObj.FindProperty("SC_routeIndex");
                        EditorGUILayout.PropertyField(SC_routeIndexProperty, true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (_routes.Length > 0)
                    {
                        SerializedProperty SC_lightProperty = serialObj.FindProperty("SC_light");
                        if (SC_lightIndex == -1)
                            EditorGUILayout.HelpBox("Press L handle button to select a Light", MessageType.Info);
                        GUI.enabled = false;
                        EditorGUILayout.PropertyField(SC_lightProperty, true);
                        GUI.enabled = true;

                        SerializedProperty SC_routeProperty = serialObj.FindProperty("SC_route");
                        if (SC_routeIndex == -1)
                            EditorGUILayout.HelpBox("Press R handle to select a Route", MessageType.Info);
                        GUI.enabled = false;
                        EditorGUILayout.PropertyField(SC_routeProperty, true);
                        GUI.enabled = true;

                        if (SC_light && SC_route)
                        {
                            if (SC_lights[SC_lightIndex].waypointRoute == _routes[SC_routeIndex])
                            {
                                if (GUILayout.Button("Disconnect Light From Route"))
                                {
                                    Object[] objs = new Object[] { this, SC_lights[SC_lightIndex] };
                                    UndoRecordTargetObject(objs, "Disconnect Light From Route");

                                    SC_lights[SC_lightIndex].waypointRoute = null;

                                    EditorUtility.SetDirty(SC_lights[SC_lightIndex]);
                                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                    ClearData(false);
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("Connect Light To Route"))
                                {
                                    Object[] objs = new Object[] { this, SC_lights[SC_lightIndex] };
                                    UndoRecordTargetObject(objs, "Connect Light To Route");

                                    SC_lights[SC_lightIndex].waypointRoute = _routes[SC_routeIndex];

                                    EditorUtility.SetDirty(SC_lights[SC_lightIndex]);
                                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                    ClearData(false);
                                }
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.SpawnPoints:
                    #region SpawnPoints GUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("SPAWN POINTS", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            STSPrefs.hideSpawnPointsInEditMode = false;
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(40));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            STSPrefs.hideSpawnPointsInEditMode = false;
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        GUI.enabled = false;
                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);



                    if (_routes.Length > 0)
                    {
                        if (GUILayout.Button("Align Route Waypoints"))
                        {
                            List<Object> objList = new List<Object>();
                            objList.Add(this);
                            for (int i = 0; i < _routes.Length; i++)
                            {
                                for (int j = 0; j < _routes[i].waypointDataList.Count; j++)
                                {
                                    objList.Add(_routes[i].waypointDataList[j]._transform);
                                }
                            }
                            Object[] objArray = objList.ToArray();
                            UndoRecordTargetObject(objArray, "Align Route Waypoints");
                            for (int i = 0; i < _routes.Length; i++)
                            {
                                _routes[i].AlignPoints();
                            }
                            EditorUtility.SetDirty(this);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                            Repaint();
                        }
                        if (GUILayout.Button("Remove All Spawn Points"))
                        {
                            for (int i = 0; i < _routes.Length; i++)
                            {
                                string message = "removing spawn point " + i.ToString() + "/" + _routes.Length.ToString();
                                EditorUtility.DisplayProgressBar("Remove All Spawn Points", message, i / (float)_routes.Length);
                                Undo.RegisterFullObjectHierarchyUndo(_routes[i].gameObject, "Remove All Spawn Points");
                                AITrafficSpawnPoint[] spawnPoints = _routes[i].GetComponentsInChildren<AITrafficSpawnPoint>();
                                for (int j = 0; j < spawnPoints.Length; j++)
                                {
                                    Undo.DestroyObjectImmediate(spawnPoints[j].gameObject);
                                }
                            }
                            EditorUtility.SetDirty(this);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                            Repaint();
                            EditorUtility.ClearProgressBar();
                        }
                        if (GUILayout.Button("Setup Random Spawn Points"))
                        {
                            GameObject waypointObject = STSRefs.AssetReferences._AITrafficSpawnPoint;
                            for (int i = 0; i < _routes.Length; i++)
                            {
                                string message = "updating route " + i.ToString() + "/" + _routes.Length.ToString();
                                EditorUtility.DisplayProgressBar("Setup Random Spawn Points", message, i / (float)_routes.Length);
                                if (_routes[i].waypointDataList.Count > 4)
                                {
                                    Undo.RegisterFullObjectHierarchyUndo(_routes[i].gameObject, "Setup Random Spawn Points");
                                    AITrafficSpawnPoint[] spawnPoints = _routes[i].GetComponentsInChildren<AITrafficSpawnPoint>();
                                    for (int j = 0; j < spawnPoints.Length; j++)
                                    {
                                        Undo.DestroyObjectImmediate(spawnPoints[j].gameObject);
                                    }

                                    int randomIndex = UnityEngine.Random.Range(0, 3);
                                    for (int j = randomIndex; j < _routes[i].waypointDataList.Count && j < _routes[i].waypointDataList.Count - 3; j += UnityEngine.Random.Range(2, 4))
                                    {
                                        GameObject loadedSpawnPoint = Instantiate(waypointObject, _routes[i].waypointDataList[j]._transform);
                                        Undo.RegisterCreatedObjectUndo(loadedSpawnPoint, "AITrafficSpawnPoint");
                                        AITrafficSpawnPoint trafficSpawnPoint = loadedSpawnPoint.GetComponent<AITrafficSpawnPoint>();
                                        trafficSpawnPoint.waypoint = trafficSpawnPoint.transform.parent.GetComponent<AITrafficWaypoint>();
                                    }
                                }
                            }
                            EditorUtility.ClearProgressBar();
                            Undo.FlushUndoRecordObjects();
                            EditorUtility.SetDirty(this);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                        EditorGUILayout.HelpBox("Press S handle buttons to create spwan points", MessageType.Info);
                    }
                    #endregion
                    break;
                case EditMode.StopConnector:
                    #region StopConnector GUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("STOP CONNECTOR", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Stops & Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            STC_stops = FindObjectsOfType<AITrafficStop>();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(40));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Stops & Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Stops & Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            STC_stops = FindObjectsOfType<AITrafficStop>();
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        GUI.enabled = false;
                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);

                        SerializedProperty STC_stopsProperty = serialObj.FindProperty("STC_stops");
                        EditorGUILayout.PropertyField(STC_stopsProperty, true);

                        SerializedProperty STC_stopProperty = serialObj.FindProperty("STC_stop");
                        EditorGUILayout.PropertyField(STC_stopProperty, true);

                        SerializedProperty STC_routeProperty = serialObj.FindProperty("STC_route");
                        EditorGUILayout.PropertyField(STC_routeProperty, true);

                        SerializedProperty STC_stopIndexProperty = serialObj.FindProperty("STC_stopIndex");
                        EditorGUILayout.PropertyField(STC_stopIndexProperty, true);

                        SerializedProperty STC_routeIndexProperty = serialObj.FindProperty("STC_routeIndex");
                        EditorGUILayout.PropertyField(STC_routeIndexProperty, true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (_routes.Length > 0)
                    {
                        SerializedProperty STC_stopProperty = serialObj.FindProperty("STC_stop");
                        if (STC_stopIndex == -1)
                            EditorGUILayout.HelpBox("Press L handle button to select a Stop", MessageType.Info);
                        GUI.enabled = false;
                        EditorGUILayout.PropertyField(STC_stopProperty, true);
                        GUI.enabled = true;

                        SerializedProperty STC_routeProperty = serialObj.FindProperty("STC_route");
                        if (STC_routeIndex == -1)
                            EditorGUILayout.HelpBox("Press R handle to select a Route", MessageType.Info);
                        GUI.enabled = false;
                        EditorGUILayout.PropertyField(STC_routeProperty, true);
                        GUI.enabled = true;

                        if (STC_stop && STC_route)
                        {
                            if (STC_stops[STC_stopIndex].waypointRoute == _routes[STC_routeIndex])
                            {
                                if (GUILayout.Button("Disconnect Stop From Route"))
                                {
                                    Object[] objs = new Object[] { this, STC_stops[STC_stopIndex] };
                                    UndoRecordTargetObject(objs, "Disconnect Stop From Route");

                                    STC_stops[STC_stopIndex].waypointRoute = null;

                                    EditorUtility.SetDirty(STC_stops[STC_stopIndex]);
                                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                    ClearData(false);
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("Connect Stop To Route"))
                                {
                                    Object[] objs = new Object[] { this, STC_stops[STC_stopIndex] };
                                    UndoRecordTargetObject(objs, "Connect Stop To Route");

                                    STC_stops[STC_stopIndex].waypointRoute = _routes[STC_routeIndex];

                                    EditorUtility.SetDirty(STC_stops[STC_stopIndex]);
                                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                    ClearData(false);
                                }
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.YieldTriggers:
                    #region YieldTriggers GUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("YIELD TRIGGERS", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            YT_routeInfoList = new List<AITrafficWaypointRouteInfo>();
                            AITrafficWaypointRouteInfo[] array = FindObjectsOfType<AITrafficWaypointRouteInfo>();
                            for (int i = 0; i < array.Length; i++)
                            {
                                if (array[i].yieldTrigger != null)
                                {
                                    YT_routeInfoList.Add(array[i]);
                                }
                            }
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(40));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            YT_routeInfoList = new List<AITrafficWaypointRouteInfo>();
                            AITrafficWaypointRouteInfo[] array = FindObjectsOfType<AITrafficWaypointRouteInfo>();
                            for (int i = 0; i < array.Length; i++)
                            {
                                if (array[i].yieldTrigger != null)
                                {
                                    YT_routeInfoList.Add(array[i]);
                                }
                            }
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        GUI.enabled = false;

                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);

                        SerializedProperty YT_routeInfoListProperty = serialObj.FindProperty("YT_routeInfoList");
                        EditorGUILayout.PropertyField(YT_routeInfoListProperty, true);

                        SerializedProperty YT_configureRouteIndexProperty = serialObj.FindProperty("YT_configureRouteIndex");
                        EditorGUILayout.PropertyField(YT_configureRouteIndexProperty, true);

                        SerializedProperty YT_configureWaypointIndexProperty = serialObj.FindProperty("YT_configureWaypointIndex");
                        EditorGUILayout.PropertyField(YT_configureWaypointIndexProperty, true);

                        SerializedProperty YT_routeInfoIndex = serialObj.FindProperty("YT_routeInfoIndex");
                        EditorGUILayout.PropertyField(YT_routeInfoIndex, true);

                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (_routes.Length > 0)
                    {
                        if (YT_configureRouteIndex == -1)
                            EditorGUILayout.HelpBox("Press R handle button to select a route", MessageType.Info);
                        GUI.enabled = false;
                        SerializedProperty YT_routeProperty = serialObj.FindProperty("YT_route");
                        EditorGUILayout.PropertyField(YT_routeProperty, true);
                        SerializedProperty YT_waypointProperty = serialObj.FindProperty("YT_waypoint");
                        EditorGUILayout.PropertyField(YT_waypointProperty, true);
                        SerializedProperty YT_routeInfoProperty = serialObj.FindProperty("YT_routeInfo");
                        EditorGUILayout.PropertyField(YT_routeInfoProperty, true);
                        GUI.enabled = true;

                        if (YT_waypoint != null && YT_routeInfo != null)
                        {
                            bool waypointContainsThisYieldTrigger = false;
                            int connectedIndex = 0;
                            for (int i = 0; i < YT_waypoint.onReachWaypointSettings.yieldTriggers.Count; i++)
                            {
                                Object objA = YT_waypoint.onReachWaypointSettings.yieldTriggers[i];
                                Object objB = YT_routeInfo;
                                if (ReferenceEquals(objA, objB))
                                {
                                    waypointContainsThisYieldTrigger = true;
                                    connectedIndex = i;
                                }
                            }
                            if (waypointContainsThisYieldTrigger)
                            {
                                if (GUILayout.Button("Remove Yield Trigger From Waypoint"))
                                {
                                    UndoRecordTargetObject(this, "Remove Yield Trigger From Waypoint");
                                    Undo.RegisterFullObjectHierarchyUndo(YT_waypoint, "Remove Yield Trigger From Waypoint");
                                    YT_waypoint.onReachWaypointSettings.yieldTriggers.RemoveAt(connectedIndex);
                                    YT_routeInfo = null;
                                    YT_routeInfoIndex = -1;
                                    SceneView.RepaintAll();
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("Assign Yield Trigger To Waypoint"))
                                {
                                    UndoRecordTargetObject(this, "Assign Yield Trigger To Waypoint");
                                    Undo.RegisterFullObjectHierarchyUndo(YT_waypoint, "Assign Yield Trigger To Waypoint");
                                    YT_waypoint.onReachWaypointSettings.yieldTriggers.Add(YT_routeInfo);
                                    YT_routeInfo = null;
                                    YT_routeInfoIndex = -1;
                                    SceneView.RepaintAll();
                                }
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.Cidy2:
                    #region CiDy GUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("CiDy 2 INTEGRATION", style);
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
#if CiDy
                    if (cidyGraph == null)
                    {
                        EditorGUILayout.HelpBox("Requires CiDy Graph to be in the scene.", MessageType.Info);
                        cidyGraph = GameObject.FindObjectOfType<CiDy.CiDyGraph>();
                    }
                    if (cidyGraph != null)
                    {
                        EditorGUILayout.HelpBox("Construct a road network with CiDy, then generate routes and lights.", MessageType.Info);
                        SerializedObject _serialObj_contentHolder = new SerializedObject(CiDy_STS_Generator.contentHolder);
                        if (GUILayout.Button("Generate"))
                        {
                            _serialObj_contentHolder.Update();
                            CiDy_STS_Generator.Generate();
                            EditorUtility.SetDirty(CiDy_STS_Generator.contentHolder);
                            _serialObj_contentHolder.ApplyModifiedProperties();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Clear"))
                        {
                            _serialObj_contentHolder.Update();
                            CiDy_STS_Generator.Clear();
                            EditorUtility.SetDirty(CiDy_STS_Generator.contentHolder);
                            _serialObj_contentHolder.ApplyModifiedProperties();
                            SceneView.RepaintAll();
                        }

                        EditorGUILayout.Space();

                        var headerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 14 };
                        EditorGUILayout.LabelField("CiDy Road Settings", headerStyle);

                        /// CiDy Road Options

                        SerializedProperty regenerateRoads = _serialObj_contentHolder.FindProperty("regenerateRoads");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(regenerateRoads, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Route Settings", headerStyle);

                        /// Options

                        SerializedProperty spawnPointsProperty = _serialObj_contentHolder.FindProperty("spawnPoints");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(spawnPointsProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        SerializedProperty carsSpawnedPerRouteProperty = _serialObj_contentHolder.FindProperty("carsSpawnedPerRoute");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(carsSpawnedPerRouteProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        EditorGUIUtility.wideMode = true;
                        SerializedProperty waypointSizeProperty = _serialObj_contentHolder.FindProperty("waypointSize");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(waypointSizeProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        /// Speed Limits

                        SerializedProperty defaultSpeedProperty = _serialObj_contentHolder.FindProperty("defaultSpeed");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(defaultSpeedProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        SerializedProperty intersectionSpeedProperty = _serialObj_contentHolder.FindProperty("intersectionSpeed");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(intersectionSpeedProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        SerializedProperty culDeSacSpeedProperty = _serialObj_contentHolder.FindProperty("culDeSacSpeed");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(culDeSacSpeedProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        /// Pooling

                        SerializedProperty maxDensityProperty = _serialObj_contentHolder.FindProperty("maxDensity");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(maxDensityProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Traffic Light Settings", headerStyle);

                        SerializedProperty greenTimerProperty = _serialObj_contentHolder.FindProperty("greenTimer");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(greenTimerProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        SerializedProperty yellowTimerProperty = _serialObj_contentHolder.FindProperty("yellowTimer");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(yellowTimerProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        SerializedProperty redTimerProperty = _serialObj_contentHolder.FindProperty("redTimer");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(redTimerProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Stop Sign Settings", headerStyle);

                        SerializedProperty activeTimeProperty = _serialObj_contentHolder.FindProperty("activeTime");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(activeTimeProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();

                        SerializedProperty waitTimeProperty = _serialObj_contentHolder.FindProperty("waitTime");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(waitTimeProperty, true);
                        if (EditorGUI.EndChangeCheck())
                            _serialObj_contentHolder.ApplyModifiedProperties();


                        EditorGUIUtility.wideMode = false;
                    }

#else
                    EditorGUILayout.HelpBox("Requires CiDy 2", MessageType.Info);
#endif

                    #endregion
                    break;
                default:
                    break;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        #endregion

        void RE_GetPointsConnectedToRoutes()
        {
            RE_connectedWaypoints = new List<AITrafficWaypoint>();
            for (int i = 0; i < _routes.Length; i++)
            {
                for (int j = 0; j < _routes[i].waypointDataList.Count; j++)
                {
                    for (int k = 0; k < _routes[i].waypointDataList[j]._waypoint.onReachWaypointSettings.newRoutePoints.Length; k++)
                    {
                        if (_routes[i].waypointDataList[j]._waypoint.onReachWaypointSettings.newRoutePoints[k] != null)
                        {
                            if (_routes[i].waypointDataList[j]._waypoint.onReachWaypointSettings.newRoutePoints[k].onReachWaypointSettings.parentRoute == RE_route)
                            {
                                RE_connectedWaypoints.Add(_routes[i].waypointDataList[j]._waypoint);
                            }
                        }
                    }
                    for (int k = 0; k < _routes[i].waypointDataList[j]._waypoint.onReachWaypointSettings.laneChangePoints.Count; k++)
                    {
                        if (_routes[i].waypointDataList[j]._waypoint.onReachWaypointSettings.laneChangePoints[k].onReachWaypointSettings.parentRoute == RE_route)
                        {
                            RE_connectedWaypoints.Add(_routes[i].waypointDataList[j]._waypoint);
                        }

                    }
                }
            }
        }

        #region SceneView GUI
        void OnFocus()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
            Undo.undoRedoPerformed -= this.OnUndoRedo;
            Undo.undoRedoPerformed += this.OnUndoRedo;
            EditorSceneManager.activeSceneChangedInEditMode -= SceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += SceneChanged;
            EditorApplication.playModeStateChanged -= PlayModeStateChangedCallback;
            EditorApplication.playModeStateChanged += PlayModeStateChangedCallback;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
            Undo.undoRedoPerformed -= this.OnUndoRedo;
            EditorSceneManager.activeSceneChangedInEditMode -= SceneChanged;
            EditorApplication.playModeStateChanged -= PlayModeStateChangedCallback;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Transform sceneViewCameraTransform = Camera.current.transform;
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 12;

            switch (editMode)
            {
                case EditMode.LaneConnector:
                    #region LaneConnector
                    for (int i = 0; i < this._routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (this._routes[i].waypointDataList.Count > 0)
                        {
                            int index = this._routes[i].waypointDataList.Count - 1;
                            Vector3 pointTransformPosition = this._routes[i].waypointDataList[index]._transform.position;
                            float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                            float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                            Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                            float size = linearPointDistance * handleSize;
                            if (STSPrefs.drawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                            {
                                if ((LC_routeIndexA == i || LC_routeIndexB == i) || (LC_routeIndexA == -1 || LC_routeIndexB == -1)) // if index is selected // if 1 or more indexes are not assigned
                                {
                                    style.normal.textColor = (this.LC_routeIndexA == i || this.LC_routeIndexB == i) ? STSPrefs.handleTextSelectedColor : STSPrefs.handleTextColor;
                                    Handles.color = (this.LC_routeIndexA == i || this.LC_routeIndexB == i) ? STSPrefs.handleSelectedColor : STSPrefs.handleColor;
                                    if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                    {
                                        UndoRecordTargetObject(this, "Lane Connector Handle");
                                        bool updatedSelection = false;
                                        if (this.LC_routeIndexA == i) /// Check if selection is already assigned to A
                                        {
                                            this.LC_routeA = null;
                                            LC_routeIndexA = -1;
                                            updatedSelection = true;
                                        }
                                        if (this.LC_routeIndexB == i) /// Check if selection is already assigned to B
                                        {
                                            this.LC_routeB = null;
                                            LC_routeIndexB = -1;
                                            updatedSelection = true;
                                        }
                                        if (updatedSelection == false) /// Assign selection
                                        {
                                            if (this.LC_routeIndexA == -1)
                                            {
                                                this.LC_routeIndexA = i;
                                                this.LC_routeA = this._routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute;
                                            }
                                            else if (this.LC_routeIndexB == -1)
                                            {
                                                this.LC_routeIndexB = i;
                                                this.LC_routeB = this._routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute;
                                            }
                                        }
                                        Repaint();
                                    }
                                    Handles.Label(_textPosition, "R", style);
                                }
                            }
                        }
                    }
                    if (this.LC_routeIndexA != -1 && this.LC_routeIndexB != -1) /// Draw Line
                    {
                        Handles.color = STSPrefs.connectionColor;
                        int indexA = this._routes[LC_routeIndexA].waypointDataList.Count - 1;
                        int indexB = this._routes[LC_routeIndexB].waypointDataList.Count - 1;
                        Vector3 positionA = this._routes[LC_routeIndexA].waypointDataList[indexA]._transform.position + new Vector3(0, 0.2f, 0);
                        Vector3 positionB = this._routes[LC_routeIndexB].waypointDataList[indexB]._transform.position + new Vector3(0, 0.2f, 0);
                        Handles.DrawLine(positionA, positionB);
                    }
                    #endregion
                    break;
                case EditMode.RouteConnector:
                    #region RouteConnector
                    for (int i = 0; i < _routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        for (int j = 0; j < _routes[i].waypointDataList.Count; j++)
                        {
                            Vector3 pointTransformPosition = _routes[i].waypointDataList[j]._transform.position;
                            float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                            float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                            Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                            float size = linearPointDistance * handleSize;
                            if (STSPrefs.drawDistance >= pointDistance)
                            {
                                #region TO Handle
                                if (RC_toPointIndex == -1 || RC_toRouteIndex == i && RC_toPointIndex == j)
                                {
                                    if (RC_fromRouteIndex != i && RC_fromPointIndex != j)
                                    {
                                        style.normal.textColor = (RC_toRouteIndex == i) && (RC_toPointIndex == j) ? STSPrefs.handleTextSelectedColor : STSPrefs.handleTextColor;
                                        Handles.color = (RC_toRouteIndex == i) && (RC_toPointIndex == j) ? STSPrefs.handleSelectedColor : STSPrefs.handleColor;
                                        if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                        {
                                            UndoRecordTargetObject(this, "To Handle");
                                            if (RC_toRouteIndex == i && RC_toPointIndex == j)
                                            {
                                                RC_toRouteIndex = -1;
                                                RC_toPointIndex = -1;
                                                RC_toPoint = null;
                                            }
                                            else
                                            {
                                                RC_toRouteIndex = i;
                                                RC_toPointIndex = j;
                                                RC_toPoint = _routes[i].waypointDataList[j]._waypoint;
                                            }
                                            Repaint();
                                        }
                                        Handles.Label(_textPosition, "T", style);
                                    }
                                }
                                #endregion

                                #region FROM Handle
                                if (RC_fromPointIndex == -1 || (RC_fromRouteIndex == i && RC_fromPointIndex == j))
                                {
                                    if (RC_toRouteIndex != i && RC_toPointIndex != j)
                                    {
                                        pointTransformPosition = _routes[i].waypointDataList[j]._transform.position + new Vector3(0, (2.75f * linearPointDistance), 0);
                                        _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                                        style.normal.textColor = (RC_fromRouteIndex == i) && (RC_fromPointIndex == j) ? STSPrefs.handleTextSelectedColor : STSPrefs.handleTextColor;
                                        Handles.color = (RC_fromRouteIndex == i) && (RC_fromPointIndex == j) ? STSPrefs.handleSelectedColor : STSPrefs.handleColor;
                                        if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                        {
                                            UndoRecordTargetObject(this, "From Handle");
                                            if (RC_fromRouteIndex == i && RC_fromPointIndex == j)
                                            {
                                                RC_fromRouteIndex = -1;
                                                RC_fromPointIndex = -1;
                                                RC_fromPoint = null;
                                            }
                                            else
                                            {
                                                RC_fromRouteIndex = i;
                                                RC_fromPointIndex = j;
                                                RC_fromPoint = _routes[i].waypointDataList[j]._waypoint;
                                            }
                                            Repaint();
                                        }
                                        Handles.Label(_textPosition, "F", style);
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.RouteEditor:
                    #region RouteEditor
                    for (int i = 0; i < _routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (_routes[i].waypointDataList.Count > 0)
                        {
                            int index = _routes[i].waypointDataList.Count - 1;
                            Vector3 pointTransformPosition = _routes[i].waypointDataList[index]._transform.position;
                            float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                            float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                            Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                            float size = linearPointDistance * handleSize;
                            if (STSPrefs.drawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                            {
                                if (RE_routeIndex == i || RE_routeIndex == -1) // if index is selected // if index is not assigned
                                {
                                    style.normal.textColor = RE_routeIndex == i ? STSPrefs.handleTextSelectedColor : STSPrefs.handleTextColor;
                                    Handles.color = RE_routeIndex == i ? STSPrefs.handleSelectedColor : STSPrefs.handleColor;
                                    if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                    {
                                        UndoRecordTargetObject(this, "Route Editor Handle");
                                        bool updatedSelection = false;
                                        if (RE_routeIndex == i) /// Check if selection is already assigned to A
                                        {
                                            RE_route = null;
                                            RE_routeIndex = -1;
                                            updatedSelection = true;
                                        }
                                        if (updatedSelection == false) /// Assign selection
                                        {
                                            if (RE_routeIndex == -1)
                                            {
                                                RE_routeIndex = i;
                                                RE_route = _routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute;
                                            }
                                        }
                                        Repaint();
                                    }
                                    Handles.Label(_textPosition, "R", style);
                                }
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.SignalConnector:
                    #region SignalConnector
                    #region Route Handle
                    for (int i = 0; i < _routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (_routes[i].waypointDataList.Count > 0)
                        {
                            if (SC_routeIndex == -1 || SC_routeIndex == i)
                            {
                                int index = _routes[i].waypointDataList.Count - 1;
                                Vector3 pointTransformPosition = _routes[i].waypointDataList[index]._transform.position;
                                float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                                float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                                Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                                float size = linearPointDistance * handleSize;
                                if (STSPrefs.drawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                                {
                                    style.normal.textColor = (SC_routeIndex == i) ? STSPrefs.handleTextSelectedColor : STSPrefs.handleTextColor;
                                    Handles.color = (SC_routeIndex == i) ? STSPrefs.handleSelectedColor : STSPrefs.handleColor;
                                    if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                    {
                                        UndoRecordTargetObject(this, "Signal Connector Route Handle");
                                        if (SC_routeIndex == i)
                                        {
                                            SC_routeIndex = -1;
                                            SC_route = null;
                                        }
                                        else
                                        {
                                            SC_routeIndex = i;
                                            SC_route = _routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute;
                                        }
                                        Repaint();
                                    }
                                    Handles.Label(_textPosition, "R", style);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Light Handle
                    for (int i = 0; i < SC_lights.Length; i++)
                    {
                        if (SC_lights[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (SC_lightIndex == -1 || SC_lightIndex == i)
                        {
                            Vector3 pointTransformPosition = SC_lights[i].transform.position;
                            float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                            float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                            Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                            float size = linearPointDistance * handleSize;
                            if (STSPrefs.drawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                            {
                                style.normal.textColor = (SC_lightIndex == i) ? STSPrefs.handleTextSelectedColor : STSPrefs.handleTextColor;
                                Handles.color = (SC_lightIndex == i) ? STSPrefs.handleSelectedColor : STSPrefs.handleColor;
                                if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                {
                                    UndoRecordTargetObject(this, "Signal Connector Light Handle");
                                    if (SC_lightIndex == i)
                                    {
                                        SC_lightIndex = -1;
                                        SC_light = null;
                                    }
                                    else
                                    {
                                        SC_lightIndex = i;
                                        SC_light = SC_lights[i];
                                    }
                                    Repaint();
                                }
                                Handles.Label(_textPosition, "L", style);
                            }
                        }
                    }
                    #endregion

                    #region Line Handle
                    for (int i = 0; i < SC_lights.Length; i++)
                    {
                        if (SC_lights[i].waypointRoute != null)
                        {
                            int index = SC_lights[i].waypointRoute.waypointDataList.Count - 1;
                            Handles.color = STSPrefs.connectionColor;
                            Handles.DrawLine(SC_lights[i].transform.position, SC_lights[i].waypointRoute.waypointDataList[index]._transform.position);
                        }
                    }
                    #endregion

                    #endregion
                    break;
                case EditMode.SpawnPoints:
                    #region SpawnPoints
                    style.normal.textColor = STSPrefs.handleTextColor;
                    Handles.color = STSPrefs.handleColor;
                    for (int i = 0; i < _routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (_routes[i].waypointDataList.Count > 2) // ignore final 2 waypoints
                        {
                            for (int j = 0; j < _routes[i].waypointDataList.Count - 2; j++)
                            {
                                Vector3 pointTransformPosition = _routes[i].waypointDataList[j]._transform.position;
                                float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                                float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                                Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                                float size = linearPointDistance * handleSize;
                                if (STSPrefs.drawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                                {
                                    if (_routes[i].waypointDataList[j]._transform.childCount == 0)
                                    {
                                        if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                        {
                                            UndoRecordTargetObject(this, "Spawn Point Handle");
                                            GameObject loadedSpawnPoint = Instantiate(STSRefs.AssetReferences._AITrafficSpawnPoint, _routes[i].waypointDataList[j]._transform);
                                            Undo.RegisterCreatedObjectUndo(loadedSpawnPoint, "Create Spawn Point");

                                            AITrafficSpawnPoint trafficSpawnPoint = loadedSpawnPoint.GetComponent<AITrafficSpawnPoint>();
                                            trafficSpawnPoint.waypoint = trafficSpawnPoint.transform.parent.GetComponent<AITrafficWaypoint>();

                                            GameObject[] newSelection = new GameObject[1];
                                            newSelection[0] = loadedSpawnPoint;
                                            Selection.objects = newSelection;

                                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                            Repaint();
                                        }
                                        Handles.Label(_textPosition, "S", style);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.StopConnector:
                    #region StopConnector
                    #region Route Handle
                    for (int i = 0; i < _routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (_routes[i].waypointDataList.Count > 0)
                        {
                            if (STC_routeIndex == -1 || STC_routeIndex == i)
                            {
                                int index = _routes[i].waypointDataList.Count - 1;
                                Vector3 pointTransformPosition = _routes[i].waypointDataList[index]._transform.position;
                                float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                                float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                                Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                                float size = linearPointDistance * handleSize;
                                if (STSPrefs.drawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                                {
                                    style.normal.textColor = (STC_routeIndex == i) ? STSPrefs.handleTextSelectedColor : STSPrefs.handleTextColor;
                                    Handles.color = (STC_routeIndex == i) ? STSPrefs.handleSelectedColor : STSPrefs.handleColor;
                                    if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                    {
                                        UndoRecordTargetObject(this, "Stop Connector Route Handle");
                                        if (STC_routeIndex == i)
                                        {
                                            STC_routeIndex = -1;
                                            STC_route = null;
                                        }
                                        else
                                        {
                                            STC_routeIndex = i;
                                            STC_route = _routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute;
                                        }
                                        Repaint();
                                    }
                                    Handles.Label(_textPosition, "R", style);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Stop Handle
                    for (int i = 0; i < STC_stops.Length; i++)
                    {
                        if (STC_stops[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (STC_stopIndex == -1 || STC_stopIndex == i)
                        {
                            Vector3 pointTransformPosition = STC_stops[i].transform.position;
                            float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                            float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                            Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                            float size = linearPointDistance * handleSize;
                            if (STSPrefs.drawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                            {
                                style.normal.textColor = (STC_stopIndex == i) ? STSPrefs.handleTextSelectedColor : STSPrefs.handleTextColor;
                                Handles.color = (STC_stopIndex == i) ? STSPrefs.handleSelectedColor : STSPrefs.handleColor;
                                if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                {
                                    UndoRecordTargetObject(this, "Stop Connector Stop Handle");
                                    if (STC_stopIndex == i)
                                    {
                                        STC_stopIndex = -1;
                                        STC_stop = null;
                                    }
                                    else
                                    {
                                        STC_stopIndex = i;
                                        STC_stop = STC_stops[i];
                                    }
                                    Repaint();
                                }
                                Handles.Label(_textPosition, "S", style);
                            }
                        }
                    }
                    #endregion

                    #region Line Handle
                    for (int i = 0; i < STC_stops.Length; i++)
                    {
                        if (STC_stops[i].waypointRoute != null)
                        {
                            int index = STC_stops[i].waypointRoute.waypointDataList.Count - 1;
                            Handles.color = STSPrefs.connectionColor;
                            Handles.DrawLine(STC_stops[i].transform.position, STC_stops[i].waypointRoute.waypointDataList[index]._transform.position);
                        }
                    }
                    #endregion

                    #endregion
                    break;
                case EditMode.YieldTriggers:
                    #region YieldTriggers
                    for (int i = 0; i < _routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (_routes[i].waypointDataList.Count > 0)
                        {
                            int index = _routes[i].waypointDataList.Count - 1;
                            Vector3 pointTransformPosition = _routes[i].waypointDataList[index]._transform.position;
                            float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                            float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                            Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                            float size = linearPointDistance * handleSize;
                            if (STSPrefs.drawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                            {
                                if (YT_configureRouteIndex == i || YT_configureRouteIndex == -1) // if index is selected // if 1 or more indexes are not assigned
                                {
                                    style.normal.textColor = YT_configureRouteIndex == i ? STSPrefs.handleTextSelectedColor : STSPrefs.handleTextColor;
                                    Handles.color = YT_configureRouteIndex == i ? STSPrefs.handleSelectedColor : STSPrefs.handleColor;
                                    if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                    {
                                        UndoRecordTargetObject(this, "Yield Trigger Route Handle");
                                        bool updatedSelection = false;
                                        if (YT_configureRouteIndex == i) /// Check if selection is already assigned to A
                                        {
                                            YT_route = null;
                                            YT_configureRouteIndex = -1;
                                            YT_waypoint = null;
                                            YT_configureWaypointIndex = -1;
                                            YT_routeInfo = null;
                                            YT_routeInfoIndex = -1;
                                            updatedSelection = true;
                                        }
                                        if (updatedSelection == false) /// Assign selection
                                        {
                                            if (YT_configureRouteIndex == -1)
                                            {
                                                YT_configureRouteIndex = i;
                                                YT_route = _routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute;
                                            }
                                        }
                                        Repaint();
                                    }
                                    Handles.Label(_textPosition, "R", style);

                                    if (YT_configureRouteIndex == i)
                                    {
                                        for (int j = 0; j < _routes[i].waypointDataList.Count - 1; j++)
                                        {
                                            if (YT_configureWaypointIndex == j || YT_configureWaypointIndex == -1)
                                            {
                                                style.normal.textColor = YT_configureWaypointIndex == j ? STSPrefs.handleTextSelectedColor : STSPrefs.handleTextColor;
                                                Handles.color = YT_configureWaypointIndex == j ? STSPrefs.handleSelectedColor : STSPrefs.handleColor;
                                                pointTransformPosition = _routes[i].waypointDataList[j]._transform.position;
                                                pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                                                linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                                                _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                                                size = linearPointDistance * handleSize;
                                                if (STSPrefs.drawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                                                {
                                                    if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                                    {
                                                        UndoRecordTargetObject(this, "Yield Trigger WayPoint Handle");
                                                        bool updatedSelection = false;
                                                        if (YT_configureWaypointIndex == j) /// Check if selection is already assigned to A
                                                        {
                                                            YT_waypoint = null;
                                                            YT_configureWaypointIndex = -1;
                                                            YT_routeInfo = null;
                                                            YT_routeInfoIndex = -1;
                                                            updatedSelection = true;
                                                        }
                                                        if (updatedSelection == false) /// Assign selection
                                                        {
                                                            if (YT_configureWaypointIndex == -1)
                                                            {
                                                                YT_configureWaypointIndex = j;
                                                                YT_waypoint = _routes[i].waypointDataList[j]._waypoint;
                                                            }
                                                        }
                                                        Repaint();
                                                    }
                                                    Handles.Label(_textPosition, "WP", style);
                                                    //}
                                                }
                                            }
                                        }
                                    }


                                }
                            }
                        }
                    }
                    if (YT_route != null && YT_waypoint != null)
                    {
                        for (int i = 0; i < _routes.Length; i++)
                        {
                            if (_routes[i].waypointDataList.Count > 0)
                            {
                                int index = _routes[i].waypointDataList.Count - 1;
                                if (
                                    (YT_routeInfoIndex == -1 || YT_routeInfoIndex == i) &&
                                    _routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute.GetComponent<AITrafficWaypointRouteInfo>().yieldTrigger != null
                                    )
                                {
                                    Vector3 pointTransformPosition = _routes[i].waypointDataList[index]._transform.position;
                                    float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                                    float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                                    Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                                    float size = linearPointDistance * handleSize;
                                    if (STSPrefs.drawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                                    {
                                        style.normal.textColor = (YT_routeInfoIndex == i) ? STSPrefs.handleTextSelectedColor : STSPrefs.handleTextColor;
                                        Handles.color = (YT_routeInfoIndex == i) ? STSPrefs.handleSelectedColor : STSPrefs.handleColor;
                                        if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                        {
                                            UndoRecordTargetObject(this, "Yield Trigger Route Info Handle");
                                            if (YT_routeInfoIndex == i)
                                            {
                                                YT_routeInfoIndex = -1;
                                                YT_routeInfo = null;
                                            }
                                            else
                                            {
                                                YT_routeInfoIndex = i;
                                                YT_routeInfo = _routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute.GetComponent<AITrafficWaypointRouteInfo>();
                                            }
                                            Repaint();
                                        }
                                        Handles.Label(_textPosition, "Y", style);
                                    }
                                }
                            }
                        }
                    }
                    #region Line Handle
                    if (YT_route != null)
                    {
                        Handles.color = STSPrefs.connectionColor;
                        for (int i = 0; i < YT_route.waypointDataList.Count; i++)
                        {
                            for (int j = 0; j < YT_route.waypointDataList[i]._waypoint.onReachWaypointSettings.yieldTriggers.Count; j++)
                            {
                                Handles.DrawLine(
                                YT_route.waypointDataList[i]._transform.position,
                                YT_route.waypointDataList[i]._waypoint.onReachWaypointSettings.yieldTriggers[j].yieldTrigger.transform.position

                                );
                            }
                        }
                    }
                    #endregion
                    #endregion
                    break;
            }
            SceneView.RepaintAll();
        }
        #endregion

        #region UndoRecordThisObject
        void UndoRecordTargetObject(Object targetObject, string actionName)
        {
            Undo.RegisterCompleteObjectUndo(targetObject, "STS Window Action: " + actionName);
            Undo.FlushUndoRecordObjects();
        }

        void UndoRecordTargetObject(Object[] targetObjects, string actionName)
        {
            Undo.RegisterCompleteObjectUndo(targetObjects, "STS Window Action: " + actionName);
            Undo.FlushUndoRecordObjects();
        }

        void OnUndoRedo()
        {
            requireRepaint = true;
        }
        #endregion

        #region SaveEditorPreferences
        void SaveColor(string _colorPropertyName, Color _color)
        {
            EditorPrefs.SetFloat("STS_" + _colorPropertyName + "_R", _color.r);
            EditorPrefs.SetFloat("STS_" + _colorPropertyName + "_G", _color.g);
            EditorPrefs.SetFloat("STS_" + _colorPropertyName + "_B", _color.b);
            EditorPrefs.SetFloat("STS_" + _colorPropertyName + "_A", _color.a);
        }

        public Color GetSavedColor(string _colorPropertyName, Color _defaultColor)
        {
            Color savedColor = new Color
                (
                 EditorPrefs.GetFloat("STS_" + _colorPropertyName + "_R", _defaultColor.r),
                 EditorPrefs.GetFloat("STS_" + _colorPropertyName + "_G", _defaultColor.g),
                 EditorPrefs.GetFloat("STS_" + _colorPropertyName + "_B", _defaultColor.b),
                 EditorPrefs.GetFloat("STS_" + _colorPropertyName + "_A", _defaultColor.a)
                );
            return savedColor;
        }
        #endregion

        void SceneChanged(Scene scene1, Scene scene2)
        {
            Debug.Log("STS Tools Window: A new scene was loaded, unloading window data");
            ClearData(true);
        }

        void PlayModeStateChangedCallback(PlayModeStateChange state)
        {
            Debug.Log("STS Tools Window: Edit/Play mode state has changed, unloading window data");
            ClearData(true);
        }

        void ClearData(bool clearRoutes)
        {
            if (clearRoutes)
            {
                _routes = new AITrafficWaypointRoute[0];
                SC_lights = new AITrafficLight[0];
                STC_stops = new AITrafficStop[0];
            }
            SC_routeIndex = -1;
            SC_lightIndex = -1;
            SC_light = null;
            SC_route = null;
            STC_routeIndex = -1;
            STC_stopIndex = -1;
            STC_stop = null;
            STC_route = null;
            RC_fromRouteIndex = -1;
            RC_fromPointIndex = -1;
            RC_toRouteIndex = -1;
            RC_toPointIndex = -1;
            RC_fromPoint = null;
            RC_toPoint = null;
            LC_routeIndexA = -1;
            LC_routeIndexB = -1;
            LC_routeA = null;
            LC_routeB = null;
            RE_route = null;
            RE_routeIndex = -1;
            YT_route = null;
            YT_waypoint = null;
            YT_routeInfo = null;
            YT_routeInfoList = new List<AITrafficWaypointRouteInfo>();
            YT_configureRouteIndex = -1;
            YT_configureWaypointIndex = -1;
            YT_routeInfoIndex = -1;
            EditorUtility.SetDirty(this);
            Repaint();
        }

        void AssignLaneChangePoints(AITrafficWaypointRoute getFrom, AITrafficWaypointRoute assignTo)
        {
            for (int i = 2; i < assignTo.waypointDataList.Count; i++) // skip first 2 waypoints
            {
                if (i < assignTo.waypointDataList.Count - 3) // skip last 3 waypoints
                {
                    if (getFrom.waypointDataList.Count > i + 1)
                    {
                        assignTo.waypointDataList[i]._waypoint.onReachWaypointSettings.laneChangePoints.Add
                            (getFrom.waypointDataList[i + 1]._waypoint);
                    }
                }
            }
        }

    }
}