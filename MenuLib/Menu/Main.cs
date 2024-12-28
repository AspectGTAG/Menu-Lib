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
        public static bool logsEnabled = true;
        public static void Log(string message)
        {
            if (logsEnabled)
            {
                Debug.Log(message);
            }
        }

        public static void UpdateButtons(Menu menu)
        {
            for (int i = 0; i < menu.EnabledButtons.Count; i++)
            {
                Button button = menu.EnabledButtons[i];
                if (button != null && button.Enabled)
                {
                    try
                    { 
                        if (button.Actions.Length > 1)
                        {
                            button.Actions[1].Invoke();
                        }
                        else
                        {
                            button.Actions[0].Invoke();
                        }
                    }
                    catch (Exception e)
                    {
                        Log(e.Message);
                    }
                }
                else
                {
                    menu.EnabledButtons.Remove(button);
                }
            }
        }

        // Call this function when wanting to update a menu
        public static void CallUpdate(bool stateDepender, Menu menu, bool autoUpdateButtons = true, bool patched = true)
        {
            // Update buttons
            if (autoUpdateButtons)
                UpdateButtons(menu);

            // Get player instance
            GorillaLocomotion.Player instance = GorillaLocomotion.Player.Instance;

            if (!stateDepender && menu.menuroot != null || !patched && menu.menuroot != null)
            {
                // Send logs
                Log("Destroying menu");

                foreach (SphereCollider c_collider in menu.ReferenceParent.GetComponentsInChildren<SphereCollider>())
                {
                    if (c_collider.gameObject.name == menu.ReferenceName) // check if gameobject name is equal to reference name
                    {
                        GameObject.Destroy(c_collider);
                        menu.Reference = null;
                    }
                }

                if (Menu.FallPhysics)
                {
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
                    GameObject.Destroy(menu.menuroot, Menu.DestroyDelay);
                    menu.menuroot = null;
                }
                else
                {
                    // Destroy menu instantly
                    GameObject.Destroy(menu.menuroot);
                    menu.menuroot = null;
                }
            }

            else if (stateDepender && menu.menuroot == false)
            {
                if (menu != null)
                {
                    // Send logs
                    Log("Drawing menu");

                    // Draw menu
                    Draw(menu);

                    // TODO: fix reference
                    // Create menu reference
                    menu.Reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    menu.Reference.GetComponent<Renderer>().name = menu.ReferenceName;

                    // Get reference parent
                    if (!GorillaTagger.Instance.offlineVRRig.enabled)
                    {
                        menu.ReferenceParent = menu.lefthand ? instance.rightControllerTransform.gameObject : instance.leftControllerTransform.gameObject;
                        menu.ReferenceOffset = new Vector3(0, -0.1f, 0);
                    }
                    else
                    {
                        menu.ReferenceParent = menu.lefthand ? GorillaTagger.Instance.rightHandTriggerCollider : GorillaTagger.Instance.leftHandTriggerCollider;
                        menu.ReferenceOffset = Vector3.zero;
                    }

                    menu.Reference.transform.parent = menu.ReferenceParent.transform;
                    menu.Reference.transform.position = menu.ReferenceOffset;
                    menu.Reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    GameObject.Destroy(menu.Reference.GetComponent<Renderer>());
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
                    if (!menu.lefthand && menu.pivot == instance.rightControllerTransform.gameObject)
                    {
                        menu.menuroot.transform.RotateAround(menu.menuroot.transform.position, menu.menuroot.transform.forward, 180f);
                    }
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
            Category currentCategory = menu.categories[menu.currentCategory];
            float pagecount_f = (currentCategory.buttons.Count - 1) / Menu.ButtonsPerPage;
            int currentPage = currentCategory.currentPage;
            text.text = menu.title + $" [{currentPage+1}:{(int)Math.Ceiling(pagecount_f)+1}]";
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

            // Add page buttons to page
            AddPageButtons(menu);
            
            // Add buttons to page
            Button[] array = currentCategory.buttons.Skip(currentPage * Menu.ButtonsPerPage).Take(Menu.ButtonsPerPage).ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                AddButton(Menu.ButtonSpace * i, array[i], menu);
            }
        }

        private static void AddButton(float Offset, Button button, Menu menu)
        {
            // creates the button object
            GameObject buttonGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(buttonGO.GetComponent<Rigidbody>());
            buttonGO.GetComponent<Collider>().isTrigger = true;
            buttonGO.transform.SetParent(menu.menuroot.transform, false);
            buttonGO.transform.localScale = new Vector3(0.09f, menu.Scale.y - Menu.SpaceToSides * 2, 0.08f);
            buttonGO.transform.localPosition = new Vector3(0.56f, 0f, Menu.DefaultButtonOffset - Offset);
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
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(0.2f, 0.03f) * GorillaLocomotion.Player.Instance.scale;
            text.GetComponent<RectTransform>().localPosition = new Vector3(0.064f, 0f, 0.111f - Offset / Menu.TextDivider) * GorillaLocomotion.Player.Instance.scale;
            text.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        private static void AddPageButtons(Menu menu)
        {
            for (int i = 0; i < 2; i++)
            {
                // Create button go
                GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject.Destroy(button.GetComponent<Rigidbody>());
                button.GetComponent<BoxCollider>().isTrigger = true;
                button.transform.SetParent(menu.menuroot.transform, false);
                button.transform.localScale = new Vector3(0.09f, menu.Scale.y - Menu.SpaceToSides * 2, 0.08f);
                button.transform.localPosition = new Vector3(0.56f, 0f, Menu.DefaultButtonOffset - Menu.ButtonSpace * i - Menu.ButtonSpace * Menu.ButtonsPerPage);
                button.GetComponent<Renderer>().material.SetColor("_Color", menu.off);

                string ButtonText = "";
                string ButtonType = "";
                if (i == 0)
                {
                    ButtonText = ">>>";
                    ButtonType = "page_button_right";
                }
                else
                {
                    ButtonText = "<<<";
                    ButtonType = "page_button_left";
                }

                button.AddComponent<ButtonCollider>().button = Button.CreateButton(menu, ButtonText, ButtonType, null, dontattachtocategory: true);
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
                text.GetComponent<RectTransform>().localPosition = new Vector3(0.064f, 0f, 0.111f - Menu.ButtonSpace * (Menu.ButtonsPerPage + 1 * i) / Menu.TextDivider) * GorillaLocomotion.Player.Instance.scale;
                text.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
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
        public Vector3 Scale;

        // Constant menu settings
        public static int ButtonsPerPage { get; internal set; } = 4;
        public static float ButtonSpace { get; internal set; } = 0.13f;
        public static float SpaceToSides { get; internal set; } = 0.1f;
        public static float DefaultButtonOffset { get; internal set; } = 0.28f;
        public static float TextDivider { get; internal set; } = 2.55f; // a more acurate version is 2.523f
        public static bool FallPhysics { get; internal set; } = false;
        public static float DestroyDelay { get; internal set; } = 2;

        // Reference
        public GameObject Reference = null;
        public string ReferenceName;
        public GameObject ReferenceParent;
        public Vector3 ReferenceOffset;

        // Text
        public string title;
        public Font font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
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
            Category.CreateCategory(menu, mainPath);

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
            Category.CreateCategory(menu, mainPath);

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
            // Destroys the menuroot to re-draw
            if (this.menuroot)
                UnityEngine.Object.Destroy(this.menuroot);
        }
        
        // Category system
        public const string mainPath = "main";
        public Dictionary<string, Category> categories = new Dictionary<string, Category>();
        public string currentCategory = mainPath;

        // Button system
        public List<Button> EnabledButtons = new List<Button>();
    }
}
