using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ats.client.Helpers;
using ats.client.Model;
using ats.client.Properties;
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
        private BitmapSource _bitmapSource;
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
            FaceData = faceDataModel;
            OpenFileCommand = new DelegateCommand(OpenFileCommandExecute);
            RegisterCommand = new DelegateCommand(RegisterCommandExecute);
            CreateFolder();
            FaceData.FaceDataModels = new ObservableCollection<FaceDataModel>();
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
            CreateFolder("Anand");

            string appFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string path = Path.Combine(
                                         Directory.GetParent(appFolderPath).Parent.FullName, "Resources");

            FaceData.FaceDataModels.Add(new FaceDataModel
            {
                Id = 0,
                Name = "Anand Tiwari",
                EntryTime = DateTime.Now.ToString(),
                ExitTime = string.Empty,
                Status = StatusEnum.Enter.ToString(),
                SelectedFileName = "anand_lnkdn.jpg",
                SelectedFile= $"{path}\\anand_lnkdn.jpg",
                ImageSource = Helper.BitmapToBitmapSource(Resources.anand_lnkdn)

            });
            SaveImageToFolder(FaceData.FaceDataModels[0].Name,FaceData.FaceDataModels[0].SelectedFileName);
            FaceData.FaceDataModels.Add(new FaceDataModel
            {
                Id = 1,
                Name = "Arti Tiwari",
                EntryTime = DateTime.Now.ToString(),
                ExitTime = DateTime.Now.ToString(),
                Status = StatusEnum.Exit.ToString(),
                ImageSource = Helper.BitmapToBitmapSource(Resources.t1),
                SelectedFileName="t1.jpg",
                SelectedFile=$"{path}\\t1.jpg"

            });
            SaveImageToFolder(FaceData.FaceDataModels[1].Name, FaceData.FaceDataModels[1].SelectedFileName);

        }

        private void RegisterCommandExecute(object obj)
        {
            CreateFolder(FaceData.Name);
            var imagePath = SaveImageToFolder(FaceData.Name,FaceData.SelectedFileName);
            AddPersonToList(imagePath);
        }

        private void AddPersonToList(string imagePath)
        {
            Bitmap bitmap = (Bitmap)Bitmap.FromFile(imagePath, true);
            _bitmapSource = Helper.BitmapToBitmapSource(bitmap);

            _faceDataModel.FaceDataModels.Add(new FaceDataModel
            {
                Name = FaceData.Name,
                EntryTime = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Status = StatusEnum.Enter.ToString(),
                ImageSource = _bitmapSource

            });
        }

        private string SaveImageToFolder(string name, string selectedFileName)
        {
              var  path = $"C:\\ATS\\{name}\\{selectedFileName}";

            if (!File.Exists(path))
            {
                File.Copy(FaceData.SelectedFile, path);
                return path;
            }
            else
            {
                FaceData.Error = $"File {FaceData.SelectedFile} already exists.";
                return "";
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
