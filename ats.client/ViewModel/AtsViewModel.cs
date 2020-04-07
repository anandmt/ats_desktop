using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ats.client.Helpers;
using ats.client.Model;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Win32;

namespace ats.client.ViewModel
{
    public class AtsViewModel : ObservableObject
    {
        private const string subscriptionKey = "d4c8b7435f7643aaba40f3719f0f8bc3";
        private const string faceEndpoint = "https://smartface.cognitiveservices.azure.com/";
        private readonly IFaceClient faceClient = new FaceClient(
           new ApiKeyServiceClientCredentials(subscriptionKey),
           new System.Net.Http.DelegatingHandler[] { });
        string personGroupId = "atsRegisteredGroup";
        private FaceDataModel _faceDataModel;
        private static BitmapSource _bitmapSource;
        #region Commands
        public ICommand OpenFileCommand { get; }
        public ICommand RegisterCommand { get; }

        #endregion Commands

        #region Properties
        public FaceDataModel FaceData
        {
            get => _faceDataModel;
            set
            {
                if (_faceDataModel != value)
                {
                    _faceDataModel = value;
                    OnPropertyChanged(nameof(FaceData));
                }
            }
        }
        #endregion



        public AtsViewModel(FaceDataModel faceDataModel)
        {
            _faceDataModel = faceDataModel;
            OpenFileCommand = new DelegateCommand(OpenFileCommandExecute);
            RegisterCommand = new DelegateCommand(RegisterCommandExecute);
            CreateFolder();
            _faceDataModel.FaceDataModels = new List<FaceDataModel>();
            SeedData();
            //if (Uri.IsWellFormedUriString(faceEndpoint, UriKind.Absolute))
            //{
            //    faceClient.Endpoint = faceEndpoint;
            //}
            //else
            //{
            //    //Invalid URI
            //    Environment.Exit(0);
            //}

            //  CreatePersonAsync();
            //CreateUserGroupAsync();
        }

        private void SeedData()
        {
            Bitmap bitmap = (Bitmap)Bitmap.FromFile(@"C:\ATS\Anand Tiwari\test.jpg", true);
            _bitmapSource = Helper.BitmapToBitmapSource(bitmap);

            CreateFolder("Anand");
            _faceDataModel.FaceDataModels.Add(new FaceDataModel
            {
                Name = "Anand",
                EntryTime = DateTime.Now.ToString(),
                ExitTime = DateTime.Now.ToString(),
                Status = StatusEnum.Enter.ToString(),
                ImageSource = _bitmapSource

            });
        }

        private void RegisterCommandExecute(object obj)
        {
            CreateFolder(FaceData.Name);
            SaveImageToFolder();
            AddPersonToList();
        }

        private void AddPersonToList()
        {
            _faceDataModel.FaceDataModels.Add(new FaceDataModel
            {
                Name = FaceData.Name,
                EntryTime = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Status = StatusEnum.Enter.ToString()
            });
        }

        private void SaveImageToFolder()
        {
            var path = $"C:\\ATS\\{FaceData.Name}\\{FaceData.SelectedFileName}";

            if (!File.Exists(path))
            {
                File.Copy(FaceData.SelectedFile, path);
            }
            else
            {
                FaceData.Error = $"File {FaceData.SelectedFile} already exists.";
                return;
            }
        }

        private void CreateFolder(string folderName = "ATS")
        {
            string path = string.Empty;
            if (folderName != "ATS")
                path = Path.Combine(@"C:\ATS\", folderName);
            else
                path = Path.Combine(@"C:\", folderName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void OpenFileCommandExecute(object obj)
        {
            var path = Path.GetDirectoryName(FaceData.SelectedFile);
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = path ?? Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources"),
                RestoreDirectory = true,
                FilterIndex = 1,
                ReadOnlyChecked = true,
                ShowReadOnly = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "txt",
                Filter = "image files (*.jpg)|*.jpg|All files (*.*)|*.*",
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FaceData.SelectedFile = openFileDialog.FileName;
                FaceData.SelectedFileName = openFileDialog.SafeFileName;
            }
        }

        private async void CreateUserGroupAsync()
        {
            // Create an empty PersonGroup

            await faceClient.PersonGroup.DeleteAsync(personGroupId);

            await faceClient.PersonGroup.CreateAsync(personGroupId, "My Friends");

            // Define Anna
            Person friend1 = await faceClient.PersonGroupPerson.CreateAsync(
                // Id of the PersonGroup that the person belonged to
                personGroupId,
                // Name of the person
                "Anna"
            );
        }

        private async void CreatePersonAsync()
        {
            //temp
            await faceClient.PersonGroup.DeleteAsync(personGroupId);

            await faceClient.PersonGroup.CreateAsync(personGroupId, "My Friends");

            // Define Anna
            Person friend1 = await faceClient.PersonGroupPerson.CreateAsync(
                // Id of the PersonGroup that the person belonged to
                personGroupId,
                // Name of the person
                "Anna"
            );

            //temp

            // Define Bill and Clare in the same way

            // Directory contains image files of Anna
            const string friend1ImageDir = @"C:\Users\atiwari\Pictures\Camera Roll\Anand\";

            foreach (string imagePath in Directory.GetFiles(friend1ImageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    // Detect faces in the image and add to Anna
                    await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                        personGroupId, friend1.PersonId, s);
                }
            }
            // Do the same for Bill and Clare

            await faceClient.PersonGroup.TrainAsync(personGroupId);

            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId);

                if (trainingStatus.Status != TrainingStatusType.Running)
                {
                    break;
                }

                await Task.Delay(1000);
            }


            string testImageFile = @"C:\Users\atiwari\Pictures\Camera Roll\test\anand.jpg";

            using (Stream s = File.OpenRead(testImageFile))
            {
                var faces = await faceClient.Face.DetectWithStreamAsync(s);
                var faceIds = faces.Select(face => face.FaceId.Value).ToArray();

                var results = await faceClient.Face.IdentifyAsync(faceIds, personGroupId);
                foreach (var identifyResult in results)
                {
                    Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                    if (identifyResult.Candidates.Count == 0)
                    {
                        Console.WriteLine("No one identified");
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId);
                        Console.WriteLine("Identified as {0}", person.Name);
                    }
                }
            }
        }
    }
}
