using UnityEngine;

namespace Sweet_And_Salty_Studios
{
    public class InputManager : Singelton<InputManager>
    {
        public bool IsMouseDown(int index)
        {
            return Input.GetMouseButtonDown(index);
        }

        public bool IsMouseUp(int index)
        {
            return Input.GetMouseButtonUp(index);
        }
    }
}