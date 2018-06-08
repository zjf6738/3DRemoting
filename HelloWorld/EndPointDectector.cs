using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Tracking;

namespace HelloWorld
{
    public class EndPointDectector
    {
        // 检测器的原始输入图像帧
        protected Mat frame = null;
        // 原始图像的ROI
        protected Rectangle roi = new Rectangle();
        // 跟踪器
        protected Tracker tracker = null; 

        // 缩放的图像帧
        protected Mat frame_scale = new Mat();
        // 缩放图像的ROI
        Rectangle roi_scale = new Rectangle(375 - 5, 122 - 5, 21 + 5, 58 + 5);

        // sobel
        Mat src_roi = new Mat();
        Mat roi_sobel = new Mat();

        //求取末端点的位置
        Mat roi_canny = new Mat();
        Mat roi_gray = new Mat();
        Mat roi_threshold = new Mat();
        Mat roi_erode = new Mat();

        //复制hougline直线检测的部分
        Mat roi_pro = new Mat();
        //用这个显示检测的末端点和拟合的线
        Mat roi_temp = new Mat();
        //用这个单独显示检测的末端点
        Mat roi_singletemp = new Mat();

        LineSegment2D[] lines = new LineSegment2D[0];
        LineSegment2D final_linestand = new LineSegment2D();
        LineSegment2D final_lineanother = new LineSegment2D(); //最终的hougline的直线首末点

        private Size scaleSize = new Size(480,320);

        public EndPointDectector(Mat _frame, Rectangle _roi)
        {
            frame = _frame;
            roi = _roi;
            tracker = new TrackerKCF();
            tracker.Init(frame, roi);
        }

        public Mat Frame
        {
            get { return frame; }
            set { frame = value; }
        }

        public Mat FrameScale
        {
            get { return frame_scale; }
            set { frame_scale = value; }
        }

        public Rectangle Roi
        {
            get { return roi; }
        }

        public Rectangle RoiScale
        {
            get { return roi_scale; }
        }

        public Mat RoiSobel
        {
            get { return roi_sobel; }
        }

        public Mat RoiCanny
        {
            get { return roi_canny; }
        }

        public Mat RoiTemp
        {
            get { return roi_temp; }
        }

        public Mat RoiSingletemp
        {
            get { return roi_singletemp; }
        }

        public LineSegment2D FinalLinestand
        {
            get { return final_linestand; }
        }

        public LineSegment2D FinalLineanother
        {
            get { return final_lineanother; }
        }

        public LineSegment2D[] Lines
        {
            get { return lines; }
        }


