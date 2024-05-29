using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    /// <summary>
    /// Class defining a flag target
    /// </summary>
    public class FlagTarget : Obstacle, IRelativelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.FlagTarget;

        public override bool canRespawnOn => false;

        public override bool canRollOn => false;

        public override bool canKill => false;

        public int obstacleId { get; set; }

        public CardinalDirection direction { get; set; }

        public FlagTarget(int id, int obstacleId, CardinalDirection direction) : base(id)
        {
            this.obstacleId = obstacleId;
            this.direction = direction;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject flagTarget = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/FlagTarget.prefab", typeof(GameObject)));
            flagTarget.name = "Target";

            PositionObstacleOverObstacleFromId(obstacles, flagTarget.transform, obstacleId, new Vector3(0, 0.065f, 0));

            return flagTarget;
        }
    }
}