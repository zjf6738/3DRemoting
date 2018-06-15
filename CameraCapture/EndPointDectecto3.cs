using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Tracking;
using System.Runtime.InteropServices;

namespace HelloWorld
{
    public class EndPointDectector
    {
        // 检测器的原始输入图像帧
        protected Mat frame = null;
        // 缩放图像的ROI
        protected Rectangle roi = new Rectangle();
        // 跟踪器
        protected Tracker tracker = null;

        // 缩放的图像帧
        protected Mat frame_scale = new Mat();
        // 原始图像的ROI
        Rectangle roi_scale = new Rectangle(375 - 5, 122 - 5, 21 + 5, 58 + 5);

        // kmeans 处理
        Mat src_roi = new Mat();
        //Mat roi_sobel = new Mat();

        //求取末端点的位置
        Mat roi_canny = new Mat();
        Mat roi_gray = new Mat();
        Mat roi_threshold = new Mat();
        //Mat roi_erode = new Mat();
        Mat roi_kmeans = new Mat();
        Mat roi_blur = new Mat();
        Mat roi_cannyx = new Mat();

        //复制hougline直线检测的部分
        //Mat roi_pro = new Mat();
        //用这个显示检测的末端点和拟合的线
        Mat roi_temp = new Mat();
        //用这个单独显示检测的末端点
        Mat roi_singletemp = new Mat();

        LineSegment2D[] lines = new LineSegment2D[0];
        LineSegment2D final_linestand = new LineSegment2D();
        LineSegment2D final_lineanother = new LineSegment2D(); //最终的hougline的直线首末点

        private Size scaleSize = new Size(480, 320);

