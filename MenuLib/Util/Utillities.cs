using UnityEngine;

namespace MenuLib.MenuLib.Util
{
    /// <summary>
    /// This class contains tools that can be used throughout the menu.
    /// </summary>
    public static class Utillities
    {
        // Function to create a random string
        public static string GenString(int length, string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789")
        {
            string finalString = "";

            for (int i = 0; i < length; i++)
            {
                char caracterAtRandomIndex = letters[Random.Range(1, letters.Length)];
                finalString += caracterAtRandomIndex;
            }

            return finalString;
        }
    }
}
