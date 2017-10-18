using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

public static class QnAMakerRecognizer
{
    //TODO: Define strategy, first match or highest index

    private class QnAMakerApp
    {
        public string KnowledgeBaseId { get; set; }

        public string SubscriptionKey { get; set; }
    }

    /// <summary>
    /// Iterate through all registered QnA Maker apps until reaching a high confidence index
    /// </summary>
    /// <param name="utterance"></param>
    /// <returns></returns>
    public static QnAMakerResult Recognize(string utterance, string userCulture)
    {
        IList<QnAMakerApp> _apps = new List<QnAMakerApp>();

        int count = System.Configuration.ConfigurationManager.AppSettings.AllKeys
            .Where(x => x.StartsWith("QnAMakerAppId") && x.EndsWith(userCulture))
            .Count();

        //Instantiate a QnAMakerApp for each id and key pair for this user culture
        for (int i = 1; i <= count; i++)
        {
            string id = System.Configuration.ConfigurationManager.AppSettings.GetValues($"QnAMakerAppId_{i}_{userCulture}").First();
            string key = System.Configuration.ConfigurationManager.AppSettings.GetValues($"QnAMakerAppKey_{i}_{userCulture}").First();

            _apps.Add(new QnAMakerApp() { KnowledgeBaseId = id, SubscriptionKey = key });
        }

        //Iterate through each app until finding a high cofidence response
        foreach (QnAMakerApp app in _apps)
        {
            QnAMakerResult result = Recognize(app, utterance);

            // Get pre-defined confidence threshold
            float.TryParse(System.Configuration.ConfigurationManager.AppSettings.GetValues("QnaMakerConfidenceThreshold").First(), out float confidenceThreshold);

            if (result.Score >= confidenceThreshold)
            {
                return result;
            }
        }

        // No result was better than the pre-defined confidence threshould, ignore this QnA App
        return null;
    }

    private static QnAMakerResult Recognize(QnAMakerApp app, string utterance)
    {
        var responseString = String.Empty;

        QnAMakerResult QnAresponse = null;

        // Send question to API QnA bot
        if (utterance.Length > 0)
        {
            var knowledgebaseId = app.KnowledgeBaseId; 
            var qnamakerSubscriptionKey = app.SubscriptionKey; 

            //Build the URI
            Uri qnamakerUriBase = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v1.0");
            var builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowledgebaseId}/generateAnswer");

            //Add the question as part of the body
            var postBody = $"{{\"question\": \"{utterance}\"}}";

            //Send the POST request
            using (WebClient client = new WebClient())
            {
                //Set headers and encoding
                client.Encoding = System.Text.Encoding.UTF8;
                client.Headers.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
                client.Headers.Add("Content-Type", "application/json");
                responseString = client.UploadString(builder.Uri, postBody);
            }

            try
            {
                QnAresponse = JsonConvert.DeserializeObject<QnAMakerResult>(responseString);
            }
            catch
            {
                throw new Exception("Unable to deserialize QnA Maker response string.");
            }
        }

        return QnAresponse;
    }
}