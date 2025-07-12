using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TL;
using TL.Methods;
using WTelegram;

namespace TGA.Models
{
    internal class TgParser
    {
        private static Client _client;
        //private readonly static string _outputDir = "TelegramData";
        private static User _user;
        private static Dictionary<long, ChatBase> _chats = new Dictionary<long, ChatBase>();
        private static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "TextClassificationModel.zip");

        private static long _selectedID;

        //public static Dictionary<long, ChatBase> Chats { get; set; } = new Dictionary<long, ChatBase>();
        public Func<string, string> ConfigFunc { get; set; }

        private static Classifier _classifier;

        public TgParser(Func<string, string> configFunc = null)
        {
            if (configFunc == null)
            {
                ConfigFunc = Config;
            }
            else
            {
                ConfigFunc = configFunc;
            }
            _client = new Client(ConfigFunc);
            _classifier = new Classifier();
            _classifier.LoadModel(_modelPath);
        }

        public static async Task InitializeParser()
        {
            _user = await _client.LoginUserIfNeeded();
        }

        public static async Task<List<ChannelModel>> GetChatsList()
        {
            await LoadChats();
            var chatList = _chats.Values.ToList();
            var channels = new List<ChannelModel>();
            foreach (var chat in chatList)
            {
                if (chat is Channel channel)
                {
                    channels.Add(new ChannelModel(chat.ID, chat.Title, chat.MainUsername));
                }
            }

            return channels;
        }

        private static async Task LoadChats()
        {
            var dialogs = await _client.Messages_GetAllChats();
            _chats = dialogs.chats;
        }

        //public static async Task<ChatBase> SelectChat()
        //{
        //    var i = 0;
        //    var chatList = Chats.Values.ToList();

        //    foreach (var chat in chatList)
        //    {
        //        if (chat is Channel channel)
        //        {
        //            Console.WriteLine($"{++i}. {chat.GetType().Name} {chat.Title} (ID: {chat.ID})");
        //        }
        //    }

        //    Console.WriteLine("\nВыберите номер канала (или 0 для отмены):");
        //    var selected = int.Parse(Console.ReadLine() ?? "0");
        //    _selectedID = chatList[selected - 1].ID;
        //    //Console.WriteLine("\nВыберите категорию для канала:");
        //    //_category = Console.ReadLine();

        //    return selected > 0 ? chatList[selected - 1] : null;
        //}

        public static async Task<List<MessageData>> ParseMessages(long ID, int limit)
        {
            _chats.TryGetValue(ID, out ChatBase? chat);
            var allMessages = new List<MessageData>();
            var totalCount = 0;
            var offsetId = 0;
            var hasMore = true;

            while (hasMore && (limit == 0 || totalCount < limit))
            {
                var messages = await _client.Messages_GetHistory(
                    chat,
                    offsetId,
                    min_id: 0,
                    max_id: 0,
                    limit: Math.Min(100, limit == 0 ? 100 : limit - totalCount));

                if (messages.Messages.Length == 0)
                {
                    hasMore = false;
                    break;
                }

                foreach (var msg in messages.Messages)
                {
                    var messageData = await ProcessMessage(msg);
                    if (messageData != null)
                    {
                        if (msg is Message message && HasTextContent(message))
                        {
                            allMessages.Add(messageData);
                            totalCount++;
                        }
                    }

                    offsetId = msg.ID;
                }

                await Task.Delay(200);
            }

            return allMessages;
        }

        private static async Task<MessageData> ProcessMessage(MessageBase msgBase)
        {
            try
            {
                if (!(msgBase is Message msg)) return null;

                var (reactionsList, totalReactions) = GetMessageReactions(msg);
                var messageData = new MessageData
                {
                    Id = msg.ID,
                    Date = msg.Date.ToLocalTime(),
                    EditDate = msg.edit_date.ToLocalTime(),
                    Views = msg.views,
                    //Content = GetMessageContent(msg),
                    Content = msg.message,
                    Category = _classifier.PredictCategory(msg.message),
                    Author = await GetMessageAuthor(msg, _selectedID),
                    TotalReactionsCount = totalReactions,
                    Reactions = reactionsList,
                };

                return messageData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static async Task<MessageAuthor> GetMessageAuthor(Message msg, long channelID)
        {
            var channel = _chats[channelID];
            try
            {
                if (msg.from_id is PeerUser userPeer)
                {
                    var users = await _client.Users_GetUsers(new[] { new InputUser(userPeer.user_id, 0) });
                    if (users.FirstOrDefault() is User user)
                    {
                        return new MessageAuthor
                        {
                            Id = user.id,
                            Type = "User",
                            Name = $"{user.first_name} {user.last_name}".Trim(),
                            Username = user.username,
                        };
                    }
                }

                return new MessageAuthor
                {
                    Id = channel.ID,
                    Type = "Channel",
                    Name = channel.Title,
                    Username = (channel as Channel)?.username
                };
            }
            catch
            {
                return null;
            }
        }

        private static bool HasTextContent(MessageBase msg)
        {
            if (!(msg is Message message)) return false;

            // 1. Проверяем основной текст сообщения
            if (!string.IsNullOrWhiteSpace(message.message))
                return true;

            // 2. Проверяем медиа-контент с текстовыми атрибутами
            switch (message.media)
            {
                case MessageMediaPoll poll:
                    return !string.IsNullOrWhiteSpace(poll.poll.question.ToString());

                case MessageMediaInvoice invoice:
                    return !string.IsNullOrWhiteSpace(invoice.title) ||
                           !string.IsNullOrWhiteSpace(invoice.description);

                case MessageMediaPhoto:
                case MessageMediaDocument:
                    // Для фото и документов проверяем атрибуты
                    return message.entities?.Length > 0; // Если есть текстовые entities
            }

            return false;
        }

        private static (List<Reaction>, int) GetMessageReactions(Message msg)
        {
            var reactions = new List<Reaction>();
            int totalCount = 0;

            if (msg.reactions?.results != null)
            {
                foreach (var r in msg.reactions.results)
                {
                    reactions.Add(new Reaction
                    {
                        Emoji = r.reaction,
                        Count = r.count,
                    });
                    totalCount += r.count;
                }
            }

            return (reactions, totalCount);
        }

        private static string Config(string what)
        {
            switch (what)
            {
                case "api_id": return Environment.GetEnvironmentVariable("api_id");
                case "api_hash": return Environment.GetEnvironmentVariable("api_hash"); ;
                case "phone_number": return Environment.GetEnvironmentVariable("phone_number");
                case "verification_code":
                    Console.WriteLine("Введите код подтверждения:");
                    return Console.ReadLine();
                case "password":
                    Console.WriteLine("Введите пароль (2FA):");
                    return Console.ReadLine();
                case "session_pathname": return "session.dat";
                default: return null;
            }
        }
    }
}
