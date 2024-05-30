using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    public enum TunnelType
    {
        TwoWay,
        TwoWayCornered,
        ThreeWay,
        FourWay
    }


    public class Tunnel : Obstacle, IAbsolutelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.Tunnel;

        public override bool canRespawnOn => true;

        public override bool canRollOn => true;

        public override bool canKill => false;

        public Vector3 position { get; set; }

        [JsonProperty("tt")]
        public TunnelType tunnelType;

        public CardinalDirection direction { get; set; }


        public Tunnel(int id, TunnelType tunnelType, Vector3 position, CardinalDirection direction) : base(id)
        {
            this.position = position;
            this.direction = direction;
            this.tunnelType = tunnelType;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject tunnel;

            if (Application.isPlaying)
            {
                switch (tunnelType)
                {
                    case TunnelType.TwoWay:
                        tunnel = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/tunnels/2waytunnel.fbx");
                        break;
                    case TunnelType.TwoWayCornered:
                        tunnel = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/tunnels/2waycornertunnel.fbx");
                        break;
                    case TunnelType.ThreeWay:
                        tunnel = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/tunnels/3waytunnel.fbx");
                        break;
                    case TunnelType.FourWay:
                        GameObject test = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/tunnels/4waytunnel.fbx");
                        Debug.Log("test" + test);
                        tunnel = test;
                        break;
                    default:
                        tunnel = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/tunnels/2waytunnel.fbx");
                        break;
                }
            }
            else
            {
#if UNITY_EDITOR
                switch (tunnelType)
                {
                    case TunnelType.TwoWay:
                        tunnel = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Tunnels/2WayTunnel.fbx", typeof(GameObject)));
                        break;
                    case TunnelType.TwoWayCornered:
                        tunnel = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Tunnels/2WayCornerTunnel.fbx", typeof(GameObject)));
                        break;
                    case TunnelType.ThreeWay:
                        tunnel = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Tunnels/3WayTunnel.fbx", typeof(GameObject)));
                        break;
                    case TunnelType.FourWay:
                        tunnel = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Tunnels/4WayTunnel.fbx", typeof(GameObject)));
                        break;
                default:
                    tunnel = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Tunnels/2WayTunnel.fbx", typeof(GameObject)));
                    break;
            }
#else
                return null;
#endif
            }

            tunnel.name = "Tunnel";

            tunnel.transform.position = position;
            tunnel.transform.rotation = Quaternion.Euler(0, (int)direction * 90, 0);

            Transform tunnelFloor = tunnel.transform.Find("Tunnel_Floor");
            Transform tunnelExterior = tunnel.transform.Find("Tunnel_Exterior");
            Transform tunnelInterior = tunnel.transform.Find("Tunnel_Interior");

            tunnelFloor.GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/baseobstacle.mat");
            tunnelExterior.GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/tunnel/tunnelexterior.mat");
            tunnelInterior.GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/tunnel/tunnelinterior.mat");

            return tunnel;
        }
    }
}
