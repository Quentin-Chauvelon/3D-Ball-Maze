using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    public class Spikes : Obstacle, IRelativelyPositionnable
    {
        public override ObstacleType obstacleType => ObstacleType.Spikes;

        public override bool canRespawnOn => false;

        public override bool canRollOn => false;

        public override bool canKill => true;

        public int obstacleId { get; set; }


        public Spikes(int id, int obstacleId) : base(id)
        {
            this.obstacleId = obstacleId;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject spikes = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Spikes.fbx", typeof(GameObject)));
            spikes.name = "Spikes";

            PositionObstacleOverObstacleFromId(obstacles, spikes.transform, obstacleId, Vector3.zero);

            spikes.transform.Find("Spikes_SpikesBases").GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/Spikes/SpikesBase.mat", typeof(Material));
            spikes.transform.Find("Spikes_Spikes").GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/Spikes/Spikes.mat", typeof(Material));

            GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trigger.transform.localScale = new Vector3(0.75f, 0.3f, 0.75f);
            trigger.transform.position = spikes.transform.position + new Vector3(0, 0.275f, 0);
            trigger.GetComponent<MeshRenderer>().enabled = false;
            trigger.GetComponent<BoxCollider>().isTrigger = true;
            trigger.transform.parent = spikes.transform;

            return spikes;
        }
    }
}