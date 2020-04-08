using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ats.client.Model
{
    public class FaceDataModel : ObservableObject
    {
        private string selectedFile;
        private string selectedFileName;
        private string name;
        private string message;
        private string entryTime;
        private string exitTime;
        private string status;
        private int id;
        private ImageSource imageSource;
        private ObservableCollection<FaceDataModel> faceDataModels;
        private BitmapSource videoStream;
        private string identifiedPerson;
        private bool isEnable;
        private bool isVisible;

        public bool IsEnable { get => isEnable; set { 
                
                isEnable = value;
                if (value)
                {
                    IsVisible = false;
                }
                else
                {
                    IsVisible = true;
                }
                OnPropertyChanged(nameof(IsEnable)); } }
        public bool IsVisible { get => isVisible; set { isVisible = value; OnPropertyChanged(nameof(IsVisible)); } }
        

        public ObservableCollection<FaceDataModel> FaceDataModels { get => faceDataModels; set { faceDataModels = value; OnPropertyChanged(nameof(FaceDataModels)); } }
        public ImageSource ImageSource
        {
            get => imageSource;
            set { imageSource = value; OnPropertyChanged(nameof(ImageSource)); }
        }

        public BitmapSource VideoStream { get => videoStream; set { videoStream = value; OnPropertyChanged(nameof(VideoStream)); } }

        public string SelectedFile
        {
            get => selectedFile;

            set
            {
                if (selectedFile != value)
                {
                    selectedFile = value;
                    OnPropertyChanged(nameof(SelectedFile));
                }
            }
        }
        public string SelectedFileName
        {
            get => selectedFileName;

            set
            {
                if (selectedFileName != value)
                {
                    selectedFileName = value;
                    OnPropertyChanged(nameof(SelectedFileName));
                }
            }
        }
        public int Id { get => id; set => id = value; }
        public string Name { get => name; set { if (name != value) { name = value; OnPropertyChanged(nameof(Name)); } } }
        public string EntryTime { get => entryTime; set { if (entryTime != value) { entryTime = value; OnPropertyChanged(nameof(EntryTime)); } } }
        public string ExitTime { get => exitTime; set { if (exitTime != value) { exitTime = value; OnPropertyChanged(nameof(ExitTime)); } } }
        public string Status { get => status; set { if (status != value) { status = value; OnPropertyChanged(nameof(Status)); } } }
        public string Message { get => message; set { if (message != value) { message = value; OnPropertyChanged(nameof(Message)); } } }
        public string IdentifiedPerson { get => identifiedPerson; set { identifiedPerson = value; OnPropertyChanged(nameof(IdentifiedPerson)); } }
    }


    public enum StatusEnum
    {
        Exit,
        Enter
    }

}
