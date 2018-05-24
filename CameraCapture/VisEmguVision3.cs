using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;

namespace CameraCapture
{
    public class VisEmguVision3
    {
        // 视频采集
        private VideoCapture capture0 = null;
        private VideoCapture capture1 = null;
        private VideoCapture capture2 = null;
        private VideoCapture capture3 = null;
        // 采集的图片
        private Mat frame0 = null;
        private Mat frame1 = null;
        private VideoWriter vw0 = null;
        private VideoWriter vw1 = null;
        private Mat frame2 = null;
        private Mat frame3 = null;
        // 视频写
        private VideoWriter vw2 = null;
        private VideoWriter vw3 = null;

        // 采集及录制状态状态
        private bool captureInProgress = false;
        private bool videoInProgress = false;
        private char[] codec = { 'D', 'I', 'V', 'X' };
        private int fps = 25;

        // 当前的工作相机个数
        private int numberOfAvailableCamera = 0;

        // 当前视觉系统的工作状态
        private VisVisionState visState;

        // 父窗口
        private CameraCapture3 frm = null;


        public VisEmguVision3(CameraCapture3 _frm)
        {
            frm = _frm;
            //theVisMindVision = new VisMindVision3(frm.Handle);
        }

        #region 属性
        public VideoCapture Capture0
        {
            get { return capture0; }
            set { capture0 = value; }
        }

        public VideoCapture Capture1
        {
            get { return capture1; }
            set { capture1 = value; }
        }

        public VideoCapture Capture2
        {
            get { return capture2; }
            set { capture2 = value; }
        }

        public VideoCapture Capture3
        {
            get { return capture3; }
            set { capture3 = value; }
        }

        public Mat Frame0
        {
            get { return frame0; }
            set { frame0 = value; }
        }

        public Mat Frame1
        {
            get { return frame1; }
            set { frame1 = value; }
        }

        public Mat Frame2
        {
            get { return frame2; }
            set { frame2 = value; }
        }

        public Mat Frame3
        {
            get { return frame3; }
            set { frame3 = value; }
        }

        public bool CaptureInProgress
        {
            get { return captureInProgress; }
            set { captureInProgress = value; }
        }

        public bool VideoInProgress
        {
            get { return videoInProgress; }
            set { videoInProgress = value; }
        }

        public VideoWriter Vw0
        {
            get { return vw0; }
            set { vw0 = value; }
        }

        public VideoWriter Vw1
        {
            get { return vw1; }
            set { vw1 = value; }
        }

        public VideoWriter Vw2
        {
            get { return vw2; }
            set { vw2 = value; }
        }

        public VideoWriter Vw3
        {
            get { return vw3; }
            set { vw3 = value; }
        }

        public int NumberOfAvailableCamera
        {
            get { return numberOfAvailableCamera; }
            set { numberOfAvailableCamera = value; }
        }

        public VisVisionState VisState
        {
            get { return visState; }
            set { visState = value; }
        }

        #endregion


        public void Start()
        {
            CvInvoke.UseOpenCL = false;

            try
            {
                capture0 = new VideoCapture(0);
                capture0.ImageGrabbed += ProcessFrame0;

                capture1 = new VideoCapture(1);
                capture1.ImageGrabbed += ProcessFrame1;

                capture2 = new VideoCapture(2);
                capture2.ImageGrabbed += ProcessFrame2;

                capture3 = new VideoCapture(3);
                capture3.ImageGrabbed += ProcessFrame3;

                frame0 = new Mat();
                frame1 = new Mat();
                frame2 = new Mat();
                frame3 = new Mat();

            }
            catch (NullReferenceException excpt)
            {
                throw;
            }
        }

