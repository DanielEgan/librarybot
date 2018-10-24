using Microsoft.Bot.Builder.Luis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace librarybot
{
    public sealed class LuisModelDataAttribute : LuisModelAttribute
    {
        public LuisModelDataAttribute()
        : base(modelID: ConfigurationManager.AppSettings["LUISModelID"], subscriptionKey: ConfigurationManager.AppSettings["LUISSubscriptionID"]) { }
    }
}