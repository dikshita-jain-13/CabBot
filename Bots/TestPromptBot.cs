using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace TestEchoBot.Bots
{
    public class TestPromptBot : ActivityHandler
    {
        private readonly BotState _userState;
        private readonly BotState _conversationState;

        public TestPromptBot(ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
            _userState = userState;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            IStatePropertyAccessor<QuestionsFlow> conversationStateAccessors = _conversationState.CreateProperty<QuestionsFlow>(nameof(QuestionsFlow));
            QuestionsFlow flow = await conversationStateAccessors.GetAsync(turnContext, () => new QuestionsFlow());

            IStatePropertyAccessor<Profile> userStateAccessors = _userState.CreateProperty<Profile>(nameof(Profile));
            Profile profile = await userStateAccessors.GetAsync(turnContext, () => new Profile());

            await BindProfileQuestionsAsync(flow, profile, turnContext);

            // Save State changes.
            await _conversationState.SaveChangesAsync(turnContext);
            await _userState.SaveChangesAsync(turnContext);
        }

        private static async Task BindProfileQuestionsAsync(QuestionsFlow flow, Profile profile, ITurnContext turnContext)
        {
            string input = turnContext.Activity.Text?.Trim();
            string message;

            switch (flow.LastQuestionAsked)
            {
                case QuestionsFlow.Question.None:
                    await turnContext.SendActivityAsync("Let's get started. What is your name?");
                    flow.LastQuestionAsked = QuestionsFlow.Question.Name;
                    break;
                case QuestionsFlow.Question.Name:
                    if (RecognizeHelper.CheckName(input, out string name, out message))
                    {
                        profile.Name = name;
                        await turnContext.SendActivityAsync($"Hi {profile.Name}.");
                        await turnContext.SendActivityAsync("How old are you?");
                        flow.LastQuestionAsked = QuestionsFlow.Question.Age;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case QuestionsFlow.Question.Age:
                    if (RecognizeHelper.CheckAge(input, out int age, out message))
                    {
                        profile.Age = age;
                        await turnContext.SendActivityAsync($"I have your age as {profile.Age}.");
                        await turnContext.SendActivityAsync("When do you want schedule cab?");
                        flow.LastQuestionAsked = QuestionsFlow.Question.Date;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }

                case QuestionsFlow.Question.Date:
                    if (RecognizeHelper.CheckDate(input, out string date, out message))
                    {
                        profile.Date = date;
                        await turnContext.SendActivityAsync($"Your cab ride is scheduled for {profile.Date}.");
                        await turnContext.SendActivityAsync($"Thanks for booking cab {profile.Name}.");
                        await turnContext.SendActivityAsync($"Bot works great way. You can book cab anytime.");
                        flow.LastQuestionAsked = QuestionsFlow.Question.None;
                        profile = new Profile();
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
            }
        }
    }
}
