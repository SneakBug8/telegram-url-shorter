using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;


namespace url_shortener
{
    public class Bot
    {
        public TelegramBotClient Client;
        public Bot(string id)
        {
            Initialize(id);
        }

        async Task Initialize(string id)
        {
            Client = new TelegramBotClient(id);
            var me = await Client.GetMeAsync();
            Console.Title = me.Username;
            System.Console.WriteLine($"Hello! My name is {me.FirstName}");

            Client.OnMessage += OnMessage;

            Client.StartReceiving();
        }

        void OnMessage(object sender, MessageEventArgs args)
        {
            if (args.Message.Text == "/start")
            {
                Client.SendTextMessageAsync(args.Message.Chat.Id, "Hi. I'm your handy shortening bot." +
                "Just send me Url and I'll shorten it for you");
                return;
            }

            Console.WriteLine(string.Format("Recieved message '{0}'.", args.Message.Text));
            var message = Client.SendTextMessageAsync(args.Message.Chat.Id, "Processing your link...").Result;
            var json = GetShortenedUrl(args.Message.Text);
            var response = JsonConvert.DeserializeObject<Response>(json);
            Client.DeleteMessageAsync(args.Message.Chat.Id, message.MessageId);

            if (response.status == "1")
            {
                Client.SendTextMessageAsync(args.Message.Chat.Id, "Your link is: " + response.url);
            }
            else
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(json);
                Client.SendTextMessageAsync(args.Message.Chat.Id, "Error: " + errorResponse.msg);
            }
        }

        class Response
        {
            public string status;
            public string url;
        }

        class ErrorResponse
        {
            public string status;
            public string msg;
        }

        string GetShortenedUrl(string url)
        {
            string site = string.Format("https://bit.ws/shorten?url={0}", url);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(site);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            using (StreamReader stream = new StreamReader(
                 resp.GetResponseStream(), Encoding.UTF8))
            {
                return stream.ReadToEnd();
            }
        }
    }
}