        public void End()
        {
            try
            {
                capture0.Stop();
                capture1.Stop();
                capture2.Stop();
                capture3.Stop();

                //capture0.ImageGrabbed -= ProcessFrame0;
                //capture0 = null;

                //capture1.ImageGrabbed -= ProcessFrame1;
                //capture1 = null;

                //capture2.ImageGrabbed -= ProcessFrame2;
                //capture2 = null;

                //capture3.ImageGrabbed -= ProcessFrame3;
                //capture3 = null;

                capture0.Dispose();
                capture1.Dispose();
                capture2.Dispose();
                capture3.Dispose();


                frame0.Dispose();
                frame1.Dispose();
                frame2.Dispose();
                frame3.Dispose();

                //frame0 = null;
                //frame1 = null;
                //frame2 = null;
                //frame3 = null;
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        #region 图像采集及处理
        private void ProcessFrame0(object sender, EventArgs arg)
        {
            if (capture0 != null && capture0.Ptr != IntPtr.Zero)
            {
                capture0.Retrieve(frame0, 0);

                frm.ImageBox0.Image = frame0.Bitmap;
                ProcessVideo(vw0, frame0);
            }
        }

        private void ProcessFrame1(object sender, EventArgs arg)
        {
            if (capture1 != null && capture1.Ptr != IntPtr.Zero)
            {
                capture1.Retrieve(frame1, 0);
                frm.ImageBox1.Image = frame1.Bitmap;
                ProcessVideo(vw1, frame1);
            }
        }

        private void ProcessFrame2(object sender, EventArgs arg)
        {
            if (capture2 != null && capture2.Ptr != IntPtr.Zero)
            {
                capture2.Retrieve(frame2, 0);
                frm.ImageBox2.Image = frame2.Bitmap;
                ProcessVideo(vw2, frame2);
            }
        }

        private void ProcessFrame3(object sender, EventArgs arg)
        {
            if (capture3 != null && capture3.Ptr != IntPtr.Zero)
            {
                capture3.Retrieve(frame3, 0);
                frm.ImageBox3.Image = frame3.Bitmap;
                ProcessVideo(vw3, frame3);
            }
        }

        private void ProcessVideo(VideoWriter vw, Mat frame)
        {
            if (videoInProgress)
            {
                vw.Write(frame);
            }
        }

        #endregion


        public void OnCapture()
        {
            if (capture0 != null && capture1 != null && capture2 != null && capture3 != null)
            {
                if (captureInProgress)
                {  //stop the capture
                    //captureButton.Text = "开始采集";
                    capture0.Pause();
                    capture1.Pause();
                    capture2.Pause();
                    capture3.Pause();
                }
                else
                {
                    //start the capture
                    //captureButton.Text = "停止采集";
                    capture0.Start();
                    capture1.Start();
                    capture2.Start();
                    capture3.Start();
                }

                captureInProgress = !captureInProgress;
            }
        }

        public void OnSnap(object sender, EventArgs e)
        {

        }

        public void OnRecord(object sender, EventArgs e)
        {
            string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (videoInProgress)
            {
                vw0 = new VideoWriter(dt + "-0.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                    25,
                    new Size(capture0.Width, capture0.Height),
                    true);

                vw1 = new VideoWriter(dt + "-1.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                    25,
                    new Size(capture0.Width, capture0.Height),
                    true);

                vw2 = new VideoWriter(dt + "-2.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                    25,
                    new Size(capture0.Width, capture0.Height),
                    true);

                vw3 = new VideoWriter(dt + "-3.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                    25,
                    new Size(capture0.Width, capture0.Height),
                    true);


                Application.Idle += new EventHandler(ProcessFrame0);
                Application.Idle += new EventHandler(ProcessFrame1);
                Application.Idle += new EventHandler(ProcessFrame2);
                Application.Idle += new EventHandler(ProcessFrame3);
            }
            else
            {
                vw0.Dispose();
                vw1.Dispose();
                vw2.Dispose();
                vw3.Dispose();
                Application.Idle -= new EventHandler(ProcessFrame0);
                Application.Idle -= new EventHandler(ProcessFrame1);
                Application.Idle -= new EventHandler(ProcessFrame2);
                Application.Idle -= new EventHandler(ProcessFrame3);
            }

            videoInProgress = !videoInProgress;



            //if (recordButton.Text == "一键录制")
            //{
            //    if (MessageBox.Show("开始录制吗？", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            //    {
            //        VisEmguVision3.videoInProgress = true;

            //        // char[] codec = { 'M', 'J', 'P', 'G' };
            //        char[] codec = { 'D', 'I', 'V', 'X' };
            //        int fps = 25;

            //        vw0 = new VideoWriter(dt + "-0.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //            25,
            //            new Size(capture0.Width,capture0.Height),
            //            true);

            //        vw1 = new VideoWriter(dt + "-1.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //                                25,
            //                                new Size(capture0.Width, capture0.Height),
            //                                true);

            //        vw2 = new VideoWriter(dt + "-2.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //                                25,
            //                                new Size(capture0.Width, capture0.Height),
            //                                true);

            //        vw3 = new VideoWriter(dt + "-3.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //                                25,
            //                                new Size(capture0.Width, capture0.Height),
            //                                true);


            //        Application.Idle += new EventHandler(ProcessFrame0);
            //        Application.Idle += new EventHandler(ProcessFrame1);
            //        Application.Idle += new EventHandler(ProcessFrame2);
            //        Application.Idle += new EventHandler(ProcessFrame3);

            //    }


            //}
            //else
            //{
            //    if (MessageBox.Show("停止录制吗？", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            //    {
            //        VisEmguVision3.videoInProgress = false;


            //        vw0.Dispose();
            //        vw1.Dispose();
            //        vw2.Dispose();
            //        vw3.Dispose();
            //        Application.Idle -= new EventHandler(ProcessFrame0);
            //        Application.Idle -= new EventHandler(ProcessFrame1);
            //        Application.Idle -= new EventHandler(ProcessFrame2);
            //        Application.Idle -= new EventHandler(ProcessFrame3);
            //        recordButton.Text = "录制";
            //    }
            //}
        }
    }
}