        public EndPointDectector(Mat _frame, Rectangle _roi)
        {
            frame_scale = _frame;
            roi = _roi;
            tracker = new TrackerKCF();
            tracker.Init(frame_scale, roi);
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

        public Mat Roi_Kmeans
        {
            get { return roi_kmeans; }
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
            PointF crossPoint = new PointF(-1, -1);

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

            // 打印喷头的分类处理----分割出喷头
            src_roi = new Mat(frame, roi_scale);
            //roi_kmeans = get_roi_Sprinkler_area(ref src_roi);
            roi_kmeans = get_roi_Sprinkler_area(src_roi);

            //roi_pro = new Mat(frame_scale, roi);//复制跟踪包围框部分
            roi_temp = frame.Clone();//用这个显示检测的末端点和拟合的线
            roi_singletemp = frame.Clone();//用这个单独显示检测的末端点


            // 原frame中的roi区域分类后的色空间转换、阈值化操作、边缘平滑、canny边缘提取
            Size blur_size = new Size(3, 3);
            int srcthreshold = 100;

            CvInvoke.CvtColor(roi_kmeans, roi_gray, ColorConversion.Bgr2Gray);//转化为灰度图像
            CvInvoke.Threshold(roi_gray, roi_threshold, 180, 255, ThresholdType.Binary);//阈值化操作
            imageblur(roi_threshold, roi_blur, blur_size, srcthreshold);
            //Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2, 1), new Point(-1, -1));//腐蚀操作
            //CvInvoke.Erode(roi_threshold, roi_erode, element, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 255, 255));

            CvInvoke.Canny(roi_blur, roi_canny, 10, 200, 3);

            //canny检测后直线的末端点判断
            PointF final_canny_crossPoint = CrossPointFromCanny(roi_canny, roi_scale);

            //去掉canny检测后上半部分的
            roi_cannyx = SegmenttopFromCanny(roi_canny);

            //hough直线检测
            lines = CvInvoke.HoughLinesP(roi_cannyx, 1, Math.PI / 360, 40, 50, 380);//hough直线

            if (lines.Length > 1)
            {
                crossPoint = CrossPointFromHough(lines, roi_scale, ref roi_temp, ref final_linestand, ref final_lineanother, final_canny_crossPoint);
            }

            return crossPoint;
        }



        #region 工具函数
        //获取最终的喷头末端交点
        private static PointF CrossPointFromHough(LineSegment2D[] lines,
            Rectangle roi_scale,
            ref Mat roi_temp,
            ref LineSegment2D final_linestand,
            ref LineSegment2D final_lineanother,
            PointF final_canny_crossPoint)
        {
            LineSegment2D linestand = new LineSegment2D();
            LineSegment2D lineanother = new LineSegment2D();
            linestand = lines[0];
            lineanother = lines[1];

            double ka, kb;
            double[] lines0 = new double[4]
            {
                lines[0].P1.X, lines[0].P1.Y,
                lines[0].P2.X, lines[0].P2.Y
            };

            double[] lines1 = new double[4]
            {
                lines[1].P1.X, lines[1].P1.Y,
                lines[1].P2.X, lines[1].P2.Y
            };

            ka = getAngle(lines0);//求出第1条直线倾斜角
            kb = getAngle(lines1);//求出第2条直线倾斜角

            if (lines.Length == 2)
            {
                linestand = lines[0];
                lineanother = lines[1];
            }
            else
            {
                double kc;

                double[] lines2 = new double[4]
                {
                    lines[2].P1.X, lines[2].P1.Y,
                    lines[2].P2.X, lines[2].P2.Y
                };

                kc = getAngle(lines2);//求出第3条直线倾斜角

                if (Math.Abs(ka - kb) >= 8)
                {
                    linestand = lines[0];
                    lineanother = lines[1];
                }
                else if ((Math.Abs(ka - kb) < 8) && (Math.Abs(kb - kc) > 8))
                {
                    linestand = lines[0];
                    lineanother = lines[2];
                }
                else
                {
                    Console.WriteLine("检测直线在一边！");
                }

            }

            //最终的hougline的直线首末点
            final_linestand.P1 = new Point(linestand.P1.X + roi_scale.X, linestand.P1.Y + roi_scale.Y);
            final_linestand.P2 = new Point(linestand.P2.X + roi_scale.X, linestand.P2.Y + roi_scale.Y);

            final_lineanother.P1 = new Point(lineanother.P1.X + roi_scale.X, lineanother.P1.Y + roi_scale.Y);
            final_lineanother.P2 = new Point(lineanother.P2.X + roi_scale.X, lineanother.P2.Y + roi_scale.Y);


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

            Console.WriteLine("pt1：" + pt1[0].ToString() + "," + pt1[1].ToString() + "," + pt1[2].ToString() + "," + pt1[3].ToString());
            Console.WriteLine("pt2：" + pt2[0].ToString() + "," + pt2[1].ToString() + "," + pt2[2].ToString() + "," + pt2[3].ToString());
            Console.WriteLine("final_canny_crossPoint：" + final_canny_crossPoint.ToString());

            return crosspoint;
        }

        //获取canny边缘检测边缘图像的末端的横坐标
        private static PointF CrossPointFromCanny(Mat roi_canny, Rectangle roi_scale)
        {
            PointF canny_crossPoint = new PointF();
            int nl, nc;
            nl = roi_canny.Rows;
            nc = roi_canny.Cols * roi_canny.NumberOfChannels;
            int flag = 0;

            //for (int i = nl - 1; i >= 0; i--)
            //{
            //    Byte[] data = roi_canny.GetData();
            //    for (int j = 0; j < nc; j++)
            //    {
            //        if (data[j] > 0)
            //        {
            //            flag = 1;
            //            canny_crossPoint.X = j;
            //            canny_crossPoint.Y = i;
            //            break;
            //        }
            //    }

            //    if (flag == 1)
            //    {
            //        break;
            //    }
            //}

            Image<Gray, byte> src = roi_canny.ToImage<Gray, byte>();
            for (int i = nl - 1; i >= 0; i--)
            {
                // Byte[] data = roi_canny.GetData();
                for (int j = 0; j < nc; j++)
                {
                    if (src[i, j].Intensity > 0)
                    {
                        flag = 1;
                        canny_crossPoint.X = j;
                        canny_crossPoint.Y = i;
                        break;
                    }
                }

                if (flag == 1)
                {
                    break;
                }
            }

            PointF final_canny_crossPoint = new PointF(); //最终的canny检测的点
            final_canny_crossPoint.X = canny_crossPoint.X + roi_scale.X;
            final_canny_crossPoint.Y = canny_crossPoint.Y + roi_scale.Y;//为什么求解过程y坐标被减去了20

            //Console.WriteLine("canny检测的末端点：" + canny_crossPoint.ToString());
            //Console.WriteLine("原图canny检测的末端点：" + final_canny_crossPoint.ToString());

            return final_canny_crossPoint;
        }

        //去掉canny边缘检测的上部的一小部分
        private static Mat SegmenttopFromCanny(Mat roi_canny)
        {
            Mat roi_cannyx = roi_canny.Clone();
            Image<Gray, byte> src = roi_canny.ToImage<Gray, byte>();

            for (int i = 0; i < roi_cannyx.Rows; i++)
            {
                //Byte[] p = roi_cannyx.GetData();
                for (int j = 0; j < roi_cannyx.Cols; j++)
                {
                    if (i < 0.1 * roi_cannyx.Rows)
                    {
                        src.Data[i, j, 0] = 0;
                    }
                }
            }
            return src.Mat;
            //return roi_cannyx;
        }

        //sobel边缘检测
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
            double k1=0, k2=0;
            k1 = (double)(LineA[3] - LineA[1]) / (double)(LineA[2] - LineA[0]); //求出LineA斜率
            k2 = (double)(LineB[3] - LineB[1]) / (double)(LineB[2] - LineB[0]); //求出LineB斜率

            PointF crossPointa = new PointF();
            PointF crossPointb = new PointF();
            //PointF fitcrossPoint = new PointF();
            PointF cannycrossPoint = new PointF();
            PointF finalcrossPoint = new PointF();

            double ka_dm = (double)(LineA[2] - LineA[0]);
            double kb_dm = (double)(LineB[2] - LineB[0]);

            //double final_x = final_canny_crossPoint.X;
            double final_y = final_canny_crossPoint.Y;

            //首先运用所给的直线进行拟合求的末端点
            //if (ka_dm != 0 && kb_dm != 0)//点都不在坐标轴上
            //{
            //    double b1 = LineA[1] - ka * LineA[0]; //第1条直线的b(y=k*x+b)
            //    double b2 = LineB[1] - ka * LineB[0]; //第2条直线的b(y=k*x+b)

            //    fitcrossPoint.X = (float)((ka * LineA[0] - LineA[1] - kb * LineB[0] + LineB[1]) / (ka - kb));
            //    fitcrossPoint.Y = (float)((ka * kb * (LineA[0] - LineB[0]) + ka * LineB[1] - kb * LineA[1]) / (ka - kb));

            //}
            //else if (ka_dm == 0 || kb_dm != 0)//垂直
            //{

            //    fitcrossPoint.X = (float)LineA[0];
            //    fitcrossPoint.Y = (float)(kb * (LineA[0] - LineB[0]) + LineB[1]);

            //}
            //else if (ka_dm != 0 || kb_dm == 0)//垂直
            //{

            //    fitcrossPoint.X = (float)LineB[0];
            //    fitcrossPoint.Y = (float)(ka * (LineB[0] - LineA[0]) + LineA[1]);

            //}
            //else //2组点都在坐标轴上
            //{
            //    fitcrossPoint.X = 0;
            //    fitcrossPoint.Y = 0;
            //    //cout << "error!\n" << endl;
            //}

            //拟合的点在末端点下面---求取的canny检测的末端点
            double b1 = LineA[1] - k1 * LineA[0]; //第1条直线的b(y=k*x+b)
            double b2 = LineB[1] - k2 * LineB[0]; //第2条直线的b(y=k*x+b)

            if (ka_dm != 0 && kb_dm != 0)//两条相交的直线都不垂直x轴
            {
                crossPointa.X = (float)((final_y - b1) / k1);
                crossPointa.Y = (float)final_y;

                crossPointb.X = (float)((final_y - b2) / k2);
                crossPointb.Y = (float)final_y;

                cannycrossPoint.X = (float)((crossPointa.X + crossPointb.X) / 2);
                cannycrossPoint.Y = (float)final_y;
            }
            else if (ka_dm == 0 || kb_dm != 0)//ka垂直,kb不垂直
            {
                crossPointa.X = (float)LineA[0];
                crossPointa.Y = (float)final_y;

                crossPointb.X = (float)((final_y - b2) / k2);
                crossPointb.Y = (float)final_y;

                cannycrossPoint.X = (float)((crossPointa.X + crossPointb.X) / 2);
                cannycrossPoint.Y = (float)final_y;
            }
            else if (ka_dm != 0 || kb_dm == 0)//kb垂直,Ka不垂直
            {
                crossPointa.X = (float)((final_y - b1) / k1);
                crossPointa.Y = (float)final_y;

                crossPointb.X = (float)LineB[0];
                crossPointb.Y = (float)final_y;

                cannycrossPoint.X = (float)((crossPointa.X + crossPointb.X) / 2);
                cannycrossPoint.Y = (float)final_y;
            }
            else //2组点都在坐标轴上
            {
                Console.WriteLine("canny边缘求取的末端点不存在!");
                //cout << "canny边缘求取的末端点不存在!\n" << endl;
            }

            //最终的末端点的判断
            //if (cannycrossPoint.Y <= fitcrossPoint.Y)//如果canny检测的末端点在直线拟合交点的上面
            //{
            //    finalcrossPoint.X = cannycrossPoint.X;
            //    finalcrossPoint.Y = cannycrossPoint.Y;
            //}
            //else                                      //如果末端点不在拟合的上面
            //{
            //    finalcrossPoint.X = cannycrossPoint.X;
            //    finalcrossPoint.Y = cannycrossPoint.Y;
            //    //cout << "有打印的物质!\n" << endl;
            //}

            finalcrossPoint.X = cannycrossPoint.X;
            finalcrossPoint.Y = cannycrossPoint.Y;

            return finalcrossPoint;
        }

