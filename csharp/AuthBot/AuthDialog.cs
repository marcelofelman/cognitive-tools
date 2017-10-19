using AuthBot;
using AuthBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Graph;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MicrosoftGraphBot.Dialog
{
    [Serializable]
    public class AuthDialog : IDialog<string>
    {
        private static readonly HttpProvider HttpProvider = new HttpProvider(new HttpClientHandler(), false);
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;
            

            var token = await context.GetAccessToken("https://graph.microsoft.com/");
            if (string.IsNullOrEmpty(token))
            {
                await context.Forward(new AzureAuthDialog("https://graph.microsoft.com/"), this.resumeAfterAuth, message, CancellationToken.None);
            }
            else
            {
                var client = GetClient(token);
                await client.Me.Request().UpdateAsync(new User
                {
                    PasswordProfile = new PasswordProfile
                    {
                        Password = message.Text,
                        ForceChangePasswordNextSignIn = false
                    },
                });
                await context.PostAsync($"Password has already changed");
                context.Wait(MessageReceivedAsync);
            }
        }

        private static GraphServiceClient GetClient(string accessToken, IHttpProvider provider = null)
        {
            var delegateAuthProvider = new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.FromResult(0);
            });

            var graphClient = new GraphServiceClient(delegateAuthProvider, provider ?? HttpProvider);

            return graphClient;
        }

        private async Task resumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            await context.PostAsync(message);

            //now that token exists...forward to whatever you want!
            //await context.Forward(new NewDialog(), null, message, CancellationToken.None);
        }
    }
}
