using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;




namespace librarybot.Dialogs
{   
    //Normally we would add an attribute for LUIS here.  But we dont want these keys
    //checked into source control. All keys are in AppSettingsSecrets which
    //is referenced in the AppSettings section of Web.config but the file
    //AppSettingsSecrets is exclueded from get.   For LUIS something diferent 
    //needed to be done because you cannot pull a string from configuration 
    //into an attrubute. This is done through the LuisModelDataAttributes.cs file
    //[LuisModel("modeID", "key")]
    [Serializable]
    [LuisModelData]
    public class RootScoreDialog : DispatchDialog
    {
        //const string filePath = ConfigurationManager.AppSettings["LUISModelID"];
        //ScorableGroupAttribute allows the user to override the scoring process to create 
        //ordered scorable groups, where the scores from the first scorable group are 
        //compared first, and if there is no scorable that wishes to participate from 
        //the first scorable group, then the second scorable group is considered, and so forth.
        //This method will allow us to capture the activity type
        [MethodBind]//Means this method will participate in the scoring process
        [ScorableGroup(0)]//sets the scoreable groups
        public async Task ActivityHandler(IDialogContext context, IActivity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    //sending typing and submit for next group of scorables. 
                    var reply = context.MakeMessage();
                    reply.Type = ActivityTypes.Typing;
                    await context.PostAsync(reply);
                    // continue to next scorable group Without this, it would end with ScorableGroup 0
                    ContinueWithNextGroup();
                    break;

                case ActivityTypes.ContactRelationUpdate:
                case ActivityTypes.Typing:
                case ActivityTypes.DeleteUserData:
                case ActivityTypes.Ping:
                default:
                    await context.PostAsync($"Unknown activity type ignored: {activity.Type}");
                    break;
            }
        }

        // conversation update handler.
        // Since both of these are in scorable group 0 .. for conversation update this one will fire
        // as opposed to the activity handler, we dont want to send this to the other scorables
        // with ContinueWithNextGroup because its not a message from user
        // This handler allows us to capture when a user is added so we can greet them
        [MethodBind]
        [ScorableGroup(0)]
        public async Task ConversationUpdateHandler(IDialogContext context, IConversationUpdateActivity update)
        {
            var reply = context.MakeMessage();
            if (update.MembersAdded.Any())
            {
                var newMembers = update.MembersAdded?.Where(t => t.Id != update.Recipient.Id);
                foreach (var newMember in newMembers)
                {
                    reply.Text = "Welcome to the Library Bot.  I can help you find a libray, suggest a book, or look for events. ";
                    if (!string.IsNullOrEmpty(newMember.Name))
                    {
                        reply.Text += $" {newMember.Name}";
                    }
                    reply.Text += "!";
                    await context.PostAsync(reply);
                }
            }
        }


        // When you say reset it will trigger the prompt dialog to confirm reset of counter.
        [RegexPattern("^reset")]
        [ScorableGroup(1)]
        public async Task Reset(IDialogContext context, IActivity activity)
        {
            //Show using a confirm dialog
            PromptDialog.Confirm(context, AfterResetAsync,
            "Are you sure you want to reset the count?",
            "Didn't get that!");
            ContinueWithNextGroup();// not sure about this here or in the method below?
        }

        // This is not participating in the scorable
        private async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            //of course we can call out to another dialog here
            var confirm = await argument;
            if (confirm)
            {
                context.UserData.SetValue("count", 1);
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
        }

        //Next we want to wire up our commands.  We dont want to go to LUIS if we dont have to 
        //for simple things
        // When you say "echo: <text to echo>", it will increment the counter and echo the <text to echo>.
        [RegexPattern("^echo: (?<echo>.*)")]
        [ScorableGroup(1)]
        public async Task Echo(IDialogContext context, IActivity activity, [Entity("echo")] string echoString)
        {
            await context.PostAsync($"You said {echoString}");
            ContinueWithNextGroup();
        }
        // Next we want to wire up out LUIS Intents. 
        // Luis model didn't recognize the query and "None" intent was winner.
        [LuisIntent("")]
        [LuisIntent("None")]
        [ScorableGroup(2)]
        public async Task None(IDialogContext context, LuisResult result)
        {
            // Luis returned with None as the winning intent
            await context.PostAsync("You hit the none intent");

        }
       // Here is another Luis intent.. this needs to match exactly what you have in LUIS.
        [LuisIntent("FindLibrary")]
        [ScorableGroup(2)]
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

        }
        // And if nothing matches, we go here.. how do we handle it :)
        // Since none of the scorables in previous group won, the dialog send help message.
        [MethodBind]
        [ScorableGroup(3)]
        public async Task Default(IDialogContext context, IActivity activity)
        {
            await context.PostAsync("I'm sorry. I didn't understand you.");
            await context.PostAsync("You can tell me: \"echo: <some text>\"");
            await context.PostAsync("Or ask me about <Give an example of what LUIS would catch>\"");
        }
    }
}