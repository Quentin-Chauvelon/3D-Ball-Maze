using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    public class HalfSphere : Obstacle, IAbsolutelyPositionnable
    {
        public override ObstacleType obstacleType => ObstacleType.HalfSphere;

        public override bool canRespawnOn => false;

        public override bool canKill => false;

        public Vector3 position { get; set; }


        public HalfSphere(int id, Vector3 position) : base(id)
        {
            this.position = position;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles)
        {
            GameObject halfsphere = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/HalfSphere.fbx", typeof(GameObject)));
            halfsphere.name = "HalfSphere";

            halfsphere.transform.position = position;

            halfsphere.transform.Find("HalfSphere_Base").GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/BaseObstacle.mat", typeof(Material));
            halfsphere.transform.Find("HalfSphere_Sphere").GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/BaseObstacle.mat", typeof(Material));

            return halfsphere;
        }
    }
}