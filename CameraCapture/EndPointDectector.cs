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
        Mat roi_erode = new Mat();

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

            // ��ӡ��ͷ�ı�Ե���
            src_roi = new Mat(frame, roi_scale);
            roi_sobel = sobelEdgeDetection(ref src_roi);

            roi_pro = new Mat(frame_scale, roi);//���Ƹ��ٰ�Χ�򲿷�
            roi_temp = frame_scale.Clone();//�������ʾ����ĩ�˵����ϵ���
            roi_singletemp = frame_scale.Clone();//�����������ʾ����ĩ�˵�

            //// ����ͼ��roi�������ɫ�ռ�任����ֵ����������̬ѧ������canny��Ե��ȡ
            CvInvoke.CvtColor(roi_pro, roi_gray, ColorConversion.Bgr2Gray);//ת��Ϊ�Ҷ�ͼ��
            CvInvoke.Threshold(roi_gray, roi_threshold, 30, 255, ThresholdType.Binary);//��ֵ������
            Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2, 1), new Point(-1, -1));//��ʴ����
            CvInvoke.Erode(roi_threshold, roi_erode, element, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 255, 255));
            CvInvoke.Canny(roi_erode, roi_canny, 10, 200, 3);

            //canny����ֱ�ߵ�ĩ�˵��ж�
            PointF final_canny_crossPoint = CrossPointFromCanny(roi_canny, roi);

            //houghֱ�߼��
            lines = CvInvoke.HoughLinesP(roi_canny, 1, Math.PI / 360, 20, 7, 10);//houghֱ��


            if (lines.Length > 1)
            {
                crossPoint = CrossPointFromHough(lines, roi, ref roi_temp, ref final_linestand, ref final_lineanother, final_canny_crossPoint);
            }

            return crossPoint;
        }


        #region ���ߺ���
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
            PointF crossPoint = new PointF();

            double ka_dm = (double)(LineA[2] - LineA[0]);
            double kb_dm = (double)(LineB[2] - LineB[0]);

            double final_y = final_canny_crossPoint.Y;

            if (ka_dm != 0 && kb_dm != 0)//�����ཻ��ֱ�߶�����ֱx��
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
            else if (ka_dm == 0 || kb_dm != 0)//ka��ֱ,kb����ֱ
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
            else if (ka_dm != 0 || kb_dm == 0)//kb��ֱ,Ka����ֱ
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
            else //2��㶼����������
            {
                //cout << "error!\n" << endl;
            }

            return crossPoint;
        } 
        #endregion



    }
}