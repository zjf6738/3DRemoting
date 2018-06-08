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
        // �������ԭʼ����ͼ��֡
        protected Mat frame = null;
        // ԭʼͼ���ROI
        protected Rectangle roi = new Rectangle();
        // ������
        protected Tracker tracker = null; 

        // ���ŵ�ͼ��֡
        protected Mat frame_scale = new Mat();
        // ����ͼ���ROI
        Rectangle roi_scale = new Rectangle(375 - 5, 122 - 5, 21 + 5, 58 + 5);

        // sobel
        Mat src_roi = new Mat();
        Mat roi_sobel = new Mat();

        //��ȡĩ�˵��λ��
        Mat roi_canny = new Mat();
        Mat roi_gray = new Mat();
        Mat roi_threshold = new Mat();
        //Mat roi_erode = new Mat();
        Mat roi_kmeans = new Mat();
        //Mat roi_threshold= new Mat();
        Mat roi_blur = new Mat();
        Mat roi_cannyx = new Mat();

        //����houglineֱ�߼��Ĳ���
        Mat roi_pro = new Mat();
        //�������ʾ����ĩ�˵����ϵ���
        Mat roi_temp = new Mat();
        //�����������ʾ����ĩ�˵�
        Mat roi_singletemp = new Mat();

        LineSegment2D[] lines = new LineSegment2D[0];
        LineSegment2D final_linestand = new LineSegment2D();
        LineSegment2D final_lineanother = new LineSegment2D(); //���յ�hougline��ֱ����ĩ��

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
            PointF crossPoint = new PointF(-1,-1);

            frame = _frame;
            // �Ϸ����ж�
            if (frame == null) return crossPoint;
            if (frame.IsEmpty) return crossPoint;

            // ͼ������
            CvInvoke.Resize(frame, frame_scale, scaleSize, 0, 0, Inter.Cubic);//��С�ߴ�
            //����Ŀ�겢����ģ��
            tracker.Update(frame_scale, out roi);

            if (roi.Width == 0 || roi.Height == 0) return crossPoint;

            //ROI����ķ���
            float Fheight = frame.Rows;
            float Fwidth = frame.Cols;
            float WidthScale = (Fwidth / scaleSize.Width);//��ȵ����ű���
            float HeightScale = (Fheight / scaleSize.Height);//�߶ȵ����ű���

            //�Ŵ�roi����
            roi_scale.X = (int)Math.Round(roi.X * WidthScale);
            roi_scale.Y = (int)Math.Round(roi.Y * HeightScale);
            roi_scale.Width = (int)Math.Round(roi.Width * WidthScale);
            roi_scale.Height = (int)Math.Round(roi.Height * HeightScale);

            // ��ӡ��ͷ�ķ��ദ��
            src_roi = new Mat(frame, roi_scale);
            roi_kmeans = get_roi_Sprinkler_area(ref src_roi);

            roi_pro = new Mat(frame_scale, roi);//���Ƹ��ٰ�Χ�򲿷�
            roi_temp = frame_scale.Clone();//�������ʾ����ĩ�˵����ϵ���
            roi_singletemp = frame_scale.Clone();//�����������ʾ����ĩ�˵�


            // ԭframe�е�roi���������ɫ�ռ�ת������ֵ����������Եƽ����canny��Ե��ȡ
            Size blur_size=new Size(3,3);
            int srcthreshold = 100;

            CvInvoke.CvtColor(roi_kmeans, roi_gray, ColorConversion.Bgr2Gray);//ת��Ϊ�Ҷ�ͼ��
            CvInvoke.Threshold(roi_gray, roi_threshold, 30, 255, ThresholdType.Binary);//��ֵ������
            imageblur(roi_threshold, roi_blur, blur_size, srcthreshold);
            //Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2, 1), new Point(-1, -1));//��ʴ����
            //CvInvoke.Erode(roi_threshold, roi_erode, element, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 255, 255));
            CvInvoke.Canny(roi_blur, roi_canny, 10, 200, 3);

            //canny����ֱ�ߵ�ĩ�˵��ж�
            PointF final_canny_crossPoint = CrossPointFromCanny(roi_canny, roi);

            //ȥ��canny�����ϰ벿�ֵ�
            roi_cannyx = SegmenttopFromCanny(roi_canny);

            //houghֱ�߼��
            lines = CvInvoke.HoughLinesP(roi_cannyx, 1, Math.PI / 360, 40, 50, 380);//houghֱ��


            if (lines.Length > 1)
            {
                crossPoint = CrossPointFromHough(lines, roi_scale, ref roi_temp, ref final_linestand, ref final_lineanother, final_canny_crossPoint);
            }

            return crossPoint;
        }



        #region ���ߺ���
        private static PointF CrossPointFromHough( LineSegment2D[] lines,
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

            ka = getAngle(lines0);//�����1��ֱ����б��
            kb = getAngle(lines1);//�����2��ֱ����б��

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

                kc = getAngle(lines2);//�����3��ֱ����б��

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
                    Console.WriteLine("���ֱ����һ�ߣ�");
                }
 
            }

            //���յ�hougline��ֱ����ĩ��
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
            return crosspoint;
        }

        //��ȡ����ֱ�ߵļн�

        private static PointF getCrossPoint(double[] pt1, double[] pt2, PointF final_canny_crossPoint, Mat roi_kmeans)
        {
            throw new NotImplementedException();
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
                    //cout << "��͵Ľ���㣺" << data[j] << endl;
                }

                if (flag == 1)
                {
                    break;
                }
            }

            PointF final_canny_crossPoint = new PointF(); //���յ�canny���ĵ�
            final_canny_crossPoint.X = canny_crossPoint.X + roi.X;
            final_canny_crossPoint.Y = canny_crossPoint.Y + roi.Y;

            return final_canny_crossPoint;
        }

        private static Mat SegmenttopFromCanny(Mat roi_canny)
        {
            Mat roi_cannyx = roi_canny.Clone();
            for (int i = 0; i < roi_cannyx.Rows; i++)
            {
                Byte[] p = roi_cannyx.GetData();
                for (int j = 0; j < roi_cannyx.Cols; j++)
                {
                    if (i < 0.1 * roi_cannyx.Rows)
                    {
                        p[j] = 0;
                    }
                }
            }
            return roi_cannyx;
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

            CvInvoke.Sobel(roi_gray, grad_x, DepthType.Cv16S, 1, 0, 3, 1, 1, BorderType.Default);//x�����sobel���
            CvInvoke.ConvertScaleAbs(grad_x, grad_absx, 1, 0);

            CvInvoke.Sobel(roi_gray, grad_y, DepthType.Cv16S, 0, 1, 3, 1, 1, BorderType.Default);//y�����sobel���
            CvInvoke.ConvertScaleAbs(grad_y, grad_absy, 1, 0);

            CvInvoke.AddWeighted(grad_absx, 0.5, grad_absy, 0.5, 0, roi_soble);

            return roi_soble;
        }

        /*�������ܣ�������ֱ�߽���*/
        /*���룺����Vec4i����ֱ��,��ͷ���±ߵĵ�*/
        /*���أ�Point2f���͵ĵ�*/
        private static PointF getCrossPoint(double[] LineA, double[] LineB, PointF final_canny_crossPoint)
        {
            //���������ka,kb����Ϊ0���߶�������������
            double ka, kb;
            ka = (double)(LineA[3] - LineA[1]) / (double)(LineA[2] - LineA[0]); //���LineAб��
            kb = (double)(LineB[3] - LineB[1]) / (double)(LineB[2] - LineB[0]); //���LineBб��

            PointF crossPointa = new PointF();
            PointF crossPointb = new PointF();
            //PointF crossPoint = new PointF();
            PointF fitcrossPoint = new PointF();
            PointF cannycrossPoint = new PointF();
            PointF finalcrossPoint = new PointF();

            double ka_dm = (double)(LineA[2] - LineA[0]);
            double kb_dm = (double)(LineB[2] - LineB[0]);

            double final_x = final_canny_crossPoint.X;
            double final_y = final_canny_crossPoint.Y;

            //��������������ֱ�߽���������ĩ�˵�
            if (ka_dm != 0 && kb_dm != 0)//�㶼������������
            {
                
                fitcrossPoint.X = (float)((ka * LineA[0] - LineA[1] - kb * LineB[0] + LineB[1]) / (ka - kb));
                fitcrossPoint.Y = (float)((ka * kb * (LineA[0] - LineB[0]) + ka * LineB[1] - kb * LineA[1]) / (ka - kb));
            
            }
            else if (ka_dm == 0 || kb_dm != 0)//��ֱ
            {
                
                fitcrossPoint.X = (float)LineA[0];
		        fitcrossPoint.Y = (float)(kb*(LineA[0] - LineB[0]) + LineB[1]);
            
            }
            else if (ka_dm != 0 || kb_dm == 0)//��ֱ
            {
                
                fitcrossPoint.X = (float)LineB[0];
                fitcrossPoint.Y = (float)(ka * (LineB[0] - LineA[0]) + LineA[1]);

            }
            else //2��㶼����������
            {
                fitcrossPoint.X = 0;
                fitcrossPoint.Y = 0;
                //cout << "error!\n" << endl;
            }

            //��ϵĵ���ĩ�˵�����---��ȡ��canny����ĩ�˵�
            if (ka_dm != 0 && kb_dm != 0)//�����ཻ��ֱ�߶�����ֱx��
            {
                //crossPoint.x = (ka*LineA[0] - LineA[1] - kb*LineB[0] + LineB[1]) / (ka - kb);
                //crossPoint.y = (ka*kb*(LineA[0] - LineB[0]) + ka*LineB[1] - kb*LineA[1]) / (ka - kb);

                crossPointa.X = (float)((final_y - LineA[1]) / ka + LineA[0]);
                crossPointa.Y = (float)final_y;

                crossPointb.X = (float)((final_y - LineB[1]) / kb + LineB[0]);
                crossPointb.Y = (float)final_y;

                cannycrossPoint.X = (float)((crossPointa.X + crossPointb.Y) / 2);
                cannycrossPoint.Y = (float)final_y;
            }
            else if (ka_dm == 0 || kb_dm != 0)//ka��ֱ,kb����ֱ
            {
                /*crossPoint.x = LineA[0];
                crossPoint.y = kb*(LineA[0] - LineB[0]) + LineB[1];*/

                crossPointa.X = (float)LineA[0];
                crossPointa.Y = (float)final_y;

                crossPointb.X = (float)((final_y - LineB[1]) / kb + LineB[0]);
                crossPointb.Y = (float)final_y;

                cannycrossPoint.X = (float)((crossPointa.X + crossPointb.X) / 2);
                cannycrossPoint.Y = (float)final_y;
            }
            else if (ka_dm != 0 || kb_dm == 0)//kb��ֱ,Ka����ֱ
            {
                /*crossPoint.x = LineB[0];
                crossPoint.y = ka*(LineB[0] - LineA[0]) + LineA[1];*/

                crossPointa.X = (float)((final_y - LineA[1]) / ka + LineA[0]);
                crossPointa.Y = (float)final_y;

                crossPointb.X = (float)LineB[0];
                crossPointb.Y = (float)final_y;

                cannycrossPoint.X = (float)((crossPointa.X + crossPointb.X) / 2);
                cannycrossPoint.Y = (float)final_y;
            }
            else //2��㶼����������
            {
                Console.WriteLine("canny��Ե��ȡ��ĩ�˵㲻����!");
                //cout << "canny��Ե��ȡ��ĩ�˵㲻����!\n" << endl;
            }

            //���յ�ĩ�˵���ж�
            if (cannycrossPoint.Y <= fitcrossPoint.Y)//���canny����ĩ�˵���ֱ����Ͻ��������
            {
                finalcrossPoint.X = cannycrossPoint.X;
                finalcrossPoint.Y = cannycrossPoint.Y;
            }
            else                                      //���ĩ�˵㲻����ϵ�����
            {
                finalcrossPoint.X = cannycrossPoint.X;
                finalcrossPoint.Y = cannycrossPoint.Y;
                //cout << "�д�ӡ������!\n" << endl;
            }

            return finalcrossPoint;
        }

        private static Mat get_roi_Sprinkler_area(ref Mat src_roi)
        {
            //����kmeans��������roi������з��࣬�˴��Ĵ���ֻ�Ƿ�Ϊ����

            Mat roi_data = new Mat();
            Mat roi_label = new Mat();
            Mat roi_kmeans = new Mat();
            //Mat tmp = new Mat(new Size(1,3),DepthType.Cv32F,1);
            
            for (int i = 0; i < src_roi.Rows; i++)
                for (int j = 0; j < src_roi.Cols; j++)
                {
                    byte [] point = src_roi.GetData(i, j);
                    //tmp = (Mat_<float>(1, 3) << point[0], point[1], point[2]);
                    
                    double [] fdata =  { point[0], point[1], point[2] };

                    Mat tmp = new Mat(new Size(1, 3), DepthType.Cv64F, 1);
                    Marshal.Copy(fdata, 0, tmp.DataPointer, 3);
                    //Marshal.Copy(tmp.DataPointer, fdata, 0, 1*3);
                    //Mat tmp = new Mat(new Size(1, 3), DepthType.Cv32F, 1,fdata,1);
                    //tmp.Data = new float[] { point[0], point[1], point[2] };
                    //tmp.Data.SetValue(point[0],0,0);
                    //tmp.Data.SetValue(point[1],1,0);
                    //tmp.Data.SetValue(point[2],2,0);

                    roi_data.PushBack(tmp);
                }

            for (int i = 0; i < roi_data.Rows; i++)
                for (int j = 0; j < roi_data.Cols; j++)
            {
                float point = (float) roi_data.Data.GetValue(i, j);
                Console.WriteLine(point);
                }

            //MCvTermCriteria
            //TermCritType.Eps

            CvInvoke.Kmeans(roi_data, 3, roi_label, new MCvTermCriteria(), 10, KMeansInitType.RandomCenters);
        
            //CvInvoke.Kmeans(roi_data, 3, roi_label, new MCvTermCriteria(TermCritType.Eps + TermCritType.Iter, 10, 1.0),3, KMEANS_RANDOM_CENTERS);
            
            int n = 0;
            roi_kmeans = src_roi.Clone();

	        int label1 = 0, label2 = 0, label3 = 0;
            
            for (int i = 0; i < src_roi.Rows*src_roi.Cols; i++)
            {
                Byte[] clusterIdx = roi_label.GetData();
                if (clusterIdx[i] == 0)
                {
                    label1++;
                }
                else if (clusterIdx[i] == 1)
                {
                    label2++;
                }
                else if (clusterIdx[i] == 2)
                {
                    label3++;
                }
            }
            
            int final_label = 0;
            
            if ((label3 >= label2 &&label3 >= label1 &&label1 >= label2) || (label2 >= label1 &&label2 >= label3 &&label1 >= label3))
            {
                final_label = 0;
            }
            else if ((label1 >= label2 &&label1 >= label3 &&label2 >= label3) || (label3 >= label2 &&label3 >= label1 &&label2 >= label1))
            {
                final_label = 1;
            }
            else if ((label1 >= label3 &&label1 >= label2 &&label3 >= label2) || (label2 >= label3 &&label2 >= label1 &&label3 >= label1))
            {
                final_label = 2;
            }
            
            for (int i = 0; i < src_roi.Rows; i++)
            {
                Byte[] clusterIdx = roi_label.GetData();
                for (int j = 0; j < src_roi.Cols; j++)
                {
                    if (clusterIdx[j] == final_label)
                    {
                        //roi_kmeans.at<Vec3b>(i, j) = new MCvScalar(0, 0, 255);
                        roi_kmeans.Data.SetValue(new MCvScalar(0, 0, 255), i, j);
                    }
                    else
                    {
                        //roi_kmeans.at<Vec3b>(i, j) = new MCvScalar(255, 255, 255);
                        roi_kmeans.Data.SetValue(new MCvScalar(255, 255, 255), i, j);
                    }
		             //roi_kmeans.at<Vec3b>(i, j) = colorTab[clusterIdx]
                    n++;
                }
            }
            return roi_kmeans;

        }

        //ͼƬ��Ե�⻬����  
        //size��ʾȡ��ֵ�Ĵ��ڴ�С��threshold��ʾ�Ծ�ֵͼ����ж�ֵ������ֵ  
        private static void imageblur(Mat roi_threshold, Mat roi_blur, Size blur_size, int srcthreshold) 
        {
            int height = roi_threshold.Rows;
            int width = roi_threshold.Cols;

            //CvInvoke.Blur(roi_threshold,roi_blur,blur_size);
            CvInvoke.Blur(roi_threshold, roi_blur, blur_size, new Point(-1, -1));
            
            for (int i = 0; i < height; i++)
            {
                Byte[] p = roi_blur.GetData();
                for (int j = 0; j < width; j++)
                {
                    if (p[j] < srcthreshold)
                        p[j] = 0;
                    else
                        p[j] = 255;
                }
            }
            //imshow("Blur", dst);

        }

        private static double getAngle(double[] line)
        {
            double x, y, z;
            x = (double)(line[2] - line[0]);
            y = (double)(line[3] - line[1]);

            z = Math.Atan(y / x) * 180 / Math.PI;

            //if (x<0 && y>0)z += 180;//��2����
            //if (x<0 && y <= 0)z += 180;//��3����
            //if (x >= 0 && y<0)z += 360;//��4����

            if (z < 0)
            {
                z += 180;
            }


            return z;
        }

        #endregion



    }
}