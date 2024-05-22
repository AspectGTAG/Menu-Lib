using UnityEngine;

namespace MenuLib.MenuLib.Menu
{
    public class Button
    {
    }

    public class ButtonCollider : MonoBehaviour
    {
        public Button button;
        Menu menu;
        static float PressCooldown = 0;

        public void OnTriggerEnter(Collider collider)
        {
            
        }
    }
}
