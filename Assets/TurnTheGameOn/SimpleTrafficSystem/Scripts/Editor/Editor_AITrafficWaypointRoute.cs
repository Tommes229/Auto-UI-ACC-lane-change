namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using System.Collections.Generic;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(AITrafficWaypointRoute))]
    public class Editor_AITrafficWaypointRoute : Editor
    {
        AITrafficWaypointRoute circuit;

        void OnEnable()
        {
            circuit = (AITrafficWaypointRoute)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
            EditorStyles.label.wordWrap = true;

            EditorGUILayout.HelpBox("Shift + Left Click in scene view on a Collider to add new points to the route", MessageType.None);
            EditorGUILayout.HelpBox("Shift + Ctrl + Left Click in scene view on a Collider to insert new points to the route", MessageType.None);
            EditorGUILayout.HelpBox("Shift + Right Click in scene view on a Collider to add a Yield Trigger to the route", MessageType.None);

            AITrafficWaypointRoute _AITrafficWaypointRoute = (AITrafficWaypointRoute)target;

            EditorGUILayout.BeginVertical("Box");

            SerializedProperty useSpawnPoints = serializedObject.FindProperty("useSpawnPoints");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(useSpawnPoints, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            SerializedProperty spawnFromAITrafficController = serializedObject.FindProperty("spawnFromAITrafficController");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(spawnFromAITrafficController, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            if (_AITrafficWaypointRoute.spawnFromAITrafficController)
            {
                SerializedProperty spawnAmount = serializedObject.FindProperty("spawnAmount");
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(spawnAmount, true);
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
            }

            SerializedProperty spawnTrafficVehicles = serializedObject.FindProperty("spawnTrafficVehicles");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(spawnTrafficVehicles, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("Box");

            SerializedProperty maxDensity = serializedObject.FindProperty("maxDensity");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(maxDensity, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            SerializedProperty vehicleTypes = serializedObject.FindProperty("vehicleTypes");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(vehicleTypes, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("Box");

            SerializedProperty waypointDataList = serializedObject.FindProperty("waypointDataList");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(waypointDataList, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("Reverse Waypoints", "Reverses all waypoints in the route's waypointDataList.")))
            {
                circuit.ReversePoints();
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            if (GUILayout.Button(new GUIContent("Align Waypoints", "Aligns the rotation of all waypoints to face toward the next point.")))
            {
                circuit.AlignPoints();
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            if (GUILayout.Button(new GUIContent("Setup Random Spawn Points", "First removes all spawn points, then randomly adds new spawn points.")))
            {
                
                if (circuit.waypointDataList.Count > 4)
                {
                    Undo.RegisterFullObjectHierarchyUndo(circuit.gameObject, "Remove All Spawn Points");
                    AITrafficSpawnPoint[] spawnPoints = circuit.GetComponentsInChildren<AITrafficSpawnPoint>();
                    for (int i = 0; i < spawnPoints.Length; i++)
                    {
                        string message = "removing old spawn point " + i.ToString() + "/" + spawnPoints.Length.ToString();
                        EditorUtility.DisplayProgressBar("Setup Random Spawn Points", message, i / (float)spawnPoints.Length);
                        Undo.DestroyObjectImmediate(spawnPoints[i].gameObject);
                    }
                    int randomIndex = UnityEngine.Random.Range(0, 3);
                    for (int i = randomIndex; i < circuit.waypointDataList.Count && i < circuit.waypointDataList.Count - 2; i += UnityEngine.Random.Range(2, 4))
                    {
                        string message = "updating route point " + i.ToString() + "/" + circuit.waypointDataList.Count.ToString();
                        EditorUtility.DisplayProgressBar("Setup Random Spawn Points", message, i / (float)circuit.waypointDataList.Count);
                        GameObject loadedSpawnPoint = Instantiate(STSRefs.AssetReferences._AITrafficSpawnPoint, circuit.waypointDataList[i]._transform) as GameObject;
                        Undo.RegisterCreatedObjectUndo(loadedSpawnPoint, "AITrafficSpawnPoint");
                        AITrafficSpawnPoint trafficSpawnPoint = loadedSpawnPoint.GetComponent<AITrafficSpawnPoint>();
                        trafficSpawnPoint.waypoint = trafficSpawnPoint.transform.parent.GetComponent<AITrafficWaypoint>();
                    }
                    Undo.FlushUndoRecordObjects();
                    EditorUtility.SetDirty(this);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    EditorUtility.ClearProgressBar();
                }
            }

            if (GUILayout.Button(new GUIContent("Remove All Spawn Points", "Removes all spawn points from the route.")))
            {
                Undo.RegisterFullObjectHierarchyUndo(circuit.gameObject, "Remove All Spawn Points");
                AITrafficSpawnPoint[] spawnPoints = circuit.GetComponentsInChildren<AITrafficSpawnPoint>();
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    string message = "removing spawn point " + i.ToString() + "/" + spawnPoints.Length.ToString();
                    EditorUtility.DisplayProgressBar("Remove All Spawn Points", message, i / (float)spawnPoints.Length);
                    Undo.DestroyObjectImmediate(spawnPoints[i].gameObject);
                }
                Undo.FlushUndoRecordObjects();
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorUtility.ClearProgressBar();
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        void OnSceneGUI()
        {
            AITrafficWaypointRoute _AITrafficWaypointRoute = (AITrafficWaypointRoute)target;

            for (int i = 0; i < _AITrafficWaypointRoute.waypointDataList.Count; i++)
            {
                if (_AITrafficWaypointRoute.waypointDataList[i]._waypoint)
                {
                    GUIStyle style = new GUIStyle();
                    string target = "";
                    style.normal.textColor = Color.green;
                    Handles.Label(_AITrafficWaypointRoute.waypointDataList[i]._waypoint.transform.position + new Vector3(0, 0.25f, 0),
                    "    Waypoint:   " + _AITrafficWaypointRoute.waypointDataList[i]._waypoint.onReachWaypointSettings.waypointIndexnumber.ToString() + "\n" +
                    "    SpeedLimit: " + _AITrafficWaypointRoute.waypointDataList[i]._waypoint.onReachWaypointSettings.speedLimit + "\n" +
                    target,
                    style
                    );
                }
            }

            Event e = Event.current;
            if (e.shift)
            {
                if (e.control)
                {
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        int controlId = GUIUtility.GetControlID(FocusType.Passive);
                        Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                        RaycastHit hitInfo;
                        if (Physics.Raycast(worldRay, out hitInfo))
                        {
                            ClickToInsertSpawnNextWaypoint(_AITrafficWaypointRoute, hitInfo.point);
                        }
                        GUIUtility.hotControl = controlId;
                        e.Use();
                    }
                }
                else if (e.type == EventType.MouseDown && e.button == 1)
                {
                    int controlId = GUIUtility.GetControlID(FocusType.Passive);
                    Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(worldRay, out hitInfo))
                    {
                        ClickToSpawnYieldTrigger(_AITrafficWaypointRoute, hitInfo.point);
                    }
                    GUIUtility.hotControl = controlId;
                    e.Use();
                }
                else if (e.type == EventType.MouseDown && e.button == 0)
                {
                    int controlId = GUIUtility.GetControlID(FocusType.Passive);
                    Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(worldRay, out hitInfo))
                    {
                        ClickToSpawnNextWaypoint(_AITrafficWaypointRoute, hitInfo.point);
                    }
                    GUIUtility.hotControl = controlId;
                    e.Use();
                }
            }
        }

        public Transform ClickToSpawnYieldTrigger(AITrafficWaypointRoute __AITrafficWaypointRoute, Vector3 _position)
        {
            AITrafficWaypointRouteInfo routeInfo = __AITrafficWaypointRoute.GetComponent<AITrafficWaypointRouteInfo>();
            if (routeInfo.yieldTrigger == null)
            {
                // this needs to spawn a yield trigger
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("ClickToSpawnYieldTrigger");
                var undoGroupIndex = Undo.GetCurrentGroup();
                Undo.RegisterCompleteObjectUndo(__AITrafficWaypointRoute, "ClickToSpawnYieldTrigger");
                Undo.RegisterCompleteObjectUndo(routeInfo, "ClickToSpawnYieldTrigger");
                GameObject yieldTrigger = Instantiate(STSRefs.AssetReferences._YieldTrigger, _position, Quaternion.identity, __AITrafficWaypointRoute.transform) as GameObject;
                routeInfo.yieldTrigger = yieldTrigger.GetComponent<BoxCollider>();
                Undo.RegisterCreatedObjectUndo(yieldTrigger, "ClickToSpawnYieldTrigger");
                Undo.RegisterCompleteObjectUndo(__AITrafficWaypointRoute, "ClickToSpawnYieldTrigger");
                Undo.CollapseUndoOperations(undoGroupIndex);
                return yieldTrigger.transform;
            }
            else
            {
                return null;
            }
        }

        [MenuItem("CONTEXT/AITrafficWaypointRoute/ClickToSpawnNextWaypoint" )]
        public static void ClickToSpawnNextWaypoint()
        {
            Debug.Log("!!");
        }

        public Transform ClickToSpawnNextWaypoint(AITrafficWaypointRoute __AITrafficWaypointRoute, Vector3 _position)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("ClickToSpawnNextWaypoint");
            var undoGroupIndex = Undo.GetCurrentGroup();
            Undo.RegisterCompleteObjectUndo(__AITrafficWaypointRoute, "ClickToSpawnNextWaypoint");
            GameObject newWaypoint = Instantiate(STSRefs.AssetReferences._AITrafficWaypoint, _position, Quaternion.identity, __AITrafficWaypointRoute.transform) as GameObject;
            CarAIWaypointInfo newPoint = new CarAIWaypointInfo();
            newPoint._name = newWaypoint.name = "AITrafficWaypoint " + (__AITrafficWaypointRoute.waypointDataList.Count + 1);
            newPoint._transform = newWaypoint.transform;
            newPoint._waypoint = newWaypoint.GetComponent<AITrafficWaypoint>();
            newPoint._waypoint.onReachWaypointSettings.waypointIndexnumber = __AITrafficWaypointRoute.waypointDataList.Count + 1;
            newPoint._waypoint.onReachWaypointSettings.parentRoute = __AITrafficWaypointRoute;
            newPoint._waypoint.onReachWaypointSettings.speedLimit = 25f;
            __AITrafficWaypointRoute.waypointDataList.Add(newPoint);
            Undo.RegisterCreatedObjectUndo(newWaypoint, "ClickToSpawnNextWaypoint");
            Undo.RegisterCompleteObjectUndo(__AITrafficWaypointRoute, "ClickToSpawnNextWaypoint");
            Undo.CollapseUndoOperations(undoGroupIndex);
            return newPoint._transform;
        }

        public void ClickToInsertSpawnNextWaypoint(AITrafficWaypointRoute __AITrafficWaypointRoute, Vector3 _position)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("ClickToInsertSpawnNextWaypoint");
            var undoGroupIndex = Undo.GetCurrentGroup();
            Undo.RegisterCompleteObjectUndo(__AITrafficWaypointRoute, "ClickToInsertSpawnNextWaypoint");
            List<Object> waypointObjectList = new List<Object>();
            for (int i = 0; i < __AITrafficWaypointRoute.waypointDataList.Count; i++)
            {
                waypointObjectList.Add(__AITrafficWaypointRoute.waypointDataList[i]._waypoint);
            }
            Object[] waypointObjectArray = waypointObjectList.ToArray();
            Undo.RegisterCompleteObjectUndo(waypointObjectArray, "ClickToSpawnNextWaypoint");
            bool isBetweenPoints = false;
            int insertIndex = 0;
            if (__AITrafficWaypointRoute.waypointDataList.Count >= 2)
            {
                for (int i = 0; i < __AITrafficWaypointRoute.waypointDataList.Count - 1; i++)
                {
                    Vector3 point_A = __AITrafficWaypointRoute.waypointDataList[i]._transform.position;
                    Vector3 point_B = __AITrafficWaypointRoute.waypointDataList[i + 1]._transform.position;
                    isBetweenPoints = IsCBetweenAB(point_A, point_B, _position);
                    insertIndex = i + 1;
                    if (isBetweenPoints) break;
                }
            }

            GameObject newWaypoint = Instantiate(STSRefs.AssetReferences._AITrafficWaypoint, _position, Quaternion.identity, __AITrafficWaypointRoute.transform) as GameObject;
            CarAIWaypointInfo newPoint = new CarAIWaypointInfo();
            newPoint._transform = newWaypoint.transform;
            newPoint._waypoint = newWaypoint.GetComponent<AITrafficWaypoint>();
            newPoint._waypoint.onReachWaypointSettings.parentRoute = __AITrafficWaypointRoute;
            newPoint._waypoint.onReachWaypointSettings.speedLimit = 25f;
            if (isBetweenPoints)
            {
                newPoint._transform.SetSiblingIndex(insertIndex);
                newPoint._name = newWaypoint.name = "AITrafficWaypoint " + (insertIndex + 1);
                __AITrafficWaypointRoute.waypointDataList.Insert(insertIndex, newPoint);
                for (int i = 0; i < __AITrafficWaypointRoute.waypointDataList.Count; i++)
                {
                    int newIndexName = i + 1;
                    __AITrafficWaypointRoute.waypointDataList[i]._transform.gameObject.name = "AITrafficWaypoint " + newIndexName;
                    __AITrafficWaypointRoute.waypointDataList[i]._waypoint.onReachWaypointSettings.waypointIndexnumber = i + 1;
                }
            }
            else
            {
                newPoint._name = newWaypoint.name = "AITrafficWaypoint " + (__AITrafficWaypointRoute.waypointDataList.Count + 1);
                newPoint._waypoint.onReachWaypointSettings.waypointIndexnumber = __AITrafficWaypointRoute.waypointDataList.Count + 1;
                __AITrafficWaypointRoute.waypointDataList.Add(newPoint);
            }
            Undo.RegisterCreatedObjectUndo(newWaypoint, "ClickToInsertSpawnNextWaypoint");
            Undo.RegisterCompleteObjectUndo(__AITrafficWaypointRoute, "ClickToInsertSpawnNextWaypoint");
            Undo.CollapseUndoOperations(undoGroupIndex);
        }

        bool IsCBetweenAB(Vector3 A, Vector3 B, Vector3 C)
        {
            return (
                Vector3.Dot((B - A).normalized, (C - B).normalized) < 0f && Vector3.Dot((A - B).normalized, (C - A).normalized) < 0f &&
                Vector3.Distance(A, B) >= Vector3.Distance(A, C) &&
                Vector3.Distance(A, B) >= Vector3.Distance(B, C)
                );
        }

    }
}