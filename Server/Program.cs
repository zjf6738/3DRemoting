//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Qzeim.ThrdPrint.BroadCast.Server;

namespace CameraCapture
{
   static class Program
   {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main()
      {
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);

         //Form frm = new CameraCapture();
         //Form frm = new VideoRecord();
         //Form frm = new ServerForm2();
         Form frm = new ServerForm();

         Application.Run(frm);
      }
   }
}