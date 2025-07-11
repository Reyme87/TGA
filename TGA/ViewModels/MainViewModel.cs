using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGA.Models;
using TGA.ViewModels.Base;

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

        private Classifier _classifier;
        public Classifier Classifier
        {
            get => _classifier;
            set
            {
                Set(ref _classifier, value);
            }
        }
        
        #endregion

        public MainViewModel() 
        {
            Classifier = new Classifier();
            Classifier.LoadModel(_modelPath);
        }
    }
}
