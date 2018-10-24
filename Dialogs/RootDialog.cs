using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;


namespace librarybot.Dialogs
{
    [Serializable]
    [LuisModel ("dfbef095-b88d-4012-a2c9-3df7ebb95c4d", "470501b4f7274b45b44c445870e6e583", LuisApiVersion.V2, Verbose=true, SpellCheck =true)]
    public class RootDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("FindEvent")]
        public async Task FindEvent(IDialogContext context, LuisResult result)
        {
            string message = $"Find Event'{result.Query}'";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("FindLibrary")]
        public async Task FindLibrary(IDialogContext context, LuisResult result)
        {
            //EntityRecommendation cityEntityRecommendation;

            //if (result.TryFindEntity(EntityGeographyCity, out cityEntityRecommendation))
            //{
            //    cityEntityRecommendation.Type = "Destination";
            //}
            //else
            //{
                string message = $"Find Library '{result.Query}'";
                await context.PostAsync(message);
           // }
             context.Wait(MessageReceived);

        }
        [LuisIntent("FindMaterial")]
        public async Task FindMaterial(IDialogContext context, LuisResult result)
        {
            string message = $"Find Material '{result.Query}'";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        //public Task StartAsync(IDialogContext context)
        //{
        //    context.Wait(MessageReceivedAsync);

        //    return Task.CompletedTask;
        //}

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            await context.PostAsync($"You sent {activity.Text} which was {length} characters");

            context.Wait(MessageReceivedAsync);
        }
    }
}