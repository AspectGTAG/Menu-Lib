using BepInEx;
using HarmonyLib;
using MenuLib.MenuLib.Menu;
using UnityEngine;
using System.Reflection;

namespace MenuLib.MenuLib.Plugin
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        // Version settings
        private const string modGUID = "aspect.cheat.panel";
        public const string modVersion = "7.0.0";
        private const string modName = "Aspect Cheat Panel";
        public const string menuName = modVersion;

        public void Awake()
        {
            new Harmony(modGUID).PatchAll(Assembly.GetExecutingAssembly());
        }

        // Menu
        bool runSetup = true;
        Menu.Menu menu;

        /// <summary>
        /// This is the update loop used for updating the menu.
        /// This contains 
        /// </summary>
        public void LateUpdate()
        {
            // Setup menu
            if (runSetup)
            {
                // Example of creating a new menu instance
                menu = Menu.Menu.CreateMenu(
                    menuName,
                    Color.black,
                    new Vector3(0.1f, 1f, 1f)
                );
                runSetup = false;

                // create a button in main category
                Button.CreateButton(menu, "label", "label", new System.Action[] { });
                Button.CreateButton(menu, "toggle", "toggle", new System.Action[] { () => Debug.Log("update") });
                Button.CreateButton(menu, "no_toggle", "no_toggle", new System.Action[] { () => Debug.Log("update") });

                // create a button in a new category
                Button.CreateButton(menu, "label", "label", new System.Action[] { }, category: "category_1");
                Button.CreateButton(menu, "toggle", "toggle", new System.Action[] { () => Debug.Log("update") }, category: "category_1");
                Button.CreateButton(menu, "no_toggle", "no_toggle", new System.Action[] { () => Debug.Log("update") }, category: "category_1");

                // creating a category inside a category
                Button.CreateButton(menu, "label", "label", new System.Action[] { }, category: "category_1:category_2");
                Button.CreateButton(menu, "toggle", "toggle", new System.Action[] { () => Debug.Log("update") }, category: "category_1:category_2");
                Button.CreateButton(menu, "no_toggle", "no_toggle", new System.Action[] { () => Debug.Log("update") }, category: "category_1:category_2");

                // creating a second category
                Button.CreateButton(menu, "label", "label", new System.Action[] { }, category: "category_2");
                Button.CreateButton(menu, "toggle", "toggle", new System.Action[] { () => Debug.Log("update") }, category: "category_1");
                Button.CreateButton(menu, "no_toggle", "no_toggle", new System.Action[] { () => Debug.Log("update") }, category: "category_1");
            }

            // Update menu
            MenuLib.Util.Input input = MenuLib.Util.Input.instance;
            Main.CallUpdate(input.CheckButton(MenuLib.Util.Input.ButtonType.secondary, menu.lefthand), menu);
        }
    }
}
