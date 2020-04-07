using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
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
        private string entryTime;
        private string exitTime;
        private string status;
        private int id;
        private string error;
        private ImageSource imageSource;
        private List<FaceDataModel> faceDataModels;

        public List<FaceDataModel> FaceDataModels { get => faceDataModels; set { faceDataModels = value; OnPropertyChanged(nameof(FaceDataModels)); } }
        public ImageSource ImageSource
        {
            get => imageSource;
            set { imageSource = value; OnPropertyChanged(nameof(ImageSource)); }
        }

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
        public string Error { get => error; set { if (error != value) { error = value; OnPropertyChanged(nameof(Error)); } } }

    }


    public enum StatusEnum
    {
        Exit,
        Enter
    }

}