        public PointF Detect(Mat _frame)
        {
            PointF crossPoint = new PointF(-1,-1);

            frame = _frame;
            // 合法性判断
            if (frame == null) return crossPoint;
            if (frame.IsEmpty) return crossPoint;

            // 图像缩放
            CvInvoke.Resize(frame, frame_scale, scaleSize, 0, 0, Inter.Cubic);//缩小尺寸
            //跟踪目标并更新模型
            tracker.Update(frame_scale, out roi);

            if (roi.Width == 0 || roi.Height == 0) return crossPoint;

            //ROI区域的放缩
            float Fheight = frame.Rows;
            float Fwidth = frame.Cols;
            float WidthScale = (Fwidth / scaleSize.Width);//宽度的缩放比例
            float HeightScale = (Fheight / scaleSize.Height);//高度的缩放比例

            //放大roi区域
            roi_scale.X = (int)Math.Round(roi.X * WidthScale);
            roi_scale.Y = (int)Math.Round(roi.Y * HeightScale);
            roi_scale.Width = (int)Math.Round(roi.Width * WidthScale);
            roi_scale.Height = (int)Math.Round(roi.Height * HeightScale);

            // 打印喷头的边缘检测
            src_roi = new Mat(frame, roi_scale);
            roi_sobel = sobelEdgeDetection(ref src_roi);

            roi_pro = new Mat(frame_scale, roi);//复制跟踪包围框部分
            roi_temp = frame_scale.Clone();//用这个显示检测的末端点和拟合的线
            roi_singletemp = frame_scale.Clone();//用这个单独显示检测的末端点

            //// 缩放图的roi区域的颜色空间变换、阈值化操作、形态学操作、canny边缘提取
            CvInvoke.CvtColor(roi_pro, roi_gray, ColorConversion.Bgr2Gray);//转化为灰度图像
            CvInvoke.Threshold(roi_gray, roi_threshold, 30, 255, ThresholdType.Binary);//阈值化操作
            Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2, 1), new Point(-1, -1));//腐蚀操作
            CvInvoke.Erode(roi_threshold, roi_erode, element, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 255, 255));
            CvInvoke.Canny(roi_erode, roi_canny, 10, 200, 3);

            //canny检测后直线的末端点判断
            PointF final_canny_crossPoint = CrossPointFromCanny(roi_canny, roi);

            //hough直线检测
            lines = CvInvoke.HoughLinesP(roi_canny, 1, Math.PI / 360, 20, 7, 10);//hough直线


            if (lines.Length > 1)
            {
                crossPoint = CrossPointFromHough(lines, roi, ref roi_temp, ref final_linestand, ref final_lineanother, final_canny_crossPoint);
            }

            return crossPoint;
        }


        #region 工具函数
        private static PointF CrossPointFromHough(LineSegment2D[] lines,
            Rectangle roi,
            ref Mat roi_temp,
            ref LineSegment2D final_linestand,
            ref LineSegment2D final_lineanother,
            PointF final_canny_crossPoint)
        {
            LineSegment2D linestand = new LineSegment2D();
            LineSegment2D lineanother = new LineSegment2D();
            linestand = lines[0];
            lineanother = lines[1];

            final_linestand.P1 = new Point(linestand.P1.X + roi.X, linestand.P1.Y + roi.Y);
            final_linestand.P2 = new Point(linestand.P2.X + roi.X, linestand.P2.Y + roi.Y);

            final_lineanother.P1 = new Point(lineanother.P1.X + roi.X, lineanother.P1.Y + roi.Y);
            final_lineanother.P2 = new Point(lineanother.P2.X + roi.X, lineanother.P2.Y + roi.Y);


            PointF crosspoint;
            double[] pt1 = new double[4]
            {
                final_linestand.P1.X, final_linestand.P1.Y,
                final_linestand.P2.X, final_linestand.P2.Y
            };
            double[] pt2 = new double[4]
            {
                final_lineanother.P1.X, final_lineanother.P1.Y,
                final_lineanother.P2.X, final_lineanother.P2.Y
            };


            crosspoint = getCrossPoint(pt1, pt2, final_canny_crossPoint);
            return crosspoint;
        }

        private static PointF CrossPointFromCanny(Mat roi_canny, Rectangle roi)
        {
            PointF canny_crossPoint = new PointF();
            int nl, nc;
            nl = roi_canny.Rows;
            nc = roi_canny.Cols * roi_canny.NumberOfChannels;
            int flag = 0;
            for (int i = nl - 1; i >= 0; i--)
            {
                Byte[] data = roi_canny.GetData();
                for (int j = 0; j < nc; j++)
                {
                    if (data[j] > 0)
                    {
                        flag = 1;
                        canny_crossPoint.X = j;
                        canny_crossPoint.Y = i;
                        break;
                    }
                    //cout << "最低的交叉点：" << data[j] << endl;
                }

                if (flag == 1)
                {
                    break;
                }
            }
            PointF final_canny_crossPoint = new PointF(); //最终的canny检测的点
            final_canny_crossPoint.X = canny_crossPoint.X + roi.X;
            final_canny_crossPoint.Y = canny_crossPoint.Y + roi.Y;
            return final_canny_crossPoint;
        }

        public static Mat sobelEdgeDetection(ref Mat src_roi)
        {
            Mat roi_gray = new Mat();
            Mat grad_x = new Mat();
            Mat grad_y = new Mat();
            Mat grad_absx = new Mat();
            Mat grad_absy = new Mat();
            Mat roi_soble = new Mat();
            CvInvoke.CvtColor(src_roi, roi_gray, ColorConversion.Rgb2Gray);

            CvInvoke.Sobel(roi_gray, grad_x, DepthType.Cv16S, 1, 0, 3, 1, 1, BorderType.Default);//x方向的sobel检测
            CvInvoke.ConvertScaleAbs(grad_x, grad_absx, 1, 0);

            CvInvoke.Sobel(roi_gray, grad_y, DepthType.Cv16S, 0, 1, 3, 1, 1, BorderType.Default);//y方向的sobel检测
            CvInvoke.ConvertScaleAbs(grad_y, grad_absy, 1, 0);

            CvInvoke.AddWeighted(grad_absx, 0.5, grad_absy, 0.5, 0, roi_soble);

            return roi_soble;
        }

        /*函数功能：求两条直线交点*/
        /*输入：两条Vec4i类型直线,喷头最下边的点*/
        /*返回：Point2f类型的点*/
        private static PointF getCrossPoint(double[] LineA, double[] LineB, PointF final_canny_crossPoint)
        {
            //此种情况是ka,kb都不为0或者都不在坐标轴上
            double ka, kb;
            ka = (double)(LineA[3] - LineA[1]) / (double)(LineA[2] - LineA[0]); //求出LineA斜率
            kb = (double)(LineB[3] - LineB[1]) / (double)(LineB[2] - LineB[0]); //求出LineB斜率

            PointF crossPointa = new PointF();
            PointF crossPointb = new PointF();
            PointF crossPoint = new PointF();

            double ka_dm = (double)(LineA[2] - LineA[0]);
            double kb_dm = (double)(LineB[2] - LineB[0]);

            double final_y = final_canny_crossPoint.Y;

            if (ka_dm != 0 && kb_dm != 0)//两条相交的直线都不垂直x轴
            {
                //crossPoint.x = (ka*LineA[0] - LineA[1] - kb*LineB[0] + LineB[1]) / (ka - kb);
                //crossPoint.y = (ka*kb*(LineA[0] - LineB[0]) + ka*LineB[1] - kb*LineA[1]) / (ka - kb);

                crossPointa.X = (float)((final_y - LineA[1]) / ka + LineA[0]);
                crossPointa.Y = (float)final_y;

                crossPointb.X = (float)((final_y - LineB[1]) / kb + LineB[0]);
                crossPointb.Y = (float)final_y;

                crossPoint.X = (float)((crossPointa.X + crossPointb.X) / 2);
                crossPoint.Y = (float)final_y;
            }
            else if (ka_dm == 0 || kb_dm != 0)//ka垂直,kb不垂直
            {
                /*crossPoint.x = LineA[0];
                crossPoint.y = kb*(LineA[0] - LineB[0]) + LineB[1];*/

                crossPointa.X = (float)LineA[0];
                crossPointa.Y = (float)final_y;

                crossPointb.X = (float)((final_y - LineB[1]) / kb + LineB[0]);
                crossPointb.Y = (float)final_y;

                crossPoint.X = (float)(crossPointa.X + crossPointb.X) / 2;
                crossPoint.Y = (float)final_y;


            }
            else if (ka_dm != 0 || kb_dm == 0)//kb垂直,Ka不垂直
            {
                /*crossPoint.x = LineB[0];
                crossPoint.y = ka*(LineB[0] - LineA[0]) + LineA[1];*/

                crossPointa.X = (float)((final_y - LineA[1]) / ka + LineA[0]);
                crossPointa.Y = (float)final_y;

                crossPointb.X = (float)LineB[0];
                crossPointb.Y = (float)final_y;

                crossPoint.X = (float)(crossPointa.X + crossPointb.X) / 2;
                crossPoint.Y = (float)final_y;

            }
            else //2组点都在坐标轴上
            {
                //cout << "error!\n" << endl;
            }

            return crossPoint;
        } 
        #endregion



    }
}