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
        public const string modVersion = "6.2.6";
        private const string modName = "Aspect Cheat Panel";
        public const string menuName = modVersion;

        public void Awake()
        {
            new Harmony(modGUID).PatchAll(Assembly.GetExecutingAssembly());
        }

        // Menu
        bool runSetup = true;
        Menu.Menu menu;

        // Update loop
        public void LateUpdate()
        {
            // Setup menu
            if (runSetup)
            {
                // Create menu
                menu = Menu.Menu.CreateMenu(
                    menuName,
                    Color.black
                );
                runSetup = false;
            }

            // Update menu
            MenuLib.Util.Input input = MenuLib.Util.Input.instance;
            Main.CallUpdate(input.CheckButton(MenuLib.Util.Input.ButtonType.secondary, menu.lefthand), menu);
        }
    }
}
