using System.Collections.Generic;

namespace MenuLib.MenuLib.Menu
{
    public class Category
    {
        public string Name { get; private set; } // Name of category
        public string Description { get; private set; } // Description of this category (optional)
        public string Path { get; private set; } // Path to this category

        public List<Category> categories { get; private set; } = new List<Category>(); // Categories inside this category
        public List<Button> buttons { get; private set; } = new List<Button>(); // Buttons inside this category

        public int currentPage = 0;

        public static Category CreateCategory(Menu menu, string path, string description = "")
        {
            // Cancel creation of category if path is taken
            if (menu.categories.ContainsKey(path))
                return null;

            // Create category
            Category category = new Category();

            // Get name
            string[] splitPath = path.Split(':');
            string name = splitPath[splitPath.Length - 1];

            // Set name, description and path
            category.Name = name;
            category.Description = description;
            category.Path = path;

            // Add category to menu
            menu.categories.Add(path, category);

            if (path != Menu.mainPath)
            {
                // Get parent path
                string parent_path = path.Replace($":{name}", "");

                // Add category to parent category
                menu.categories[parent_path].categories.Add(category);

                // Create category button
                Button cat_button = Button.CreateButton(menu, name, "no_toggle", new System.Action[] { () => { menu.currentCategory = path; } }, dontattachtocategory: true);
                menu.categories[parent_path].buttons.Add(cat_button);

                // Add return button to current category - attaching it manually because it doesn't attach automaticly for some reason
                string parent_cat_n = splitPath[splitPath.Length - 2];
                Button return_btn = Button.CreateButton(menu, $"Return To {parent_cat_n}", "no_toggle", new System.Action[] { () => { menu.currentCategory = parent_path; } }, dontattachtocategory: true);
                category.buttons.Add(return_btn);
            }
            
            // return new category
            return category;
        }
    }
}
