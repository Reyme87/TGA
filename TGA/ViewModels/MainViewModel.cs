using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGA.Models;
using TGA.ViewModels.Base;
using TL;

namespace TGA.ViewModels
{
    class MainViewModel : ViewModel
    {
        #region Коллекции элементов

        private ObservableCollection<ChannelModel> _channels;
        private ObservableCollection<MessageData> _messages;

        public ObservableCollection<ChannelModel> Channels
        {
            get => _channels;
            set
            {
                Set(ref _channels, value);
            }
        }
        public ObservableCollection<MessageData> Messages
        {
            get => _messages;
            set
            {
                Set(ref _messages, value);
            }
        }

        private static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "TextClassificationModel.zip");

        private TgParser _parser;
        public TgParser Parser
        {
            get => _parser;
            set
            {
                Set(ref _parser, value);
            }
        }

        private ChannelModel _selectedChannel;
        public ChannelModel SelectedChannel
        {
            get => _selectedChannel;
            set
            {
                Set(ref _selectedChannel, value);
                if(_selectedChannel != null)
                {
                    CollectMessages();
                }
            }
        }

        #endregion

        public MainViewModel()
        {
            Parser = new TgParser(Config);
            CollectChats();

            //Messages = new ObservableCollection<MessageData>();
            //// Заполните данными для теста
            //Messages.Add(new MessageData { Category = "Тест", Content = "Пример сообщения", Views = 100, TotalReactionsCount = 10 });
        }

        private async void CollectChats()
        {
            Channels = await Parser.GetChatsList();
        }

        private async void CollectMessages()
        {
            Messages = await Parser.ParseMessages(SelectedChannel.Id, 100);
        }

        private string Config(string what)
        {
            switch (what)
            {
                case "api_id": return Environment.GetEnvironmentVariable("api_id");
                case "api_hash": return Environment.GetEnvironmentVariable("api_hash"); ;
                case "phone_number": return Environment.GetEnvironmentVariable("phone_number");
                case "verification_code":
                    return Interaction.InputBox("Verification code:");
                case "password":
                    return Interaction.InputBox("Enter 2FA password");
                case "session_pathname": return "session.dat";
                default: return null;
            }
        }
    }
}
