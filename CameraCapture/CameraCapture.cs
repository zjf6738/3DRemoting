//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;

namespace CameraCapture
{
   public partial class CameraCapture : Form
   {
      private VideoCapture _capture0 = null;
      private VideoCapture _capture1 = null;

      private bool _captureInProgress;
      private Mat _frame0;
      private Mat _frame1;
      private Mat _grayFrame;
      private Mat _smallGrayFrame;
      private Mat _smoothedGrayFrame;
      private Mat _cannyFrame;

      public CameraCapture()
      {
         InitializeComponent();
         CvInvoke.UseOpenCL = false;
         try
         {
            _capture0 = new VideoCapture(0);
            _capture0.ImageGrabbed += ProcessFrame;

            _capture1 = new VideoCapture(1);
            _capture1.ImageGrabbed += ProcessFrame1;


         }
         catch (NullReferenceException excpt)
         {
            MessageBox.Show(excpt.Message);
         }
         _frame0 = new Mat();
         _frame1 = new Mat();
         _grayFrame = new Mat();
         _smallGrayFrame = new Mat();
         _smoothedGrayFrame = new Mat();
         _cannyFrame = new Mat();
      }

      private void ProcessFrame(object sender, EventArgs arg)
      {
         if (_capture0 != null && _capture0.Ptr != IntPtr.Zero)
         {
            _capture0.Retrieve(_frame0, 0);

            CvInvoke.CvtColor(_frame0, _grayFrame, ColorConversion.Bgr2Gray);

            CvInvoke.PyrDown(_grayFrame, _smallGrayFrame);

            CvInvoke.PyrUp(_smallGrayFrame, _smoothedGrayFrame);

            CvInvoke.Canny(_smoothedGrayFrame, _cannyFrame, 100, 60);

            captureImageBox.Image = _frame0;
            //grayscaleImageBox.Image = _grayFrame;
            smoothedGrayscaleImageBox.Image = _smoothedGrayFrame;
            cannyImageBox.Image = _cannyFrame;
         }
      }

      private void ProcessFrame1(object sender, EventArgs arg)
      {
          if (_capture1 != null && _capture1.Ptr != IntPtr.Zero)
          {
              _capture1.Retrieve(_frame1, 0);

              grayscaleImageBox.Image = _frame1;
          }
      }

      private void captureButtonClick(object sender, EventArgs e)
      {
         if (_capture0 != null && _capture1 != null)
         {
            if (_captureInProgress)
            {  //stop the capture
               captureButton.Text = "Start Capture";
               _capture0.Pause();
               _capture1.Pause();
            }
            else
            {
               //start the capture
               captureButton.Text = "Stop";
               _capture0.Start();
               _capture1.Start();
            }

            _captureInProgress = !_captureInProgress;
         }
      }

      private void ReleaseData()
      {
         if (_capture0 != null)
            _capture0.Dispose();

         if (_capture1 != null)
             _capture1.Dispose();
      }

      private void FlipHorizontalButtonClick(object sender, EventArgs e)
      {
         if (_capture0 != null) _capture0.FlipHorizontal = !_capture0.FlipHorizontal;
      }

      private void FlipVerticalButtonClick(object sender, EventArgs e)
      {
         if (_capture0 != null) _capture0.FlipVertical = !_capture0.FlipVertical;
      }
   }
}
