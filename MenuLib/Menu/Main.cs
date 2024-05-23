using HarmonyLib;
using MenuLib.MenuLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

                // Destroy reference
                GameObject.Destroy(menu.Reference);
                menu.Reference = null;

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
                if (menu != null)
                {
                    // Send logs
                    Log("Drawing menu");

                    // Draw menu
                    Draw(menu);

                    // Create menu reference
                    menu.Reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    menu.Reference.name = menu.ReferenceName;
                    menu.Reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    menu.Reference.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
                    menu.Reference.GetComponent<Renderer>().material.shader = textShader;

                    // Get reference parent
                    if (!GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        menu.ReferenceParent = menu.lefthand ? instance.rightControllerTransform.gameObject : instance.leftControllerTransform.gameObject;
                    }
                    else
                    {
                        menu.ReferenceParent = menu.lefthand ? GorillaTagger.Instance.rightHandTriggerCollider : GorillaTagger.Instance.leftHandTriggerCollider;
                    }
                }
            }

            if (stateDepender)
            {
                // Send logs
                Log("Updating menu");

                if (menu.pivot)
                {
                    Vector3 offset = Vector3.zero;
                    if (menu.ReferenceParent == instance.rightControllerTransform.gameObject || menu.ReferenceParent == instance.leftControllerTransform.gameObject)
                        offset = new Vector3(0, -0.1f, 0);

                    // Update reference
                    menu.Reference.transform.position = menu.ReferenceParent.transform.position + offset;

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
            background.transform.localScale = menu.Scale * GorillaLocomotion.Player.Instance.scale;
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
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Normal;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;

            // Implemented text rotation
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(0.28f, 0.05f);
            text.GetComponent<RectTransform>().position = new Vector3(0.06f, 0f, 0.175f);
            text.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            AddPageButtons(menu);
            
            Category currentCategory = menu.categories[menu.currentCategory];
            Button[] array = currentCategory.buttons.Skip(menu.currentPage * menu.ButtonsPerPage).Take(menu.ButtonsPerPage).ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                AddButton(menu.ButtonSpace * i, array[i], menu);
            }
            
        }

        private static void AddButton(float Offset, Button button, Menu menu)
        {
            // creates the button object
            GameObject buttonGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(buttonGO.GetComponent<Rigidbody>());
            buttonGO.GetComponent<BoxCollider>().isTrigger = true;
            buttonGO.transform.SetParent(menu.menuroot.transform, false);
            buttonGO.transform.localScale = new Vector3(0.09f, menu.Scale.y - menu.SpaceToSide * 2, 0.08f);
            buttonGO.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - Offset);
            buttonGO.AddComponent<ButtonCollider>().button = button;
            buttonGO.GetComponent<ButtonCollider>().menu = menu;

            // manages the button colors
            Color targetColor = button.ButtonType == "label" ? menu.label : button.Enabled ? menu.on : menu.off;
            buttonGO.GetComponent<Renderer>().material.SetColor("_Color", targetColor);

            // creates the text objects
            GameObject textObj = new GameObject();
            textObj.transform.parent = menu.Canvas.transform;
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.text = button.Title + button.Text;
            text.fontSize = 1;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;

            // initialize the text rect transform
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(0.2f, 0.03f);
            text.GetComponent<RectTransform>().localPosition = new Vector3(0.064f, 0f, 0.111f - Offset / 2.55f);
            text.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        private static void AddPageButtons(Menu menu)
        {
            // button variables
            float space = -menu.ButtonSpace;
            float calculatedSpace = menu.ButtonSpace * menu.ButtonsPerPage;
            string ButtonText = "<<<";

            for (int i = 0; i < 2; i++)
            {
                space += menu.ButtonSpace;

                // creates the button object
                GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject.Destroy(button.GetComponent<Rigidbody>());
                button.GetComponent<BoxCollider>().isTrigger = true;
                button.transform.SetParent(menu.menuroot.transform, false);
                button.transform.localScale = new Vector3(0.09f, menu.Scale.y - menu.SpaceToSide * 2, 0.08f);
                button.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - calculatedSpace);
                button.GetComponent<Renderer>().material.SetColor("_Color", menu.off);
                button.AddComponent<ButtonCollider>().button = Button.CreateButton(menu, ButtonText, "no_toggle", null, dontattachtocategory:true);
                button.GetComponent<ButtonCollider>().menu = menu;

                // creates the text objects
                GameObject textObj = new GameObject();
                textObj.transform.parent = menu.Canvas.transform;
                Text text = textObj.AddComponent<Text>();
                text.font = menu.font;
                text.text = ButtonText;
                text.fontSize = 1;
                text.alignment = TextAnchor.MiddleCenter;
                text.fontStyle = FontStyle.Normal;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 0;

                // initialize the text rect transform
                text.GetComponent<RectTransform>().sizeDelta = new Vector2(0.2f, 0.03f) * GorillaLocomotion.Player.Instance.scale;
                text.GetComponent<RectTransform>().localPosition = new Vector3(0.064f, 0f, 0.111f - calculatedSpace / 2.522522522522523f) * GorillaLocomotion.Player.Instance.scale;
                text.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                ButtonText = ">>>";
                calculatedSpace = menu.ButtonSpace * (menu.ButtonsPerPage + 1);
            }
        }

        // Get gorilla tag shaders
        public static Shader uberShader = Shader.Find("GorillaTag/UberShader");
        public static Shader textShader = Shader.Find("GUI/Text Shader");

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
        // Menu
        public GameObject menuroot;
        public bool lefthand = true;
        public GameObject pivot;
        public int ButtonsPerPage = 4;
        public float ButtonSpace = 0.13f;
        public float SpaceToSide = 0.05f;
        public Vector3 Scale;

        // Reference
        public GameObject Reference = null;
        public string ReferenceName;
        public GameObject ReferenceParent;

        // Text
        public string title;
        public Font font = Font.CreateDynamicFontFromOSFont("Agent FB", 20);
        public GameObject Canvas;

        // Colors
        public bool colorslider;
        public Color color;
        public Color[] colors;
        public Color label = Color.grey;
        public Color off = Color.red;
        public Color on = Color.green;

        public static Menu CreateMenu(string title, Color[] colors, Vector3 scale, bool leftHand = true)
        {
            Menu menu = new Menu();

            menu.title = title;
            menu.colorslider = true;
            menu.colors = colors;
            menu.Scale = scale;

            menu.lefthand = leftHand;
            if (leftHand)
            {
                menu.pivot = GorillaLocomotion.Player.Instance.leftControllerTransform.gameObject;
            }
            else
            {
                menu.pivot = GorillaLocomotion.Player.Instance.rightControllerTransform.gameObject;
            }

            menu.ReferenceName = Utillities.GenString(100);
            Category.CreateCategory(menu, "main");

            return menu;
        }

        public static Menu CreateMenu(string title, Color color, Vector3 scale, bool leftHand = true)
        {
            Menu menu = new Menu();

            menu.title = title;
            menu.colorslider = false;
            menu.color = color;
            menu.Scale = scale;

            menu.lefthand = leftHand;
            if (leftHand)
            {
                menu.pivot = GorillaLocomotion.Player.Instance.leftControllerTransform.gameObject;
            }
            else
            {
                menu.pivot = GorillaLocomotion.Player.Instance.rightControllerTransform.gameObject;
            }

            menu.ReferenceName = Utillities.GenString(100);
            Category.CreateCategory(menu, "main");

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

        public void RefreshMenu()
        {
            UnityEngine.Object.Destroy(this.menuroot);
            this.menuroot = null;
            UnityEngine.Object.Destroy(this.Reference);
            this.Reference = null;
        }

        // Page system
        public int currentPage = 0;

        // Category system
        public Dictionary<string, Category> categories = new Dictionary<string, Category>();
        public string currentCategory = "main";

        // Button system
        public Button[] EnabledButtons = new Button[] { };
    }
}
