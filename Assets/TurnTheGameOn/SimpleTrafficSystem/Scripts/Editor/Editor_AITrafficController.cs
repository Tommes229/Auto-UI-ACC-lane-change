namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(AITrafficController))]
    public class Editor_AITrafficController : Editor
    {
        private static bool isInitialized;
        private static int tab;

        public static void Initialize()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                SceneView.duringSceneGui += CustomOnSceneGUI;
            }
        }

        private void OnEnable()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                SceneView.duringSceneGui += CustomOnSceneGUI;
            }
        }
        private void OnDisable()
        {
            SceneView.duringSceneGui += CustomOnSceneGUI;
            isInitialized = false;
        }

        private static Vector3 carPosition;
        private static Vector3 carTargetPosition;

        static void CustomOnSceneGUI(SceneView sceneview)
        {
            if (Application.isPlaying)
            {
                if (STSPrefs.sensorGizmos)
                {
                    for (int i = 0; i < AITrafficController.Instance.carCount; i++)
                    {
                        if (AITrafficController.Instance.GetIsDisabled(i) == false)
                        {
                            carPosition = AITrafficController.Instance.GetFrontSensorPosition(i);
                            carTargetPosition = AITrafficController.Instance.GetCarTargetPosition(i);
                            Handles.DrawBezier(
                                carPosition,
                                carTargetPosition,
                                carPosition,
                                carTargetPosition,
                                Color.green,
                                null,
                                3);
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.wideMode = true;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((AITrafficController)target), typeof(AITrafficController), false);
            GUI.enabled = true;

            AITrafficController _AITrafficController = (AITrafficController)target;

            EditorGUILayout.BeginVertical("Box");
            tab = GUILayout.Toolbar(tab, new string[] { "Cars", "Pooling" });
            EditorGUILayout.EndVertical();

            switch (tab)
            {
                case 0:
                    #region Car Settings
                    EditorGUILayout.BeginVertical("Box");



                    #region Traffic Prefabs
                    EditorGUILayout.BeginVertical("Box");

                    SerializedProperty trafficPrefabs = serializedObject.FindProperty("trafficPrefabs");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(trafficPrefabs, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.EndVertical();
                    #endregion Traffic Prefabs



                    #region Detection Sensors
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.LabelField("Detection Sensors", EditorStyles.miniLabel);

                    SerializedProperty layerMask = serializedObject.FindProperty("layerMask");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(layerMask, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty stopThreshold = serializedObject.FindProperty("stopThreshold");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(stopThreshold, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty frontSensorFacesTarget = serializedObject.FindProperty("frontSensorFacesTarget");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(frontSensorFacesTarget, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.EndVertical();
                    #endregion Detection Sensors



                    #region Car Settings
                    EditorGUILayout.BeginVertical("Box");

                    SerializedProperty speedMultiplier = serializedObject.FindProperty("speedMultiplier");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(speedMultiplier, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty steerSensitivity = serializedObject.FindProperty("steerSensitivity");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(steerSensitivity, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty maxSteerAngle = serializedObject.FindProperty("maxSteerAngle");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(maxSteerAngle, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.EndVertical();
                    #endregion Car Settings



                    #region Lane Changing
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.LabelField("Lane Changing", EditorStyles.miniLabel);

                    SerializedProperty useLaneChanging = serializedObject.FindProperty("useLaneChanging");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(useLaneChanging, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty changeLaneTrigger = serializedObject.FindProperty("changeLaneTrigger");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(changeLaneTrigger, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty minSpeedToChangeLanes = serializedObject.FindProperty("minSpeedToChangeLanes");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(minSpeedToChangeLanes, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty changeLaneCooldown = serializedObject.FindProperty("changeLaneCooldown");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(changeLaneCooldown, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.EndVertical();
                    #endregion Lane Changing



                    #region Yield Triggers
                    EditorGUILayout.BeginVertical("Box");

                    SerializedProperty useYieldTriggers = serializedObject.FindProperty("useYieldTriggers");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(useYieldTriggers, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.EndVertical();
                    #endregion Yield Triggers



                    #region Brake Material
                    EditorGUILayout.BeginVertical("Box");

                    SerializedProperty unassignedBrakeMaterial = serializedObject.FindProperty("unassignedBrakeMaterial");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(unassignedBrakeMaterial, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    if (RenderPipeline.IsHDRP)
                    {
                        SerializedProperty brakeOnIntensityHDRP = serializedObject.FindProperty("brakeOnIntensityHDRP");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(brakeOnIntensityHDRP, true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();

                        SerializedProperty brakeOffIntensityHDRP = serializedObject.FindProperty("brakeOffIntensityHDRP");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(brakeOffIntensityHDRP, true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();
                    }
                    else if (RenderPipeline.IsURP)
                    {
                        SerializedProperty brakeOnIntensityURP = serializedObject.FindProperty("brakeOnIntensityURP");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(brakeOnIntensityURP, true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();

                        SerializedProperty brakeOffIntensityURP = serializedObject.FindProperty("brakeOffIntensityURP");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(brakeOffIntensityURP, true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();
                    }
                    else
                    {
                        SerializedProperty brakeOnIntensityDP = serializedObject.FindProperty("brakeOnIntensityDP");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(brakeOnIntensityDP, true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();

                        SerializedProperty brakeOffIntensityDP = serializedObject.FindProperty("brakeOffIntensityDP");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(brakeOffIntensityDP, true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();
                    }

                    EditorGUILayout.EndVertical();
                    #endregion




                    #region Car Parent
                    EditorGUILayout.BeginVertical("Box");

                    SerializedProperty setCarParent = serializedObject.FindProperty("setCarParent");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(setCarParent, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    if (_AITrafficController.setCarParent)
                    {
                        EditorGUILayout.HelpBox("Please note that having the car objects under a parent will result in reduced perfiormance, this setting is only recommended for reducing hierarchy clutter with in editor testing.", MessageType.Warning);
                    }

                    SerializedProperty carParent = serializedObject.FindProperty("carParent");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(carParent, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.EndVertical();
                    #endregion
                    EditorGUILayout.EndVertical();
                    #endregion
                    break;
                case 1:
                    #region Pooling

                    SerializedProperty showPoolingWarning = serializedObject.FindProperty("showPoolingWarning");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(showPoolingWarning, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    if (_AITrafficController.showPoolingWarning)
                    {
                        EditorGUILayout.HelpBox("NOTE: OnBecameVisible and OnBecameInvisible are used by cars and spawn points to determine if they are visible.\n\n" +
                        "These callbacks are also triggered by the editor scene camera.\n\n" +
                        "Hide the scene view while testing for the most accurate simulation, which is what the final build will be.\n\n" +
                        "Not hiding the scene view camera may cause objcets to register the wrong state, resulting in unproper behavior.", MessageType.Warning);
                    }

                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.BeginVertical("Box");
                    SerializedProperty usePooling = serializedObject.FindProperty("usePooling");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(usePooling, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty useRouteLimit = serializedObject.FindProperty("useRouteLimit");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(useRouteLimit, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty centerPoint = serializedObject.FindProperty("centerPoint");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(centerPoint, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty carsInPool = serializedObject.FindProperty("carsInPool");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(carsInPool, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty density = serializedObject.FindProperty("density");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(density, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty spawnRate = serializedObject.FindProperty("spawnRate");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(spawnRate, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty disabledPosition = serializedObject.FindProperty("disabledPosition");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(disabledPosition, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.LabelField("Pooling Zones", EditorStyles.miniLabel);

                    SerializedProperty minSpawnZone = serializedObject.FindProperty("minSpawnZone");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(minSpawnZone, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty cullHeadLight = serializedObject.FindProperty("cullHeadLight");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(cullHeadLight, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty actizeZone = serializedObject.FindProperty("actizeZone");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(actizeZone, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty spawnZone = serializedObject.FindProperty("spawnZone");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(spawnZone, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    #endregion
                    break;
            }
        }
    }

    [InitializeOnLoad]
    public static class PlayModeStateChanged
    {
        static Vector3 centerPosition;

        static PlayModeStateChanged()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state.ToString() == "EnteredPlayMode" && AITrafficController.Instance)
            {
                Editor_AITrafficController.Initialize();
            }
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawHandles(AITrafficController _AITrafficController, GizmoType gizmoType)
        {
            if (EditorApplication.isPlaying && _AITrafficController.usePooling && STSPrefs.poolGizmos)
            {
                centerPosition = _AITrafficController.centerPoint.position;
                // spawn zone
                Handles.color = STSPrefs.spawnZoneColor;
                Handles.DrawSolidDisc
                    (
                    centerPosition,
                Vector3.up,
                _AITrafficController.spawnZone
                );

                // active zone
                Handles.color = STSPrefs.activeZoneColor;
                Handles.DrawSolidDisc
                    (
                    centerPosition,
                Vector3.up,
                _AITrafficController.actizeZone
                );

                // cull headlight zone
                Handles.color = STSPrefs.cullHeadLightZone;
                Handles.DrawSolidDisc
                    (
                    centerPosition,
                Vector3.up,
                _AITrafficController.cullHeadLight
                );

                // min spawn zone
                Handles.color = STSPrefs.minSpawnZoneColor;
                Handles.DrawSolidDisc
                    (
                    centerPosition,
                Vector3.up,
                _AITrafficController.minSpawnZone
                );
            }
        }
    }

}