        //运用kmeans算法获取喷头末端的区域
        private static Mat get_roi_Sprinkler_area(Mat src_roi)
        {
            //运用kmeans方法，对roi区域进行分类，此处的处理只是分为三类
            //Mat roi_data = new Mat();
            //Mat roi_label = new Mat();
            Mat roi_kmeans = new Mat();
            //Mat temp = new Mat(new Size(1, 3), src_roi.Depth, 1);
            //Mat tmp = new Mat(new Size(1,3),DepthType.Cv32F,1);

            //for (int i = 0; i < src_roi.Rows; i++)
            //    for (int j = 0; j < src_roi.Cols; j++)
            //    {
            //        byte [] point = src_roi.GetData(i, j);
            //        //tmp = (Mat_<float>(1, 3) << point[0], point[1], point[2]);

            //        double [] fdata =  { point[0], point[1], point[2] };

            //        Mat temp = new Mat(new Size(1, 3), src_roi.Depth, 1);
            //        byte [] p_temp = temp.GetData();
            //        p_temp[0] = point[0];
            //        p_temp[1] = point[1];
            //        p_temp[2] = point[2];

            //        roi_data.PushBack(temp);
            //    }

            Image<Bgr, byte> src = src_roi.ToImage<Bgr, byte>();
            Matrix<float> roi_data = new Matrix<float>(src_roi.Rows * src.Cols, 1, 3);
            Matrix<int> roi_label = new Matrix<int>(src_roi.Rows * src.Cols, 1);

            for (int y = 0; y < src.Rows; y++)
            {
                for (int x = 0; x < src.Cols; x++)
                {
                    roi_data.Data[y + x * src.Rows, 0] = (float)src[y, x].Blue;
                    roi_data.Data[y + x * src.Rows, 1] = (float)src[y, x].Green;
                    roi_data.Data[y + x * src.Rows, 2] = (float)src[y, x].Red;
                }
            }

            MCvTermCriteria term = new MCvTermCriteria(10, 1.0);
            term.Type = TermCritType.Iter | TermCritType.Eps;
            int clusterCount = 3;
            int attempts = 3;
            Matrix<Single> centers = new Matrix<Single>(clusterCount, src.Rows * src.Cols);
            CvInvoke.Kmeans(roi_data, clusterCount, roi_label, term, attempts, KMeansInitType.RandomCenters);

            //CvInvoke.Kmeans(roi_data, 3, roi_label, new MCvTermCriteria(10, 1), 3, KMeansInitType.RandomCenters);


            int n = 0;
            roi_kmeans = src_roi.Clone();

            int label1 = 0, label2 = 0, label3 = 0;

            for (int i = 0; i < src_roi.Rows * src_roi.Cols; i++)
            {
                int clusterIdx = roi_label.Data[i, 0];
                if (clusterIdx == 0)
                {
                    label1++;
                }
                else if (clusterIdx == 1)
                {
                    label2++;
                }
                else if (clusterIdx == 2)
                {
                    label3++;
                }
            }

            int final_label = 0;

            if ((label3 >= label2 && label3 >= label1 && label1 >= label2) || (label2 >= label1 && label2 >= label3 && label1 >= label3))
            {
                final_label = 0;
            }
            else if ((label1 >= label2 && label1 >= label3 && label2 >= label3) || (label3 >= label2 && label3 >= label1 && label2 >= label1))
            {
                final_label = 1;
            }
            else if ((label1 >= label3 && label1 >= label2 && label3 >= label2) || (label2 >= label3 && label2 >= label1 && label3 >= label1))
            {
                final_label = 2;
            }


            Image<Bgr, byte> roiKeamsImg = roi_kmeans.ToImage<Bgr, byte>();
            for (int i = 0; i < src_roi.Rows; i++)
            {
                for (int j = 0; j < src_roi.Cols; j++)
                {
                    int clusterIdx = roi_label.Data[i + j * src.Rows, 0];

                    //if (clusterIdx == final_label)
                    //{
                    //    //roi_kmeans.at<Vec3b>(i, j) = new MCvScalar(0, 0, 255);

                    //    roi_kmeans.Data.SetValue(new MCvScalar(0, 0, 255), i, j);
                    //}
                    //else
                    //{
                    //    //roi_kmeans.at<Vec3b>(i, j) = new MCvScalar(255, 255, 255);
                    //    roi_kmeans.Data.SetValue(new MCvScalar(255, 255, 255), i, j);
                    //}

                    if (clusterIdx == final_label)
                    {
                        roiKeamsImg.Data[i, j, 0] = 0;
                        roiKeamsImg.Data[i, j, 1] = 0;
                        roiKeamsImg.Data[i, j, 2] = 255;
                    }
                    else
                    {
                        roiKeamsImg.Data[i, j, 0] = 255;
                        roiKeamsImg.Data[i, j, 1] = 255;
                        roiKeamsImg.Data[i, j, 2] = 255;
                    }

                    //roi_kmeans.at<Vec3b>(i, j) = colorTab[clusterIdx]
                    n++;
                }
            }

            return roiKeamsImg.Mat;
            //return roi_kmeans;

        }

