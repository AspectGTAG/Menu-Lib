using HarmonyLib;
using System;
using UnityEngine;

namespace MenuLib.MenuLib.Menu
{
    public class Button
    {
        public bool Enabled { get; internal set; } // Check if button is enabled
        public string ButtonType { get; private set; } // what type of button it is ("toggle", "no_toggle", "label")
        public string Title { get; private set; } // Title for button
        public string Text { get; set; } // Extra text displayed after title
        public string ToolTip { get; private set; } // Description of what the button is used for
        public Action[] Actions { get; private set; } // Actions for button { OnEnable, OnUpdate, OnDisable }
        public string CategoryPath { get; internal set; } // Path to current category of button
        public string OriginalPath { get; private set; } // Path to original category

        public static Button CreateButton(Menu menu, string title, string type, Action[] actions, string category = "main", string extraText = "", string ToolTip = "", bool dontattachtocategory = false)
        {
            Button button = new Button();

            button.Title = title;
            button.Text = extraText;
            button.ButtonType = type;
            button.Actions = actions;

            if (!dontattachtocategory)
            {
                if (category != "main")
                    category = $"main:{category}";

                if (!menu.categories.ContainsKey(category))
                {
                    Category n_category;
                    n_category = Category.CreateCategory(menu, category);
                    n_category.buttons.Add(button);
                }
                else
                {
                    menu.categories[category].buttons.Add(button);
                }
            }

            return button;
        }

        public void ChangeButtonCategory(string category)
        {
            // make code that changes the current category of a button
        }
    }

    internal class ButtonCollider : MonoBehaviour
    {
        public Button button;
        public Menu menu;
        static float PressCooldown = 0;

        public void OnTriggerEnter(Collider collider)
        {
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, false, 0.5f);
            /*
            if (collider.gameObject.name == menu.ReferenceName && button.ButtonType != "label" && Time.frameCount >= PressCooldown + 30f)
            {
                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, false, 0.5f);
                menu.RefreshMenu();
                PressCooldown = Time.frameCount;
            }
            */
        }
    }
}
