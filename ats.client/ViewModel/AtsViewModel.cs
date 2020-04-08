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
using System.Windows.Threading;
using ats.client.Helpers;
using ats.client.Model;
using ats.client.Properties;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Win32;
using System.Drawing.Imaging;
using System.Configuration;
namespace ats.client.ViewModel
{
    public class AtsViewModel : ObservableObject
    {

        private static readonly string subscriptionKey = ConfigurationManager.AppSettings["subscriptionKey"];
        private static readonly string faceEndpoint = ConfigurationManager.AppSettings["faceEndpoint"];
        private readonly IFaceClient faceClient = new FaceClient(
           new ApiKeyServiceClientCredentials(subscriptionKey),
           new System.Net.Http.DelegatingHandler[] { });
        readonly string personGroupId = "atsregistered";
        private FaceDataModel _faceDataModel;
        private BitmapSource _bitmapSource;
        private VideoCapture _capture;
        private CascadeClassifier _haarCascade;
        DispatcherTimer _timer;

        #region Commands
        public ICommand OpenFileCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand MarkAttendanceCommand { get; }

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
            FaceDetect();
            FaceData = faceDataModel;
            OpenFileCommand = new DelegateCommand(OpenFileCommandExecute);
            RegisterCommand = new DelegateCommand(RegisterCommandExecute);
            MarkAttendanceCommand = new DelegateCommand(MarkAttendanceCommandExecute);
            Helper.CreateFolder();
            FaceData.FaceDataModels = new ObservableCollection<FaceDataModel>();
            SeedData();
            if (Uri.IsWellFormedUriString(faceEndpoint, UriKind.Absolute))
            {
                faceClient.Endpoint = faceEndpoint;
            }
            else
            {
                // Environment.Exit(0);
                FaceData.Message = "Invalid URI";
            }
            FaceData.IsEnable = true;
        }



        private void MarkAttendanceCommandExecute(object obj)
        {
            FaceData.IsEnable = false;
            Image<Bgr, Byte> captureCurrentFrame = _capture.QueryFrame().ToImage<Bgr, Byte>();
            var path = Helper.CreateFolder("Temp");
            var imgPath = Path.Combine(path, "TestImage.jpg");
            captureCurrentFrame.Save(imgPath);
            TrainIdentificationModel(imgPath);

        }

        private void FaceDetect()
        {
            _capture = new VideoCapture();
            _haarCascade = new CascadeClassifier(@"haarcascade_frontalface_alt_tree.xml");
            _timer = new DispatcherTimer();
            _timer.Tick += (_, __) =>
            {
                Image<Bgr, Byte> currentFrame = _capture.QueryFrame().ToImage<Bgr, Byte>();
                if (currentFrame != null)
                {
                    Image<Gray, Byte> grayFrame = currentFrame.Convert<Gray, Byte>();
                    var detectedFaces = _haarCascade.DetectMultiScale(grayFrame);
                    foreach (var face in detectedFaces)
                    {
                        currentFrame.Draw(face, new Bgr(0, double.MaxValue, 0), 2, LineType.FourConnected);
                    }

                    FaceData.VideoStream = Helper.ToBitmapSource(currentFrame);
                }
            };
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            _timer.Start();
        }


        private void SeedData()
        {
            Helper.CreateFolder("Anand");

            string appFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string path = Path.Combine(
                                         Directory.GetParent(appFolderPath).Parent.FullName, "Resources");

            FaceData.FaceDataModels.Add(new FaceDataModel
            {
                Id = 0,
                Name = "Anand",
                EntryTime = DateTime.Now.ToString(),
                ExitTime = string.Empty,
                Status = StatusEnum.Enter.ToString(),
                SelectedFileName = "anand_lnkdn.jpg",
                SelectedFile = $"{path}\\anand_lnkdn.jpg",
                ImageSource = Helper.BitmapToBitmapSource(Resources.anand_lnkdn)

            });
            SaveImageToFolder(FaceData.FaceDataModels[0].Name,
                FaceData.FaceDataModels[0].SelectedFileName, FaceData.FaceDataModels[0].SelectedFile);

            //Second User.
            Helper.CreateFolder("Test User");
            FaceData.FaceDataModels.Add(new FaceDataModel
            {
                Id = 1,
                Name = "Test User",
                EntryTime = DateTime.Now.ToString(),
                ExitTime = DateTime.Now.ToString(),
                Status = StatusEnum.Exit.ToString(),
                ImageSource = Helper.BitmapToBitmapSource(Resources.t1),
                SelectedFileName = "t1.jpg",
                SelectedFile = $"{path}\\t1.jpg"

            });
            SaveImageToFolder(FaceData.FaceDataModels[1].Name, FaceData.FaceDataModels[1].SelectedFileName,
                FaceData.FaceDataModels[1].SelectedFile);

        }