        //阈值化处理后图像的边缘光滑处理  
        //size表示取均值的窗口大小，threshold表示对均值图像进行二值化的阈值  
        private static void imageblur(Mat roi_threshold, Mat roi_blur, Size blur_size, int srcthreshold)
        {
            int height = roi_threshold.Rows;
            int width = roi_threshold.Cols;

            //CvInvoke.Blur(roi_threshold,roi_blur,blur_size);
            CvInvoke.Blur(roi_threshold, roi_blur, blur_size, new Point(-1, -1));

            Image<Gray, byte> src = roi_blur.ToImage<Gray, byte>();

            for (int i = 0; i < height; i++)
            {
                // Byte[] p = roi_blur.GetData();
                for (int j = 0; j < width; j++)
                {
                    if (src[i, j].Intensity < srcthreshold)
                        src.Data[i, j, 0] = 0;
                    else
                        src.Data[i, j, 0] = 255;
                }
            }

            roi_blur = src.Mat;
            //imshow("Blur", dst);

        }

        //获取两条直线的夹角
        private static double getAngle(double[] lines)
        {
            double x, y, z;
            x = (double)(lines[2] - lines[0]);
            y = (double)(lines[3] - lines[1]);

            z = Math.Atan(y / x) * 180 / Math.PI;

            //if (x<0 && y>0)z += 180;//第2象限
            //if (x<0 && y <= 0)z += 180;//第3象限
            //if (x >= 0 && y<0)z += 360;//第4象限

            if (z < 0)
            {
                z = z + 180;
            }
            return z;
        }

        #endregion



    }
}