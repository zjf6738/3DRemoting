//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;
using Emgu.Util.TypeEnum;
using ZedGraph;
using LineType = Emgu.CV.CvEnum.LineType;

namespace HelloWorld
{
   class Program
   {
      static int Main(string[] args)
      {
          //��ѡBOOSTING, MIL, KCF, TLD, MEDIANFLOW, or GOTURN

          Rectangle roi = new Rectangle();
          Mat frame = new Mat();
          Mat frame_scale = new Mat();


          //������Ƶ
          string video = "S-dst.avi";   //S-800v1.avi   
          VideoCapture cap = new VideoCapture(video);
          if (!cap.IsOpened)
          {
              Console.WriteLine("cannot read video!");
              return -1;
          }

          frame = cap.QueryFrame();
          CvInvoke.Resize(frame, frame_scale, new Size(480, 320), 0, 0, Inter.Linear);//��С�ߴ�

          //����roi����
          // roi = selectROI("scale tracker", frame_scale);
          roi = new Rectangle(375 - 5, 122 - 5, 21 + 5, 58 + 5);
          if (roi.Width == 0 || roi.Height == 0) return 0;

          // λ�ü����
          EndPointDectector dectector = new EndPointDectector(frame,roi);

          Test1();


          // ������ʾ
          Console.WriteLine("Start the tracking process, press ESC to quit.\n");
          CvInvoke.NamedWindow("scale tracker", NamedWindowType.Normal);
          CvInvoke.NamedWindow("input tracker", NamedWindowType.Normal);
          CvInvoke.NamedWindow("roi_singletemp", NamedWindowType.Normal);
          CvInvoke.NamedWindow("tracker houghline", NamedWindowType.Normal);

          while (true)
          {
              // ��ȡͼ��֡
              frame = cap.QueryFrame();
              PointF crossPoint = dectector.Detect(frame);

              Console.WriteLine("��⵽������㣺" + crossPoint.ToString());

              Display(dectector, frame, crossPoint);

              //ѭ��ʱ���趨
              if (CvInvoke.WaitKey(1) == 27)
              {
                  break;
              }

          }

          cap.Dispose();
          return 0;
      }

       private static void Test1()
       {
           Mat mat = new Mat(new Size(3, 1), DepthType.Cv32S, 1);
           float[] data = new float[] {1f, 2f, 3f};
           Marshal.Copy(data, 0, mat.DataPointer, mat.Width*mat.Height);
           Console.WriteLine(mat.Width.ToString()+mat.Height.ToString());
       }

       // ��ʾ
       private static void Display(EndPointDectector dectector, Mat frame, PointF crossPoint)
       {
           //����roi�����ٿ�
           CvInvoke.Rectangle(dectector.FrameScale, dectector.Roi, new MCvScalar(255, 0, 0), 2, LineType.FourConnected);
               //���ƴ�����ͼ���ROI����
           CvInvoke.Imshow("scale tracker", dectector.FrameScale);

           //����ԭͼ�ĸ��ٿ�
           CvInvoke.Rectangle(dectector.Frame, dectector.RoiScale, new MCvScalar(255, 0, 0), 2); //����ԭͼ��ROI����
           CvInvoke.Imshow("input tracker", frame);

           // sobel
           CvInvoke.Imshow("edge_sobel", dectector.RoiSobel);

           // canny
           CvInvoke.Imshow("Canny", dectector.RoiCanny);

           if (dectector.Lines.Length > 1) //�жϼ���ֱ������������Ҫ���ڵ���2
           {
               CvInvoke.Line(dectector.RoiTemp, dectector.FinalLinestand.P1, dectector.FinalLinestand.P2,
                   new MCvScalar(0, 0, 255), 2, LineType.EightConnected);
               CvInvoke.Line(dectector.RoiTemp, dectector.FinalLineanother.P1, dectector.FinalLineanother.P2,
                   new MCvScalar(0, 0, 255), 2, LineType.EightConnected);
               CvInvoke.Circle(dectector.RoiSingletemp, new Point((int) crossPoint.X, (int) crossPoint.Y), 3,
                   new MCvScalar(255, 100, 0));
           }
           else if (dectector.Lines.Length == 1)
           {
               Console.WriteLine("ֻ�����һ��ֱ�ߣ�");
           }
           else
           {
               Console.WriteLine("��ⲻ��ֱ�ߣ�");
           }

           CvInvoke.Imshow("tracker houghline", dectector.RoiTemp); //��ʾĩ�˵��λ��
           CvInvoke.Imshow("roi_singletemp", dectector.RoiSingletemp); //��ʾĩ�˵��λ��  
       }
   }
}
