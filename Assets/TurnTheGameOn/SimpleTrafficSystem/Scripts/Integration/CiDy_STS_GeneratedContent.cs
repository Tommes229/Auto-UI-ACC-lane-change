namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using System.Collections.Generic;
#if CiDy
    using CiDy;
#endif

    public class CiDy_STS_GeneratedContent : MonoBehaviour
    {
#if UNITY_EDITOR
#if CiDy
        [Header("CiDy Road Options")]
        [Tooltip("Check if roads are using the STS configured traffic light or stop sign. If not, the correct prefab will be assigned, then the road will be regenerated to spawn it.")]
        public bool regenerateRoads = true;

        [Header("Options")]
        public bool spawnPoints = true;
        public int carsSpawnedPerRoute = 6;
        public Vector3 waypointSize = new Vector3(5, 1, 1);

        [Header("Speed Limits")]
        public float defaultSpeed = 35f;
        public float intersectionSpeed = 35f;
        public float culDeSacSpeed = 20f;

        [Header("Pooling")]
        public int maxDensity = 6;


        [Header("Traffic Lights")]
        public float greenTimer = 15f;
        public float yellowTimer = 3f;
        public float redTimer = 3f;

        [Header("Stop Signs")]
        public float activeTime = 1f;
        public float waitTime = 4f;


        [Header("Generated Data")]

        #region Route Data
        public STSRouteData routeData;
        public List<AITrafficWaypointRoute> spawnedRoutes = new List<AITrafficWaypointRoute>();
        public List<AITrafficLightManager> spawnedLightManagers = new List<AITrafficLightManager>();
        public List<AITrafficStopManager> spawnedStopManagers = new List<AITrafficStopManager>();

        [System.Serializable]
        public class STSRouteData
        {
            public List<Route> routes;

            public STSRouteData()
            {
                routes = new List<Route>(0);
            }
        }

        [System.Serializable]
        public class Route
        {
            public bool isIntersection;
            public bool isCulDeSac;
            public bool isContinuedSection;
            public List<Vector3> waypoints;
            public List<Vector3> newRoutePoints;
            public AITrafficWaypointRoute route;
            public Vector3 intersectionStartPoint;
            public Vector3 intersectionEndPoint;
            public AITrafficLight trafficLight;

            public Route()
            {
                waypoints = new List<Vector3>(0);
                newRoutePoints = new List<Vector3>(0);
            }
        }
        #endregion

        #region Intersection Data
        public STSIntersectionData intersectionData;

        public enum IntersectionType
        {
            None,
            TrafficLight,
            StopSign
        }

        [System.Serializable]
        public class STSIntersectionData
        {
            public List<STSIntersection> intersectionList; // list of all intersections

            public STSIntersectionData()
            {
                intersectionList = new List<STSIntersection>(0);
            }
        }

        [System.Serializable]
        public class STSIntersection
        {
            public IntersectionType type = IntersectionType.None;
            public List<IntersectionRoute> intersectionRoadList; // list of all routes in the intersection, used by an AITrafficLightManager
            public List<Sequence> sequenceList;

            public STSIntersection()
            {
                intersectionRoadList = new List<IntersectionRoute>(0);
            }
        }

        [System.Serializable]
        public class Sequence
        {
            public List<IntersectionRoute> sequenceList;

            public Sequence()
            {
                sequenceList = new List<IntersectionRoute>();
            }
        }

        [System.Serializable]
        public class IntersectionRoute
        {
            //public CiDyRoad cidyRoad;
            public AITrafficWaypointRoute route;
            public Vector3 finalRoutePoint; // can be used to find the route
            public AITrafficLight light; // light that will control this route
            public AITrafficStop stop; // stop that will control this route
            public int sequenceIndex; // determines which routes are active in a AITrafficLightManager sequence together
        }
        #endregion

        public void ClearAllGeneratedRoutes()
        {
            for (int i = 0; i < spawnedRoutes.Count; i++)
            {
                if (spawnedRoutes[i] != null)
                {
                    GameObject.DestroyImmediate(spawnedRoutes[i].gameObject);
                }
            }
            spawnedRoutes = new List<AITrafficWaypointRoute>();

            for (int i = 0; i < spawnedLightManagers.Count; i++)
            {
                if (spawnedLightManagers[i] != null)
                {
                    GameObject.DestroyImmediate(spawnedLightManagers[i].gameObject);
                }
            }
            spawnedLightManagers = new List<AITrafficLightManager>();

            for (int i = 0; i < spawnedStopManagers.Count; i++)
            {
                if (spawnedStopManagers[i] != null)
                {
                    GameObject.DestroyImmediate(spawnedStopManagers[i].gameObject);
                }
            }
            spawnedStopManagers = new List<AITrafficStopManager>();

            intersectionData = new STSIntersectionData();
            routeData = new STSRouteData();
        }

        public void Generate()
        {
            if (regenerateRoads)
            {
                RefreshTrafficLightsAndStopSigns();
            }
            CiDyGraph graph = GameObject.FindObjectOfType<CiDyGraph>();
            graph.BuildTrafficData();
            GenerateRoutesAndConnections();
            GenerateIntersectionData();
            GenerateTrafficLightManagers();
            GenerateTrafficStopManagers();

        }

        void GenerateRoutesAndConnections()
        {
            ClearAllGeneratedRoutes(); //Clear Previous Routes

            //Get CiDyGraph and Road Objects
            CiDyGraph graph = GameObject.FindObjectOfType<CiDyGraph>(); //Grab Graph
            List<GameObject> roads = graph.roads; //Grab Roads of Graph.

            routeData = new STSRouteData();
            for (int i = 0; i < roads.Count; i++)
            {
                CiDyRoad road = roads[i].GetComponent<CiDyRoad>();
                for (int j = 0; j < road.leftRoutes.routes.Count; j++)
                {
                    Route route = new Route();
                    route.waypoints = road.leftRoutes.routes[j].waypoints;
                    route.newRoutePoints = road.leftRoutes.routes[j].newRoutePoints;
                    routeData.routes.Add(route);
                }
                for (int j = 0; j < road.rightRoutes.routes.Count; j++)
                {
                    Route route = new Route();
                    route.waypoints = road.rightRoutes.routes[j].waypoints;
                    route.newRoutePoints = road.rightRoutes.routes[j].newRoutePoints;
                    routeData.routes.Add(route);
                }
            }

            //Iterate through Nodes and Add Routes
            List<CiDyNode> nodes = graph.masterGraph;
            for (int i = 0; i < nodes.Count; i++)
            {
                CiDyNode node = nodes[i];
                //Check if Connector or Intersection?
                if (node.type == CiDyNode.IntersectionType.continuedSection)
                {
                    //Road Connection
                    for (int j = 0; j < node.leftRoutes.routes.Count; j++)
                    {
                        Route route = new Route();
                        route.isContinuedSection = true;

                        List<Vector3> omitLastPointList = new List<Vector3>();
                        for (int k = 0; k < node.leftRoutes.routes[j].waypoints.Count - 1; k++)
                        {
                            omitLastPointList.Add(node.leftRoutes.routes[j].waypoints[k]);
                        }
                        route.waypoints = omitLastPointList;
                        //route.waypoints = node.leftRoutes.routes[j].waypoints;
                        route.newRoutePoints = node.leftRoutes.routes[j].newRoutePoints;
                        routeData.routes.Add(route);
                    }
                    for (int j = 0; j < node.rightRoutes.routes.Count; j++)
                    {
                        Route route = new Route();
                        route.isContinuedSection = true;

                        List<Vector3> omitLastPointList = new List<Vector3>();
                        for (int k = 0; k < node.rightRoutes.routes[j].waypoints.Count - 1; k++)
                        {
                            omitLastPointList.Add(node.rightRoutes.routes[j].waypoints[k]);
                        }
                        route.waypoints = omitLastPointList;
                        //route.waypoints = node.rightRoutes.routes[j].waypoints;
                        route.newRoutePoints = node.rightRoutes.routes[j].newRoutePoints;
                        routeData.routes.Add(route);
                    }
                }
                else if (node.type == CiDyNode.IntersectionType.tConnect)
                {
                    //Intersection
                    for (int j = 0; j < node.intersectionRoutes.intersectionRoutes.Count; j++)
                    {
                        Route route = new Route();
                        route.isIntersection = true;
                        route.intersectionStartPoint = node.intersectionRoutes.intersectionRoutes[j].route.waypoints[0];
                        route.intersectionEndPoint = node.intersectionRoutes.intersectionRoutes[j].route.waypoints[node.intersectionRoutes.intersectionRoutes[j].route.waypoints.Count - 1];

                        List<Vector3> omitFirstlastPointList = new List<Vector3>();
                        for (int k = 1; k < node.intersectionRoutes.intersectionRoutes[j].route.waypoints.Count - 1; k++)
                        {
                            omitFirstlastPointList.Add(node.intersectionRoutes.intersectionRoutes[j].route.waypoints[k]);
                        }
                        route.waypoints = omitFirstlastPointList;
                        route.newRoutePoints = node.intersectionRoutes.intersectionRoutes[j].route.newRoutePoints;
                        routeData.routes.Add(route);
                    }
                }
                else if (node.type == CiDyNode.IntersectionType.culDeSac)
                {
                    //Culdesac
                    for (int j = 0; j < node.leftRoutes.routes.Count; j++)
                    {
                        Route route = new Route();
                        route.isCulDeSac = true;
                        route.waypoints = node.leftRoutes.routes[j].waypoints;
                        route.newRoutePoints = node.leftRoutes.routes[j].newRoutePoints;
                        routeData.routes.Add(route);
                    }
                }
            }

            /// Generate Primary Routes
            for (int i = 0; i < routeData.routes.Count; i++)
            {
                AITrafficWaypointRoute route = GameObject.Instantiate(STSRefs.AssetReferences._AITrafficWaypointRoute, Vector3.zero, Quaternion.identity, this.transform).GetComponent<AITrafficWaypointRoute>();
                route.name = "Route_" + i;
                for (int j = 0; j < routeData.routes[i].waypoints.Count; j++)
                {
                    AITrafficWaypoint waypoint = GameObject.Instantiate(STSRefs.AssetReferences._AITrafficWaypoint, routeData.routes[i].waypoints[j], Quaternion.identity, route.transform).GetComponent<AITrafficWaypoint>();
                    waypoint.name = "Waypoint_" + (j + 1).ToString();
                    waypoint.transform.GetComponent<BoxCollider>().size = waypointSize;
                    waypoint.onReachWaypointSettings.parentRoute = route;
                    waypoint.onReachWaypointSettings.waypoint = waypoint;
                    waypoint.onReachWaypointSettings.waypointIndexnumber = j + 1;
                    if (routeData.routes[i].isIntersection)
                    {
                        waypoint.onReachWaypointSettings.speedLimit = intersectionSpeed;
                    }
                    else if (routeData.routes[i].isCulDeSac)
                    {
                        waypoint.onReachWaypointSettings.speedLimit = culDeSacSpeed;
                    }
                    else
                    {
                        if (j == routeData.routes[i].waypoints.Count - 1 || j == routeData.routes[i].waypoints.Count - 2) // final point, slow down, leads to new connection
                        {
                            waypoint.onReachWaypointSettings.speedLimit = intersectionSpeed;
                        }
                        else
                        {
                            waypoint.onReachWaypointSettings.speedLimit = defaultSpeed;
                        }
                    }
                    CarAIWaypointInfo waypointInfo = new CarAIWaypointInfo();
                    waypointInfo._name = waypoint.name;
                    waypointInfo._transform = waypoint.transform;
                    waypointInfo._waypoint = waypoint;
                    route.waypointDataList.Add(waypointInfo);
                }
                routeData.routes[i].route = route;
                spawnedRoutes.Add(route);
                route.AlignPoints();
            }

            /// Generate Intersection Connections
            for (int i = 0; i < routeData.routes.Count; i++)
            {
                // get final route waypoint
                AITrafficWaypoint finalRoutePoint = routeData.routes[i].route.waypointDataList[routeData.routes[i].route.waypointDataList.Count - 1]._waypoint;
                // iterate through all new route points for this route
                for (int j = 0; j < routeData.routes[i].newRoutePoints.Count; j++)
                {
                    // iterate through all routes
                    for (int k = 0; k < routeData.routes.Count; k++)
                    {
                        if (routeData.routes[k].isIntersection)
                        {
                            // if routes first waypoint position matches current new route point
                            if (routeData.routes[k].intersectionStartPoint == routeData.routes[i].newRoutePoints[j])
                            {
                                // create new route point array with size 1 larger than current
                                AITrafficWaypoint[] newRoutePointsArray = new AITrafficWaypoint[finalRoutePoint.onReachWaypointSettings.newRoutePoints.Length + 1];
                                // re-assign current indexes
                                for (int l = 0; l < newRoutePointsArray.Length - 1; l++)
                                {
                                    newRoutePointsArray[l] = finalRoutePoint.onReachWaypointSettings.newRoutePoints[l];
                                }
                                // assign matching new route point
                                newRoutePointsArray[newRoutePointsArray.Length - 1] = routeData.routes[k].route.waypointDataList[0]._waypoint;
                                finalRoutePoint.onReachWaypointSettings.newRoutePoints = newRoutePointsArray;
                            }
                        }
                    }
                }
            }

            /// Generate End of Intersection Connections
            for (int i = 0; i < routeData.routes.Count; i++)
            {
                if (routeData.routes[i].isIntersection)
                {
                    AITrafficWaypoint finalRoutePoint = routeData.routes[i].route.waypointDataList[routeData.routes[i].route.waypointDataList.Count - 1]._waypoint;
                    for (int j = 0; j < routeData.routes.Count; j++)
                    {
                        if (routeData.routes[i].intersectionEndPoint == routeData.routes[j].route.waypointDataList[0]._transform.position)
                        {
                            // create new route point array with size 1 larger than current
                            AITrafficWaypoint[] newRoutePointsArray = new AITrafficWaypoint[finalRoutePoint.onReachWaypointSettings.newRoutePoints.Length + 1];
                            // re-assign current indexes
                            for (int l = 0; l < newRoutePointsArray.Length - 1; l++)
                            {
                                newRoutePointsArray[l] = finalRoutePoint.onReachWaypointSettings.newRoutePoints[l];
                            }
                            // assign matching new route point
                            newRoutePointsArray[newRoutePointsArray.Length - 1] = routeData.routes[j].route.waypointDataList[0]._waypoint;
                            finalRoutePoint.onReachWaypointSettings.newRoutePoints = newRoutePointsArray;
                        }
                    }
                }
            }

            /// Generate Cul De Sac Connections
            for (int i = 0; i < routeData.routes.Count; i++)
            {
                // get final route waypoint
                AITrafficWaypoint finalRoutePoint = routeData.routes[i].route.waypointDataList[routeData.routes[i].route.waypointDataList.Count - 1]._waypoint;
                for (int j = 0; j < routeData.routes[i].newRoutePoints.Count; j++)
                {
                    if (finalRoutePoint.onReachWaypointSettings.newRoutePoints.Length == 0)
                    {
                        // iterate through all routes
                        for (int k = 0; k < routeData.routes.Count; k++)
                        {
                            if (routeData.routes[k].isCulDeSac)
                            {
                                if (routeData.routes[k].route.waypointDataList[0]._transform.position == routeData.routes[i].newRoutePoints[j])
                                {
                                    // create new route point array with size 1 larger than current
                                    AITrafficWaypoint[] newRoutePointsArray = new AITrafficWaypoint[finalRoutePoint.onReachWaypointSettings.newRoutePoints.Length + 1];
                                    // re-assign current indexes
                                    for (int l = 0; l < newRoutePointsArray.Length - 1; l++)
                                    {
                                        newRoutePointsArray[l] = finalRoutePoint.onReachWaypointSettings.newRoutePoints[l];
                                    }
                                    // assign matching new route point
                                    newRoutePointsArray[newRoutePointsArray.Length - 1] = routeData.routes[k].route.waypointDataList[0]._waypoint;
                                    finalRoutePoint.onReachWaypointSettings.newRoutePoints = newRoutePointsArray;
                                }
                            }
                        }
                    }
                }
            }

            /// Generate End of Cul De Sac Connections
            for (int i = 0; i < routeData.routes.Count; i++)
            {
                if (routeData.routes[i].isCulDeSac)
                {
                    for (int j = 0; j < routeData.routes.Count; j++)
                    {
                        if (routeData.routes[j].route.waypointDataList[0]._transform.position == routeData.routes[i].newRoutePoints[0])
                        {
                            AITrafficWaypoint finalRoutePoint = routeData.routes[i].route.waypointDataList[routeData.routes[i].route.waypointDataList.Count - 1]._waypoint;
                            AITrafficWaypoint[] newRoutePointsArray = new AITrafficWaypoint[1];
                            newRoutePointsArray[0] = routeData.routes[j].route.waypointDataList[0]._waypoint;
                            finalRoutePoint.onReachWaypointSettings.newRoutePoints = newRoutePointsArray;
                        }
                    }
                }
            }


            /// Generate Spawn Points
            if (spawnPoints)
            {
                for (int i = 0; i < routeData.routes.Count; i++)
                {
                    if (routeData.routes[i].isCulDeSac == false && routeData.routes[i].isIntersection == false && routeData.routes[i].isContinuedSection == false)
                    {
                        routeData.routes[i].route.SetupRandomSpawnPoints();
                        routeData.routes[i].route.useSpawnPoints = true;
                        routeData.routes[i].route.spawnFromAITrafficController = true;
                        routeData.routes[i].route.spawnAmount = carsSpawnedPerRoute;
                    }
                }
            }

            /// Pooling
            for (int i = 0; i < routeData.routes.Count; i++)
            {
                routeData.routes[i].route.maxDensity = maxDensity;
            }

            /// Generate Continued Section connections
            for (int i = 0; i < routeData.routes.Count; i++)
            {
                if (routeData.routes[i].isCulDeSac == false && routeData.routes[i].isIntersection == false)
                {
                    AITrafficWaypoint finalRoutePoint = routeData.routes[i].route.waypointDataList[routeData.routes[i].route.waypointDataList.Count - 1]._waypoint;
                    if (finalRoutePoint.onReachWaypointSettings.newRoutePoints.Length == 0)
                    {
                        // create new route point array with size 1 larger than current
                        AITrafficWaypoint[] newRoutePointsArray = new AITrafficWaypoint[finalRoutePoint.onReachWaypointSettings.newRoutePoints.Length + 1];
                        // re-assign current indexes
                        for (int l = 0; l < newRoutePointsArray.Length - 1; l++)
                        {
                            newRoutePointsArray[l] = finalRoutePoint.onReachWaypointSettings.newRoutePoints[l];
                        }
                        // assign matching new route point
                        for (int j = 0; j < routeData.routes.Count; j++)
                        {
                            if (routeData.routes[i].newRoutePoints.Count > 0)
                            {
                                if (routeData.routes[i].newRoutePoints[0] == routeData.routes[j].route.waypointDataList[0]._transform.position)
                                {
                                    newRoutePointsArray[newRoutePointsArray.Length - 1] = routeData.routes[j].route.waypointDataList[0]._waypoint;
                                }
                            }
                        }
                        if (newRoutePointsArray[0] != null)
                        {
                            finalRoutePoint.onReachWaypointSettings.newRoutePoints = newRoutePointsArray;
                        }
                    }
                }
            }
        }

        void GenerateIntersectionData()
        {
            intersectionData = new STSIntersectionData();

            CiDyGraph graph = GameObject.FindObjectOfType<CiDyGraph>();
            if (graph == null)
            {
                return;
            }
            List<CiDyNode> nodes = graph.masterGraph;
            for (int i = 0; i < nodes.Count; i++)
            {
                STSIntersection intersection = new STSIntersection();
                intersection.intersectionRoadList = new List<IntersectionRoute>();
                for (int j = 0; j < nodes[i].intersectionRoutes.intersectionRoutes.Count; j++)
                {
                    bool routeWasAlreadyRegistered = false; // check to prevent duplicates
                    for (int k = 0; k < intersection.intersectionRoadList.Count; k++)
                    {
                        if (intersection.intersectionRoadList[k].finalRoutePoint == nodes[i].intersectionRoutes.intersectionRoutes[j].finalRoutePoint)
                        {
                            routeWasAlreadyRegistered = true;
                            break;
                        }
                    }
                    if (routeWasAlreadyRegistered == false)
                    {
                        IntersectionRoute intersectionRoute = new IntersectionRoute();
                        intersectionRoute.finalRoutePoint = nodes[i].intersectionRoutes.intersectionRoutes[j].finalRoutePoint;
                        intersectionRoute.light = nodes[i].intersectionRoutes.intersectionRoutes[j].light.GetComponent<AITrafficLight>();
                        intersectionRoute.stop = nodes[i].intersectionRoutes.intersectionRoutes[j].light.GetComponent<AITrafficStop>();
                        if (intersectionRoute.light != null) intersectionRoute.light.waypointRoutes.Clear(); // clear light routes
                        if (intersectionRoute.stop != null) intersectionRoute.stop.waypointRoutes.Clear(); // clear light routes
                        intersectionRoute.sequenceIndex = nodes[i].intersectionRoutes.intersectionRoutes[j].sequenceIndex;
                        for (int k = 0; k < routeData.routes.Count; k++)
                        {
                            if (intersectionRoute.finalRoutePoint == routeData.routes[k].waypoints[routeData.routes[k].waypoints.Count - 1])
                            {
                                intersectionRoute.route = routeData.routes[k].route;
                                break;
                            }
                        }
                        if (intersectionRoute.light != null)
                            intersectionRoute.light.waypointRoutes.Add(intersectionRoute.route);
                        if (intersectionRoute.stop != null)
                            intersectionRoute.stop.waypointRoutes.Add(intersectionRoute.route);
                        intersection.intersectionRoadList.Add(intersectionRoute);
                    }
                }
                intersectionData.intersectionList.Add(intersection);
                // sort into sequences
                intersection.sequenceList = new List<Sequence>();
                for (int k = 0; k < 10; k++)
                {
                    Sequence sequence = new Sequence();
                    for (int l = 0; l < intersection.intersectionRoadList.Count; l++)
                    {
                        if (intersection.intersectionRoadList[l].sequenceIndex == k)
                        {
                            sequence.sequenceList.Add(intersection.intersectionRoadList[l]);
                        }
                    }
                    if (sequence.sequenceList.Count != 0)
                    intersection.sequenceList.Add(sequence);
                }
            }
            for (int i = 0; i < intersectionData.intersectionList.Count; i++) // set intersection type
            {
                IntersectionType firstRoadType = IntersectionType.None;
                IntersectionType intersectionType;
                for (int j = 0; j < intersectionData.intersectionList[i].intersectionRoadList.Count; j++)
                {
                    AITrafficStop stop = intersectionData.intersectionList[i].intersectionRoadList[j].stop;
                    AITrafficLight light = intersectionData.intersectionList[i].intersectionRoadList[j].light;
                    intersectionType = stop != null ? IntersectionType.StopSign :
                            light != null ? IntersectionType.TrafficLight :
                            IntersectionType.None;
                    if (j == 0)
                    {
                        firstRoadType = intersectionType;
                    }
                    if (firstRoadType != intersectionType)
                    {
                        intersectionData.intersectionList[i].type = IntersectionType.None;
                        break;
                    }
                    else
                    {
                        intersectionData.intersectionList[i].type = intersectionType;
                    }
                }
            }
        }

        void GenerateTrafficStopManagers()
        {
            for (int i = 0; i < intersectionData.intersectionList.Count; i++)
            {
                if (intersectionData.intersectionList[i].type == IntersectionType.StopSign)
                {
                    if (intersectionData.intersectionList[i].sequenceList.Count > 0)
                    {
                        GameObject AITrafficStopManagerObject = new GameObject();
                        AITrafficStopManagerObject.transform.parent = this.transform;
                        AITrafficStopManagerObject.name = "AITrafficStopManager_" + i.ToString();
                        AITrafficStopManager _AITrafficStopManager = AITrafficStopManagerObject.AddComponent<AITrafficStopManager>();
                        spawnedStopManagers.Add(_AITrafficStopManager);
                        if (intersectionData.intersectionList[i].sequenceList.Count > 0)
                        {
                            _AITrafficStopManager.stopCycles = new AITrafficStopCycle[intersectionData.intersectionList[i].sequenceList.Count];
                            for (int j = 0; j < intersectionData.intersectionList[i].sequenceList.Count; j++)
                            {
                                AITrafficStop[] stopArray = new AITrafficStop[intersectionData.intersectionList[i].sequenceList[j].sequenceList.Count];
                                for (int k = 0; k < stopArray.Length; k++)
                                {
                                    stopArray[k] = intersectionData.intersectionList[i].sequenceList[j].sequenceList[k].stop;
                                }
                                _AITrafficStopManager.stopCycles[j].trafficStops = stopArray;
                                _AITrafficStopManager.stopCycles[j].activeTime = activeTime;
                                _AITrafficStopManager.stopCycles[j].waitTime = waitTime;
                            }
                        }
                    }
                }
            }
        }

        void GenerateTrafficLightManagers()
        {
            for (int i = 0; i < intersectionData.intersectionList.Count; i++)
            {
                if (intersectionData.intersectionList[i].type == IntersectionType.TrafficLight)
                {
                    if (intersectionData.intersectionList[i].sequenceList.Count > 0)
                    {
                        GameObject AITrafficLightManagerObject = new GameObject();
                        AITrafficLightManagerObject.transform.parent = this.transform;
                        AITrafficLightManagerObject.name = "AITrafficLightManager_" + i.ToString();
                        AITrafficLightManager _AITrafficLightManager = AITrafficLightManagerObject.AddComponent<AITrafficLightManager>();
                        spawnedLightManagers.Add(_AITrafficLightManager);
                        if (intersectionData.intersectionList[i].sequenceList.Count > 0)
                        {
                            _AITrafficLightManager.trafficLightCycles = new AITrafficLightCycle[intersectionData.intersectionList[i].sequenceList.Count];
                            for (int j = 0; j < intersectionData.intersectionList[i].sequenceList.Count; j++)
                            {
                                AITrafficLight[] lightArray = new AITrafficLight[intersectionData.intersectionList[i].sequenceList[j].sequenceList.Count];
                                for (int k = 0; k < lightArray.Length; k++)
                                {
                                    lightArray[k] = intersectionData.intersectionList[i].sequenceList[j].sequenceList[k].light;
                                }
                                _AITrafficLightManager.trafficLightCycles[j].trafficLights = lightArray;
                                _AITrafficLightManager.trafficLightCycles[j].greenTimer = greenTimer;
                                _AITrafficLightManager.trafficLightCycles[j].yellowTimer = yellowTimer;
                                _AITrafficLightManager.trafficLightCycles[j].redtimer = redTimer;
                            }
                        }
                    }
                }
            }
        }

        public void RefreshTrafficLightsAndStopSigns()
        {
            CiDyRoad[] cidyRoadArray = FindObjectsOfType<CiDyRoad>();
            for (int i = 0; i < cidyRoadArray.Length; i++)
            {
                bool wasUpdated = false;
                if (cidyRoadArray[i].lightType == CiDyRoad.LightType.StopSign)
                {
                    if (cidyRoadArray[i].stopSign != STSRefs.AssetReferences._StopSign)
                    {
                        cidyRoadArray[i].stopSign = STSRefs.AssetReferences._StopSign;
                        wasUpdated = true;
                    }
                }
                else if (cidyRoadArray[i].lightType == CiDyRoad.LightType.TrafficLight)
                {
                    if (cidyRoadArray[i].stopSign != STSRefs.AssetReferences._TrafficLight_1)
                    {
                        cidyRoadArray[i].stopSign = STSRefs.AssetReferences._TrafficLight_1;
                        cidyRoadArray[i].stopSignTwoLane = STSRefs.AssetReferences._TrafficLight_2;
                        cidyRoadArray[i].stopSignThreeLane = STSRefs.AssetReferences._TrafficLight_3;
                        wasUpdated = true;
                    }
                }
                if (wasUpdated)
                {
                    cidyRoadArray[i].Regenerate();
                }
            }
        }
#endif
#endif
    }
}