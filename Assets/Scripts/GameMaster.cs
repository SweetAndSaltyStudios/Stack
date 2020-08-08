using UnityEngine;

namespace Sweet_And_Salty_Studios
{
    public class GameMaster : Singelton<GameMaster>
    {
        #region UNITY_FUNCTIONS

        private void Awake()
        {
            Initialize();
        }

        #endregion UNITY_FUNCTIONS

        #region CUSTOM_FUNCTIONS

        private void Initialize()
        {
           
        }

        #endregion CUSTOM_FUNCTIONS
    }
}