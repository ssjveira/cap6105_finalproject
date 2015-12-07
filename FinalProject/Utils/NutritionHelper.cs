using System;
using NutritionalInfoApp.Properties;
using Nutritionix;

namespace NutritionalInfoApp.Utils
{
    /// <summary>
    /// Helper class created from help from the C# wrapper library for the Nutritionix REST API.  
    /// Provides a set of static methods that aid in retrieving data from the Nutritionix database.
    /// </summary>
    public static class NutritionHelper
    {
        private static readonly string AppId;
        private static readonly string AppKey;

        static NutritionHelper()
        {
            AppId = Settings.Default.nutritionixAppId;
            AppKey = Settings.Default.nutritionixAppKey;
        }

        /// <summary>
        /// Search for items from the Nutritionix database.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="itemType"></param>
        public static SearchResponse Search(string query)
        {
            if(query.Equals(""))
                return new SearchResponse();

            Console.WriteLine(@"Performing search on Nutritionix database for '" + query + @"'...");

            var nutritionix = new NutritionixClient();
            nutritionix.Initialize(AppId, AppKey);

            var request = new SearchRequest { Query = query };
            
            // Return the search response
            return nutritionix.SearchItems(request);
        }

    }
}