using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze
{
    public interface IDataPersistence
    {
        void LoadData(PlayerData data);

        void SaveData(PlayerData data);
    }
}