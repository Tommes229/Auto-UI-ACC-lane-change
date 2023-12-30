namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(AITrafficCar))]
    public class Editor_AITrafficCar : Editor
    {
        private static int tab;
        private static bool showSensorSettings;

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((AITrafficCar)target), typeof(AITrafficCar), false);
            GUI.enabled = true;
            EditorGUIUtility.wideMode = true;

            EditorGUILayout.BeginVertical("Box");
            tab = GUILayout.Toolbar(tab, new string[] { "Settings", "References" });
            EditorGUILayout.EndVertical();

            //EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            switch (tab)
            {
                case 0:
                    SerializedProperty vehicleType = serializedObject.FindProperty("vehicleType");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(vehicleType, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty topSpeed = serializedObject.FindProperty("topSpeed");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(topSpeed, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty accelerationPower = serializedObject.FindProperty("accelerationPower");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(accelerationPower, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty minDrag = serializedObject.FindProperty("minDrag");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(minDrag, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty minAngularDrag = serializedObject.FindProperty("minAngularDrag");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(minAngularDrag, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty goToStartOnStop = serializedObject.FindProperty("goToStartOnStop");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(goToStartOnStop, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    showSensorSettings = EditorGUILayout.Foldout(showSensorSettings, "Sensor Settings", true);
                    if (showSensorSettings)
                    {
                        SerializedProperty frontSensorSize = serializedObject.FindProperty("frontSensorSize");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(frontSensorSize, true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();

                        SerializedProperty frontSensorLength = serializedObject.FindProperty("frontSensorLength");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(frontSensorLength, true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();

                        SerializedProperty sideSensorSize = serializedObject.FindProperty("sideSensorSize");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(sideSensorSize, true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();

                        SerializedProperty sideSensorLength = serializedObject.FindProperty("sideSensorLength");
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(sideSensorLength, true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();
                    }
                    break;
                case 1:
                    EditorGUILayout.BeginVertical("Box");
                    SerializedProperty frontSensorTransform = serializedObject.FindProperty("frontSensorTransform");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(frontSensorTransform, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty leftSensorTransform = serializedObject.FindProperty("leftSensorTransform");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(leftSensorTransform, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty rightSensorTransform = serializedObject.FindProperty("rightSensorTransform");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(rightSensorTransform, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.BeginVertical("Box");
                    SerializedProperty headLight = serializedObject.FindProperty("headLight");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(headLight, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.BeginVertical("Box");
                    SerializedProperty brakeMaterialMesh = serializedObject.FindProperty("brakeMaterialMesh");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(brakeMaterialMesh, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    SerializedProperty brakeMaterialIndex = serializedObject.FindProperty("brakeMaterialIndex");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(brakeMaterialIndex, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    if (GUILayout.Button(new GUIContent("Auto Rig Brake Mesh", "Searches children to find the mesh, and material index that has the matching 'Brake Light' material name, set in Preferences.")))
                    {
                        AutoRigBrakeMesh();
                    }
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.BeginVertical("Box");
                    SerializedProperty _wheels = serializedObject.FindProperty("_wheels");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(_wheels, true);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    if (GUILayout.Button(new GUIContent("Auto Rig Wheel Mesh", "Searches children to find the meshes that have the matching 'Wheel' names, set in Preferences.")))
                    {
                        AutoRigWheelMesh();
                    }

                    if (GUILayout.Button(new GUIContent("Align Wheel Colliders", "Moves the wheel collider positions to match the pivot of the assigned wheel meshes. Size and positions may still need to be adjusted after.")))
                    {
                        AlignWheelColliders();
                    }
                    EditorGUILayout.EndVertical();
                    break;
            }
            EditorGUILayout.EndVertical();
        }

        public void AutoRigBrakeMesh()
        {
            AITrafficCar _car = (AITrafficCar)target;
            Transform[] allChildTransforms = _car.GetComponentsInChildren<Transform>();

            for (int i = 0; i < allChildTransforms.Length; i++)
            {
                    MeshRenderer rend = allChildTransforms[i].GetComponent<MeshRenderer>();
                    if (rend != null)
                    {
                        for (int j = 0; j < rend.sharedMaterials.Length; j++)
                        {
                            if (rend.sharedMaterials[j].name == STSPrefs.brakeMaterialName)
                            {
                                _car.brakeMaterialMesh = rend;
                                _car.brakeMaterialIndex = j;
                                break;
                            }
                        }
                    }
            }
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_car.gameObject.scene);
        }

        public void AutoRigWheelMesh()
        {
            AITrafficCar _car = (AITrafficCar)target;
            Transform[] allChildTransforms = _car.GetComponentsInChildren<Transform>();

            for (int i = 0; i < allChildTransforms.Length; i++)
            {
                if (allChildTransforms[i].name == STSPrefs.fr_wheelName)
                {
                    _car._wheels[0].meshTransform = allChildTransforms[i];
                }
                else if (allChildTransforms[i].name == STSPrefs.fl_wheelName)
                {
                    _car._wheels[1].meshTransform = allChildTransforms[i];
                }
                else if (allChildTransforms[i].name == STSPrefs.br_wheelName)
                {
                    _car._wheels[2].meshTransform = allChildTransforms[i];
                }
                else if (allChildTransforms[i].name == STSPrefs.bl_wheelName)
                {
                    _car._wheels[3].meshTransform = allChildTransforms[i];
                }
            }
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_car.gameObject.scene);
        }

        public void AlignWheelColliders()
        {
            AITrafficCar _car = (AITrafficCar)target;
            Transform defaultColliderParent = _car._wheels[0].collider.transform.parent; // make a reference to the colliders original parent

            _car._wheels[0].collider.transform.parent = _car._wheels[0].meshTransform;// move colliders to the reference positions
            _car._wheels[1].collider.transform.parent = _car._wheels[1].meshTransform;
            _car._wheels[2].collider.transform.parent = _car._wheels[2].meshTransform;
            _car._wheels[3].collider.transform.parent = _car._wheels[3].meshTransform;

            _car._wheels[0].collider.transform.position = new Vector3(_car._wheels[0].meshTransform.position.x,
                _car._wheels[0].collider.transform.position.y, _car._wheels[0].meshTransform.position.z); //adjust the wheel collider positions on x and z axis to match the new wheel position
            _car._wheels[1].collider.transform.position = new Vector3(_car._wheels[1].meshTransform.position.x,
                _car._wheels[1].collider.transform.position.y, _car._wheels[1].meshTransform.position.z);
            _car._wheels[2].collider.transform.position = new Vector3(_car._wheels[2].meshTransform.position.x,
                _car._wheels[2].collider.transform.position.y, _car._wheels[2].meshTransform.position.z);
            _car._wheels[3].collider.transform.position = new Vector3(_car._wheels[3].meshTransform.position.x,
                _car._wheels[3].collider.transform.position.y, _car._wheels[3].meshTransform.position.z);

            _car._wheels[0].collider.transform.parent = defaultColliderParent; // move colliders back to the original parent
            _car._wheels[1].collider.transform.parent = defaultColliderParent;
            _car._wheels[2].collider.transform.parent = defaultColliderParent;
            _car._wheels[3].collider.transform.parent = defaultColliderParent;

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_car.gameObject.scene);
        }

    }
}