using HarmonyLib;
using System;
using UnityEngine;

namespace MenuLib.MenuLib.Menu
{
    public class Button
    {
        public bool Enabled { get; internal set; } // Check if button is enabled
        public string ButtonType { get; private set; } // what type of button it is ("toggle", "no_toggle", "label", "page_button_left", "page_button_right")
        public string Title { get; private set; } // Title for button
        public string Text { get; set; } // Extra text displayed after title
        public string ToolTip { get; private set; } // Description of what the button is used for
        public Action[] Actions { get; private set; } // Actions for button { OnEnable, OnUpdate, OnDisable }
        public string CategoryPath { get; internal set; } // Path to current category of button
        public string OriginalPath { get; private set; } // Path to original category

        public static Button CreateButton(Menu menu, string title, string type, Action[] actions, string category = Menu.mainPath, string extraText = "", string ToolTip = "", bool dontattachtocategory = false)
        {
            Button button = new Button();

            button.Title = title;
            button.Text = extraText;
            button.ButtonType = type;
            button.ToolTip = ToolTip;
            button.Actions = actions;

            if (!dontattachtocategory)
            {
                if (category != Menu.mainPath)
                    category = $"{Menu.mainPath}:{category}";

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
            // TODO: make code that changes the current category of a button
        }
    }

    internal class ButtonCollider : MonoBehaviour
    {
        public Button button;
        public Menu menu;
        static float PressCooldown = 0;

        public void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.name == menu.ReferenceName && button.ButtonType != "label" && Time.frameCount >= PressCooldown + 30f)
            {
                // Play click sound
                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, false, 0.5f);

                // Page buttons
                Category category = menu.categories[menu.currentCategory];
                float pagecount_f = (category.buttons.Count - 1) / Menu.ButtonsPerPage;
                if (button.ButtonType == "page_button_left")
                {
                    if (category.currentPage > 0)
                    {
                        category.currentPage--;
                    }
                    else
                    {
                        category.currentPage = (int)Math.Ceiling(pagecount_f);
                    }
                } 
                else if (button.ButtonType == "page_button_right") 
                {
                    if (category.currentPage < (int)Math.Ceiling(pagecount_f))
                    {
                        category.currentPage++;
                    }
                    else
                    {
                        category.currentPage = 0;
                    }
                }
                else
                {
                    // Get actions
                    Action OnEnable = null;
                    Action OnUpdate = null;
                    Action OnDisable = null;
                    if (button.Actions.Length > 1)
                    {
                        OnEnable = button.Actions[0];
                        OnUpdate = button.Actions[1];
                        OnDisable = button.Actions[2];
                    }
                    else if (button.Actions.Length == 1)
                    {
                        OnUpdate = button.Actions[0];
                    }

                    if (button.ButtonType == "toggle")
                    {
                        button.Enabled = !button.Enabled;
                        switch (button.Enabled)
                        {
                            case true:
                                menu.EnabledButtons.Add(button);
                                break;
                            case false:
                                menu.EnabledButtons.Remove(button);
                                break;
                        }
                    }

                    bool n_t = button.ButtonType == "no_toggle";
                    try
                    {
                        if (OnEnable != null)
                        {
                            if (n_t || button.Enabled)
                            {
                                OnEnable.Invoke();
                            }
                        }
                        if (OnUpdate != null)
                        {
                            if (n_t || button.Enabled)
                            {
                                OnUpdate.Invoke();
                            }
                        }
                        if (OnDisable != null)
                        {
                            if (n_t || !button.Enabled)
                            {
                                OnDisable.Invoke();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Main.Log(e.Message);
                    }
                }
                
                menu.RefreshMenu();
                PressCooldown = Time.frameCount;
            }
        }
    }
}
