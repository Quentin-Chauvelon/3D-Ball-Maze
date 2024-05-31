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

        // Tunnels aren't respawnable since they are higher than other obstacles, the player
        // would respawn above the tunnel with the current system. Furthermore, if the player
        // respanws in a tunnel, they wouldn't know where they are.
        public override bool canRespawnOn => false;

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
                        tunnel = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/tunnels/4waytunnel.fbx");
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

            if (Application.isPlaying)
            {
                tunnel.transform.Find("Tunnel_Floor").GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/baseobstacle.mat");
                tunnel.transform.Find("Tunnel_Exterior").GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/tunnel/tunnelexterior.mat");
                tunnel.transform.Find("Tunnel_Interior").GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/tunnel/tunnelinterior.mat");

                // Mark the tunnel itself as Ignore Raycast, this is used by the ball collision
                // detection to ignore the tunnel itself, otherwise the ball could be moved
                // back on top of the tunnel when falling off the maze
                tunnel.transform.Find("Tunnel_Interior").gameObject.layer = 2;
                tunnel.transform.Find("Tunnel_Exterior").gameObject.layer = 2;
            }

            return tunnel;
        }
    }
}
