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


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles)
        {
            GameObject tunnel;
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

            tunnel.name = "Tunnel";

            tunnel.transform.position = position;
            tunnel.transform.rotation = Quaternion.Euler(0, (int)direction * 90, 0);

            Transform tunnelFloor = tunnel.transform.Find("Tunnel_Floor");
            Transform tunnelExterior = tunnel.transform.Find("Tunnel_Exterior");
            Transform tunnelInterior = tunnel.transform.Find("Tunnel_Interior");

            tunnelFloor.GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/BaseObstacle.mat", typeof(Material));
            tunnelExterior.GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/Tunnel/TunnelExterior.mat", typeof(Material));
            tunnelInterior.GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/Tunnel/TunnelInterior.mat", typeof(Material));

            return tunnel;
        }
    }
}
