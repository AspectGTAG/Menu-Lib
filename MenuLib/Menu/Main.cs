using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UI;

namespace MenuLib.MenuLib.Menu
{
    public static class Main
    {
        public static bool logMessages = true;
        public static void Log(string message)
        {
            if (logMessages)
            {
                Debug.Log(message);
            }
        }

        public static void CallUpdate(bool stateDepender, Menu menu, bool patched = true) // Call this function when wanting to update a menu
        {
            // Get player instance
            GorillaLocomotion.Player instance = GorillaLocomotion.Player.Instance;

            if (!stateDepender && menu.menuroot != null || !patched && menu.menuroot != null)
            {
                // Send logs
                Log("Destroying menu");

                // Create menuroot rigidbody
                try
                {
                    // Add rigidbody to menuroot
                    Rigidbody menuRB = menu.menuroot.AddComponent<Rigidbody>();

                    // Destroy colliders of menuroots children
                    foreach (Collider collider in menu.menuroot.GetComponentsInChildren<Collider>())
                    {
                        GameObject.Destroy(collider);
                    }

                    // Add velocity to rigidbody
                    if (menu.lefthand)
                    {
                        menuRB.velocity = instance.leftHandCenterVelocityTracker.GetAverageVelocity(true, 0);
                        menuRB.angularVelocity = GameObject.Find("TurnParent/LeftHand Controller").GetComponent<GorillaVelocityEstimator>().angularVelocity;
                    }
                    else
                    {
                        menuRB.velocity = instance.rightHandCenterVelocityTracker.GetAverageVelocity(true, 0);
                        menuRB.angularVelocity = GameObject.Find("TurnParent/RightHand Controller").GetComponent<GorillaVelocityEstimator>().angularVelocity;
                    }
                }

                // Catch any errors
                catch (Exception ex)
                {
                    Log(ex.Message);
                }

                // Destroy Menu
                GameObject.Destroy(menu.menuroot, 1);
                menu.menuroot = null;
            }

            else if (stateDepender && menu.menuroot == false)
            {
                // Send logs
                Log("Drawing menu");

                // Draw menu
                Draw(menu);
            }

            if (stateDepender)
            {
                // Send logs
                Log("Updating menu");

                if (menu.pivot)
                {
                    // Update menuroot transform
                    menu.menuroot.transform.position = menu.pivot.transform.position;
                    menu.menuroot.transform.rotation = menu.pivot.transform.rotation;

                    // Rotate menu around pivot if pivot is right hand
                    //if (!menu.lefthand && menu.pivot == instance.rightControllerTransform.gameObject)
                    //{
                    //    menu.menuroot.transform.RotateAround(menu.menuroot.transform.position, menu.menuroot.transform.forward, 180f);
                    //}
                }
            }
        }

        private static void Draw(Menu menu) // Draws the menu
        {
            if (menu == null) return; // Return if menu is empty

            // Create menuroot
            Log("Creating MenuRoot");
            menu.menuroot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(menu.menuroot.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(menu.menuroot.GetComponent<BoxCollider>());
            UnityEngine.Object.Destroy(menu.menuroot.GetComponent<Renderer>());
            menu.menuroot.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f) * GorillaLocomotion.Player.Instance.scale;

            // Create menubackground
            Log("Creating BG");
            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(background.GetComponent<Rigidbody>());
            GameObject.Destroy(background.GetComponent<Collider>());
            background.transform.SetParent(menu.menuroot.transform, false);
            background.transform.localScale = new Vector3(0.1f, 1f, 1f);
            background.transform.position = new Vector3(0.05f, 0f, 0f) * GorillaLocomotion.Player.Instance.scale;
            background.GetComponent<Renderer>().material.SetColor("_Color", menu.color);

            // Create canvas
            Log("Creating Canvas");
            menu.Canvas = new GameObject();
            menu.Canvas.transform.parent = menu.menuroot.transform;
            Canvas canvas = menu.Canvas.AddComponent<Canvas>();
            CanvasScaler canvasScaler = menu.Canvas.AddComponent<CanvasScaler>();
            menu.Canvas.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = 3000f / GorillaLocomotion.Player.Instance.scale;

            // Create title text
            Log("Creating Text");
            GameObject textObj = new GameObject();
            textObj.transform.parent = menu.Canvas.transform;
            Text text = textObj.AddComponent<Text>();
            text.font = menu.font;
            text.text = menu.title + " [" + menu.currentPage.ToString() + "]";
            text.color = Color.white;
            text.fontSize = 1;
            text.fontStyle = FontStyle.BoldAndItalic;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
        }

        // Get gorilla tag shader
        public static Shader uberShader = Shader.Find("GorillaTag/UberShader");

        // Change shader to gorilla tag shader to fix rendering problems
        [HarmonyPatch(typeof(Material), "SetColor", new[] { typeof(string), typeof(Color) })]
        internal class GameObjectRenderFixer
        {
            private static void Prefix(Material __instance, string name, Color value)
            {
                if (name == "_Color")
                {
                    __instance.shader = uberShader;
                    __instance.color = value;
                    return;
                }
            }
        }
    }

    public class Menu
    {
        // Menu settings
        public bool lefthand = true;
        public GameObject pivot;

        // GameObjects
        public GameObject menuroot;

        // Text
        public string title;
        public Font font = Font.CreateDynamicFontFromOSFont("Agent FB", 20);
        public GameObject Canvas;

        // Colors
        public bool colorslider;
        public Color color;
        public Color[] colors;
        public Color off;
        public Color on;

        public static Menu CreateMenu(string title, Color[] colors, GameObject customPivot = null)
        {
            Menu menu = new Menu();

            menu.title = title;
            menu.colorslider = true;
            menu.colors = colors;

            if (!customPivot)
            {
                menu.pivot = GorillaLocomotion.Player.Instance.leftControllerTransform.gameObject;
            }
            else
            {
                menu.pivot = customPivot;
            }

            return menu;
        }

        public static Menu CreateMenu(string title, Color color, GameObject customPivot = null)
        {
            Menu menu = new Menu();

            menu.title = title;
            menu.colorslider = false;
            menu.color = color;

            if (!customPivot)
            {
                menu.pivot = GorillaLocomotion.Player.Instance.leftControllerTransform.gameObject;
            }
            else
            {
                menu.pivot = customPivot;
            }

            return menu;
        }

        public void ChangeHand(bool n_lefthand, GameObject n_pivot = null)
        {
            // Get player instance
            GorillaLocomotion.Player instance = GorillaLocomotion.Player.Instance;

            // Change hand
            lefthand = n_lefthand;

            // Change pivot
            if (n_pivot == null)
            {
                if (lefthand)
                {
                    pivot = instance.leftControllerTransform.gameObject;
                }
                else
                {
                    pivot = instance.rightControllerTransform.gameObject;
                }
            }
            else
            {
                pivot = n_pivot;
            }
        }

        // Page system
        public int currentPage = 0;
    }
}
