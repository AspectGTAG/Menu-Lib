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
        /// 
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

                // making buttons in "main" category
                Button.CreateButton(menu, "label", "label", new System.Action[] { });
                Button.CreateButton(menu, "toggle", "toggle", new System.Action[] { () => Debug.Log("update") });
                Button.CreateButton(menu, "no_toggle", "no_toggle", new System.Action[] { () => Debug.Log("update") });

                // making buttons in a new category
                Button.CreateButton(menu, "test_cat", "toggle", new System.Action[] { () => Debug.Log("update") }, category: "new_category");
                Button.CreateButton(menu, "test_btn", "toggle", new System.Action[] { }, category: "new_category");

                // creating a category inside a category
                // currently buggy, fixing soon

                // creating a second category
                Button.CreateButton(menu, "test_cat2", "toggle", new System.Action[] { () => Debug.Log("update") }, category: "new_category2");
            }

            // Update menu
            MenuLib.Util.Input input = MenuLib.Util.Input.instance;
            Main.CallUpdate(input.CheckButton(MenuLib.Util.Input.ButtonType.secondary, menu.lefthand), menu);
        }
    }
}
