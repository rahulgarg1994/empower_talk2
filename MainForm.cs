using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace EMPOWER_TALK
{
    public partial class MainForm : Form
    {
       
        #region user defined vars

        private DlgParams dlgParam = null;
        private DlgParams dlgCanny = null;
        private KNearest KNN = null;

        private CvCapture videoCapture;
        private SkinDetect skinDet = null;
        private HaarClassifier hc = null;
        private AbsDiff abs = null;

        private int fps = 0, hasil;

        public bool showROI = false,
            showThres = true,
            showEdge = false,
            showSkinHSV = true,
            showSkinRGB = false,
            showAbs = true,
            reset = false,
            initialized = false,
            capture = false,
            match = false,
            show_letter = false;

        #region declaration color / scalar
        private CvScalar merah = cvlib.CV_RGB(250, 0, 0);
        private CvScalar hijau = cvlib.CV_RGB(0, 250, 0);
        private CvScalar biru = cvlib.CV_RGB(0, 0, 250);
        #endregion

        #region IplImages
        private IplImage frame;
        private IplImage imgMain;
        private IplImage imgSkin;
        private IplImage imgBin;
        private IplImage imgGray;
        private IplImage imgCrop;
        private IplImage imgMot;
        #endregion

        public int openx, openy, closex, closey;
        public int roiX, roiY, roiH, roiW;

        #region declaration directory & files
        private string namaFile = string.Empty;
        private string finalName = string.Empty;
        private string tempName = string.Empty;
        string dir = "..\\..\\Training\\";
        string ext = ".pbm";
        string[] Signs = { "A", "B", "C", "D", "F", "G", "H", "I", "K", "L", "O", "P", "Q", "R", "U", "V", "W", "X", "Y", " ", "?" };
        #endregion

        #endregion

        public MainForm()
        {
            InitializeComponent();
        }

        public void WriteLine(string s, bool crlf, bool date)
        {
            if ((s.Length + textBox.TextLength) > textBox.MaxLength)
                textBox.Clear();
            if (!crlf && !date)
                textBox.AppendText(s);
            if (!crlf && date)
                textBox.AppendText(DateTime.Now.ToString() + ">> " + s);
            if (crlf && !date)
                textBox.AppendText(s + "\r\n");
            if (crlf && date)
                textBox.AppendText(DateTime.Now.ToString() + ">> " + s + "\r\n");
        }

        #region keyboard & button click events
        private void btnVideo_Click(object sender, EventArgs e)
        {
            double vidWidth, vidHeight;

            if (btnVideo.Text.Equals("Start Video"))
            {
                train_data();

               videoCapture= cvlib.cvCreateCameraCapture(0);

               cvlib.cvWaitKey(10);
                
                //check if valid
                if (videoCapture.ptr == IntPtr.Zero)
                {
                    MessageBox.Show("Failed shooting");
                    return;
                }

                btnVideo.Text = "Stop Video";

                cvlib.cvSetCaptureProperty( videoCapture, cvlib.CV_CAP_PROP_FRAME_WIDTH, 640);
                cvlib.cvSetCaptureProperty( videoCapture, cvlib.CV_CAP_PROP_FRAME_HEIGHT, 320);

               IplImage frame= cvlib.cvQueryFrame( videoCapture);
                

                vidWidth = cvlib.cvGetCaptureProperty(videoCapture, cvlib.CV_CAP_PROP_FRAME_WIDTH);
                vidHeight = cvlib.cvGetCaptureProperty(videoCapture, cvlib.CV_CAP_PROP_FRAME_HEIGHT);

                picBoxMain.Width = (int)vidWidth;
                picBoxMain.Height = (int)vidHeight;

                WriteLine("Taking pictures from a webcam with a resolution: " + vidWidth.ToString() + " x " + vidHeight.ToString(), true, false);

                timerGrab.Interval = 42;
                timerFPS.Interval = 1100;
                timerGrab.Enabled = true;
                timerFPS.Enabled = true;

                hc = new HaarClassifier(this);
                abs = new AbsDiff(this);
            }
            else
            {
                btnVideo.Text = "Start Video";
                timerFPS.Enabled = false;
                timerGrab.Enabled = false;

                if (videoCapture.ptr != IntPtr.Zero)
                {
                    cvlib.cvReleaseCapture( videoCapture);
                    videoCapture.ptr = IntPtr.Zero;
                }
            }
        }
        private void train_data()
        {
            KNN = new KNearest(this);

            WriteLine("Training...", true, true);

            KNN.getData();
            KNN.train();

            WriteLine("The training process has been completed", true, true);
        }

        private void trainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            train_data();
        }

        private void thresholdToolStripMenuItem_Click()
        {
            showThres = true;
            thresholdToolStripMenuItem.Checked = true;
            edgeToolStripMenuItem.Checked = false;
            showEdge = false;
        }

        private void edgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showEdge = true;
            edgeToolStripMenuItem.Checked = true;
            thresholdToolStripMenuItem.Checked = false;
            showThres = false;
        }

        private void hSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSkinHSV = true;
            hSVToolStripMenuItem.Checked = true;
            rGBToolStripMenuItem.Checked = false;
            showSkinRGB = false;
        }

        private void rGBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSkinRGB = true;
            rGBToolStripMenuItem.Checked = true;
            hSVToolStripMenuItem.Checked = false;
            showSkinHSV = false;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            namaFile = e.KeyCode.ToString();
            capture = true;
        }
        #endregion

        #region other methods
        private void skin_dlg()
        {
            if (skinDet == null)
                skinDet = new SkinDetect(this);

            if (dlgParam == null)
            {
                dlgParam = new DlgParams();
                dlgParam.BackColor = Color.FromArgb(244, 239, 240);
                dlgParam.Icon = this.Icon;
                dlgParam.Text = "Noise Removal";
                dlgParam.AddTrackbar("Dilate", 0, 0, 10, 1, 1);
                dlgParam.AddTrackbar("Erode", 1, 0, 10, 1, 1);
                dlgParam.AddTrackbar("Smooth", 2, 0, 10, 1, 1);
            }
        }

        private void edge_dlg()
        {
            if (dlgCanny == null)
            {
                dlgCanny = new DlgParams();
                dlgCanny.BackColor = Color.FromArgb(244, 239, 240);
                dlgCanny.Icon = this.Icon;
                dlgCanny.Text = "Canny";
                dlgCanny.AddTrackbar("Thres 1", 0, 0, 255, 1, 0);
                dlgCanny.AddTrackbar("Thres 2", 1, 0, 255, 1, 0);
                dlgCanny.Show();
                dlgCanny.Location = Point(675, 500);
            }
        }

        private System.Drawing.Point Point(int p1, int p2)
        {
            throw new NotImplementedException();
        }

        public void euclidean()
        {
            int jarak = (int)Math.Sqrt(Math.Pow((openx - closex), 2) + Math.Pow((openy - closey), 2));
            if (jarak < 50)
                showROI = true;
            else
                openy = openx = closex = closey = 0;
        }

        public void initialize()
        {
            cvlib.cvNamedWindow("Crop");
            cvlib.cvMoveWindow("Crop", 675, 10);

            cvlib.cvNamedWindow("Motion");
            cvlib.cvMoveWindow("Motion", 950, 10);

            skin_dlg();
            dlgParam.Show();
            dlgParam.Location = Point(675, 300);

            initialized = true;
        }

        public void resetting()
        {
           cvlib.cvDestroyAllWindows();

            openy = openx = closex = closey = 0;

            if (dlgParam != null)
                dlgParam.Hide();
            if (dlgCanny != null)
                dlgCanny.Hide();

            if (thresholdToolStripMenuItem.Checked)
                showThres = true;
            if (edgeToolStripMenuItem.Checked)
                showEdge = true;
            if (hSVToolStripMenuItem.Checked)
                showSkinHSV = true;
            if (rGBToolStripMenuItem.Checked)
                showSkinRGB = true;

            textBox.Clear();

            reset = false;
        }

        public bool adaBlackPix(IplImage image)
        {
            int p, black = 0;

            byte pix;

            byte[] data = image.ImageData;

            for (int x = 0; x < image.widthStep; x++)
            {
                for (int y = 0; y < image.height; y++)
                {
                    p = y * image.widthStep + x;

                    pix = data[p];

                    if (pix == 0)
                        black++;
                }
            }

            if (black < 1000)
                return false;
            else
                return true;
        }

        #endregion

        #region timer events
        private void timerGrab_Tick(object sender, EventArgs e)
        {
           IplImage frame = cvlib.cvQueryFrame( videoCapture);

            if (frame.ptr == IntPtr.Zero)
            {
                timerGrab.Stop();
                MessageBox.Show("Invalid Frame");
                return;
            }

            imgMain = cvlib.cvCreateImage(cvlib.cvGetSize( frame), 8, 3);

            if (reset)
            {
                showROI = false;
                initialized = false;
                resetting();
            }

            cvlib.cvCopy( frame,  imgMain);
            cvlib.cvFlip( imgMain);

            #region ROI
            if (showROI && initialized)
            {
                cvlib.cvRectangle( imgMain, cvlib.cvPoint(roiX, roiY), cvlib.cvPoint(roiX + roiW, roiY + roiH), cvlib.CV_RGB(255, 0, 125), 1, 8, 0);
                imgCrop = cvlib.cvCreateImage(cvlib.cvSize(roiW, roiH), 8, 3);

                #region skinHSV/RGB
                if (showSkinHSV || showSkinRGB)
                {
                    imgSkin = new IplImage();
                    imgSkin = cvlib.cvCreateImage(cvlib.cvGetSize( frame), 8, 3);
                    if (showSkinHSV)
                        imgSkin = skinDet.skin_hsv(imgMain);
                    else if (showSkinRGB)
                        imgSkin = skinDet.skin_rgb(imgMain);
                    cvlib.cvSetImageROI( imgSkin, cvlib.cvRect(roiX, roiY, roiW, roiH));
                    cvlib.cvCopy( imgSkin,  imgCrop);
                    cvlib.cvReleaseImage( imgSkin);

                    //noise removal
                    cvlib.cvDilate(imgCrop,  imgCrop, dlgParam.GetP(0).i);
                    cvlib.cvErode( imgCrop,  imgCrop, dlgParam.GetP(1).i);
                    for (int i = 0; i < dlgParam.GetP(2).i; i++)
                        cvlib.cvSmooth( imgCrop,  imgCrop);
                }
                #endregion

                #region show threshold
                if (showThres || showEdge)
                {
                    imgGray = cvlib.cvCreateImage(cvlib.cvGetSize( imgCrop), 8, 1);
                    imgBin = cvlib.cvCreateImage(cvlib.cvGetSize( imgCrop), 8, 1);
                    imgMot = cvlib.cvCreateImage(cvlib.cvGetSize( imgCrop), 8, 1);

                    cvlib.cvCvtColor( imgCrop,  imgGray, cvlib.CV_BGR2GRAY);

                    cvlib.cvThreshold( imgGray,  imgMot, 0, 255, cvlib.CV_THRESH_BINARY_INV);
                    abs.Absolute(imgMot);

                    if (showThres)
                        cvlib.cvThreshold( imgGray,  imgBin, 0, 255, cvlib.CV_THRESH_BINARY_INV);
                    else if (showEdge)
                    {
                        edge_dlg();
                        cvlib.cvCanny( imgGray,  imgBin, dlgCanny.GetP(0).i, dlgCanny.GetP(1).i);
                    }

                    cvlib.cvShowImage("Crop",  imgBin);

                    #region matching
                    if (match)
                    {
                        if (adaBlackPix(imgBin))
                            hasil = (int)KNN.classify(ref imgBin, false);
                        else
                            hasil = 19;
                        WriteLine(Signs[hasil], false, false);

                        match = false;
                        show_letter = true;
                    }
                    #endregion

                    cvlib.cvReleaseImage( imgGray);
                    cvlib.cvReleaseImage( imgCrop);
                    cvlib.cvReleaseImage( imgBin);
                    cvlib.cvReleaseImage( imgMot);
                }
                else
                {
                    cvlib.cvShowImage("Crop",  imgCrop);
                    cvlib.cvReleaseImage( imgCrop);
                }
                #endregion
            }
            else if (!initialized && !showROI)
                imgMain = hc.cariHaar(imgMain);
            else if (!initialized) //initialize windows
                initialize();
            #endregion

            if (show_letter)
            {
                CvFont font = new CvFont();
                cvlib.cvInitFont( font, cvlib.CV_FONT_HERSHEY_SIMPLEX, 5, 5, 0, 10, cvlib.CV_AA);
                cvlib.cvPutText( imgMain, Signs[hasil], cvlib.cvPoint(50, 200),  font, cvlib.cvScalar(0, 255, 0));
            }

            picBoxMain.Image = cvlib.ToBitmap(imgMain, false);

            cvlib.cvReleaseImage( imgMain);

            fps++;

            if ((openx != 0 && openy != 0 && closex != 0 && closey != 0) && !showROI)
                euclidean();
        }

        private void timerFPS_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel.Text = "Frame Rate: " + fps.ToString() + " Fps";
            fps = 0;
        }
        #endregion
    }
}
