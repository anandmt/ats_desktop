using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV.Face;
using Emgu.CV.Util;
using System.IO;
using System.Drawing;
using ats.client.Model;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using ats.client.Helpers;

namespace ats.client.View
{
    /// <summary>
    /// Interaction logic for FaceDetectView.xaml
    /// </summary>
    public partial class FaceDetectView : UserControl
    {
        //private VideoCapture _capture;
        //private CascadeClassifier _haarCascade;
        //DispatcherTimer _timer;

        public FaceDetectView()
        {
            InitializeComponent();
          //  FaceDetect();
        }

        //private void FaceDetect()
        //{
        //    _capture = new VideoCapture();
        //    _haarCascade = new CascadeClassifier(@"haarcascade_frontalface_alt_tree.xml");
        //    _timer = new DispatcherTimer();
        //    _timer.Tick += (_, __) =>
        //    {
        //        Image<Bgr, Byte> currentFrame = _capture.QueryFrame().ToImage<Bgr, Byte>();
        //        if (currentFrame != null)
        //        {
        //            Image<Gray, Byte> grayFrame = currentFrame.Convert<Gray, Byte>();
        //            var detectedFaces = _haarCascade.DetectMultiScale(grayFrame);
        //            foreach (var face in detectedFaces)
        //            {
        //                currentFrame.Draw(face, new Bgr(0, double.MaxValue, 0), 2, LineType.FourConnected);
        //            }
        //            FaceImage.Source = Helper.ToBitmapSource(currentFrame);
        //        }
        //    };
        //    _timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        //    _timer.Start();
        //}

       

        //private void captureButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Image<Bgr, Byte> imgeOrigenal = _capture.QueryFrame().ToImage<Bgr, Byte>();
        //    imgeOrigenal.Save(@"C:\Users\atiwari\source\test\j.jpg");
        //}
    }
}
