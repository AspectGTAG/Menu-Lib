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
            
            // return new category
            return category;
        }
    }
}
