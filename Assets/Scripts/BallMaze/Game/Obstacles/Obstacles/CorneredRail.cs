using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    [Serializable]
    public class CorneredRail : Obstacle, IAbsolutelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.CorneredRail;

        public override bool canRespawnOn => true;

        public override bool canRollOn => true;

        public override bool canKill => false;

        public Vector3 position { get; set; }

        public CardinalDirection direction { get; set; }


        public CorneredRail(int id, Vector3 position, CardinalDirection direction) : base(id)
        {
            this.position = position;
            this.direction = direction;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles)
        {
            GameObject corneredRail = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/CorneredRail.fbx", typeof(GameObject)));
            corneredRail.name = "CorneredRail";

            corneredRail.transform.position = position;
            corneredRail.transform.rotation = Quaternion.Euler(0, (int)direction * 90, 0);

            corneredRail.transform.Find("CorneredRail_InteriorRail").GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/Rail/Rail.mat", typeof(Material));
            corneredRail.transform.Find("CorneredRail_ExteriorRail").GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/Rail/Rail.mat", typeof(Material));

            return corneredRail;
        }
    }
}