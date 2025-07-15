using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using TGA.Commands;
using TGA.Models;
using TGA.Properties;
using TGA.ViewModels.Base;

namespace TGA.ViewModels
{
    class MainViewModel : ViewModel
    {
        #region Коллекции элементов

        private readonly static string _outputDir = "TelegramData";
        private Visibility _isVisible;
        private ObservableCollection<ChannelModel> _channels;
        private ObservableCollection<MessageData> _messages;
        private string _phoneNumber = "";
        private string _userName;

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

        public Visibility IsVisible
        {
            get => _isVisible;
            set
            {
                Set(ref _isVisible, value);
            }
        }
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
                if (_selectedChannel != null)
                {
                    CollectMessages();
                }
            }
        }
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                Set(ref _phoneNumber, value);
            }
        }
        public string UserName
        {
            get => _userName;
            set
            {
                Set(ref _userName, value);
            }
        }

        #endregion

        #region Команды

        #region ExportDataCommand

        public ICommand ExportDataCommand { get; }

        public async void OnExportDataCommandExecuted(object p)
        {
            Directory.CreateDirectory(_outputDir);

            string tempTitle = SelectedChannel.Name;
            string pattern = @"\W";
            string result = Regex.Replace(tempTitle, "[^.\\w.\\s+]", "").Trim();

            var path = Path.Combine(_outputDir, result);
            Directory.CreateDirectory(path);
            var fileName = Path.Combine(path, $"chat_{result}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.json");
            await File.WriteAllTextAsync(fileName, JsonConvert.SerializeObject(Messages.Reverse(), Formatting.Indented));
        }

        public bool CanExportDataCommandExecute(object p)
        {
            if (!Equals(SelectedChannel, null))
            {
                IsVisible = Visibility.Visible;
                return true;
            }
            IsVisible = Visibility.Hidden;
            return false;
        }



        #endregion

        #region ExitCommand

        public ICommand ExitCommand { get; }

        public void OnExitCommandExecuted(object p)
        {
            LoadInfoAsync("", "phone.json");
            Parser.Logout();
            Application.Current.Shutdown();
        }

        public bool CanExitCommandExecute(object p) => !Equals(Parser, null);

        #endregion

        #endregion

        public MainViewModel()
        {
            #region Команды

            ExportDataCommand = new RelayCommand(OnExportDataCommandExecuted, CanExportDataCommandExecute);

            ExitCommand = new RelayCommand(OnExitCommandExecuted, CanExitCommandExecute);

            #endregion

            Parser = new TgParser(Config);
            GetUsername();
            CollectChats();
        }

        private async void CollectChats()
        {
            Channels = await Parser.GetChatsList();
        }

        private async void CollectMessages()
        {
            Messages = await Parser.ParseMessages(SelectedChannel.Id, 100);
        }

        private async void GetUsername()
        {
            UserName = await Parser.GetUserName();
        }

        private string Config(string what)
        {
            switch (what)
            {
                case "api_id": return Settings.Default.api_id;
                case "api_hash": return Settings.Default.api_hash;
                case "phone_number":
                    if (GetInfo("phone.json") == null || GetInfo("phone.json") == "")
                    {
                        _phoneNumber = Interaction.InputBox("Введите номер телефона с +:");
                        LoadInfoAsync(_phoneNumber, "phone.json");
                        return _phoneNumber;
                    }
                    else
                    {
                        return GetInfo("phone.json");
                    }
                case "verification_code":
                    return Interaction.InputBox("Введите код верификации:");
                case "password":
                    return Interaction.InputBox("Введите 2FA пароль");
                case "session_pathname": return "session.dat";
                default: return null;
            }
        }

        static public async void LoadInfoAsync(string value, string fileName)
        {
            string json = JsonConvert.SerializeObject(value, Formatting.Indented);
            await File.WriteAllTextAsync(fileName, json);
        }

        static public string GetInfo(string fileName)
        {
            string? number = null;
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                FileInfo fileInfo = new FileInfo(fileName);
                if (fileInfo.Length != 0)
                {
                    try
                    {
                        number = System.Text.Json.JsonSerializer.Deserialize<string>(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Error occured while reading the data!");
                    }
                }
            }
            return number;
        }
    }
}