        private void RegisterCommandExecute(object obj)
        {
            Helper.CreateFolder(FaceData.Name);
            var imagePath = SaveImageToFolder(FaceData.Name, FaceData.SelectedFileName, FaceData.SelectedFile);
            if (!string.IsNullOrEmpty(imagePath))
                AddPersonToList(imagePath);
            else
            {
                FaceData.Message = "";
            }
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

        private string SaveImageToFolder(string name, string selectedFileName, string selectedFilePath)
        {
            var path = $"C:\\ATS\\{name}\\{selectedFileName}";

            if (!File.Exists(path))
                File.Copy(selectedFilePath, path);
            else
                FaceData.Message = $"File {FaceData.SelectedFile} already exists.";
            return path;
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

        private async void TrainIdentificationModel(string testImagePath)
        {

            try
            {
                await faceClient.PersonGroup.DeleteAsync(personGroupId);
            }
            catch (Exception ex)
            {
                FaceData.Message = ex.Message;
            }
            try
            {
                await faceClient.PersonGroup.CreateAsync(personGroupId, "Ats Registered");

                foreach (var item in FaceData.FaceDataModels)
                {
                    try
                    {
                        // Define user
                        Person person = await faceClient.PersonGroupPerson.CreateAsync(
                        personGroupId,
                        item.Name);

                        foreach (string imagePath in Directory.GetFiles($"C:\\ATS\\{item.Name}\\", "*.jpg"))
                        {
                            using (Stream s = File.OpenRead(imagePath))
                            {
                                // Detect faces in the image and add
                                await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                                    personGroupId, person.PersonId, s);
                            }
                        }

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

                        using (Stream s = File.OpenRead(testImagePath))
                        {
                            var faces = await faceClient.Face.DetectWithStreamAsync(s);
                            var faceIds = faces.Select(face => face.FaceId.Value).ToArray();

                            var results = await faceClient.Face.IdentifyAsync(faceIds, personGroupId);
                            foreach (var identifyResult in results)
                            {
                                FaceData.Message = $"Result of face: {identifyResult.FaceId}";
                                if (identifyResult.Candidates.Count == 0)
                                {
                                    FaceData.Message = "No one identified";
                                }
                                else
                                {
                                    // Get top 1 among all candidates returned
                                    var candidateId = identifyResult.Candidates[0].PersonId;
                                    var identifiedperson = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId);
                                    FaceData.IdentifiedPerson = identifiedperson.Name;
                                    FaceData.Message = $"Welcome {identifiedperson.Name}";

                                    break; //Breaking the loop as person has been identified.
                                }
                            }
                        }

                    }
                    catch (Microsoft.Rest.TransientFaultHandling.HttpRequestWithStatusException httpEx)
                    {
                        FaceData.Message = httpEx.Message;
                    }
                    catch (APIErrorException apiex)
                    {
                        FaceData.Message = apiex.Message;

                    }
                    catch (Exception ex)
                    {
                        FaceData.Message = ex.Message;
                    }
                    if (!string.IsNullOrEmpty(FaceData.IdentifiedPerson))
                        break;


                }

                if (!string.IsNullOrEmpty(FaceData.IdentifiedPerson))
                {
                    foreach (var item in FaceData.FaceDataModels)
                    {
                        if (item.Name.Equals(FaceData.IdentifiedPerson))
                        {
                            if (item.Status == StatusEnum.Enter.ToString())
                            {
                                item.Status = StatusEnum.Exit.ToString();
                            }
                            else
                            {
                                item.Status = StatusEnum.Enter.ToString();
                            }
                            break;
                        }
                    }
                }
                FaceData.IsEnable = true;
                FaceData.IdentifiedPerson = string.Empty; // making empty for other person encounter 


            }
            catch (Exception ex)
            {
                FaceData.Message = ex.Message;
            }
        }

    }
}
