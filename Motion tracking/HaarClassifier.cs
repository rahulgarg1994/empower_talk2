using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace EMPOWER_TALK
{
    class HaarClassifier
    {
        private MainForm form;

        CvHaarClassifierCascade cascadeO;
        CvHaarClassifierCascade cascadeC;
        SkinDetect skinDet;
        CvRect hc_rect, ho_rect;

        private IplImage imgSkin;

        public HaarClassifier(MainForm form)
        {
            this.form = form;
            skinDet = new SkinDetect(this.form);
        }

        public IplImage cariHaar(IplImage image)
        {
            cvlib.cvFlip( image);

            imgSkin = new IplImage();
            imgSkin = cvlib.cvCreateImage(cvlib.cvGetSize( image), 8, 3);
            imgSkin = skinDet.skin_hsv(image);

            IplImage gray = cvlib.cvCreateImage(cvlib.cvSize(imgSkin.width, imgSkin.height), (int)cvlib.IPL_DEPTH_8U, 1);
            cvlib.cvCvtColor( imgSkin,  gray, cvlib.CV_BGR2GRAY);

            IplImage small_image = imgSkin;
            CvMemStorage storage = cvlib.cvCreateMemStorage(0);
            CvSeq handOpen, handClose;
            int i, scale = 1;
            bool do_pyramids = true;

            #region percepat proses
            if (do_pyramids)
            {
                small_image = cvlib.cvCreateImage(cvlib.cvSize(imgSkin.width / 2, imgSkin.height / 2), (int)cvlib.IPL_DEPTH_8U, 3);
                cvlib.cvPyrDown( imgSkin,  small_image, (int)cvlib.CV_GAUSSIAN_5x5);
                scale = 2;
            }
            #endregion

            #region open hand
            IntPtr ptrO = cvlib.cvLoad("..\\..\\Training\\handOpen.xml");
            cascadeO = (CvHaarClassifierCascade)cvlib.PtrToType(ptrO, typeof(CvHaarClassifierCascade));
            cascadeO.ptr = ptrO;
            handOpen = cvlib.cvHaarDetectObjects(ref small_image, ref cascadeO, ref storage, 1.2, 2, cv.CV_HAAR_DO_CANNY_PRUNING, cvlib.cvSize(0, 0));
            if (handOpen.total != 0)
            {
                for (i = 0; i < handOpen.total; i++)
                {
                    ho_rect = (CvRect)cvlib.PtrToType(cvlib.cvGetSeqElem( handOpen, i), typeof(CvRect));
                    cvlib.cvRectangle( image, cvlib.cvPoint(ho_rect.x * scale - 10, ho_rect.y * scale - 10),
                        cvlib.cvPoint((ho_rect.x + ho_rect.width) * scale + 10, (ho_rect.y + ho_rect.height) * scale + 10),
                        cvlib.CV_RGB(255, 0, 0), 1, 8, 0);
                }
                form.closex = 0;
                form.closey = 0;
                form.openx = image.width - ((ho_rect.x * scale) + ((ho_rect.width * scale) / 2));
                form.openy = ho_rect.y * scale + ((ho_rect.height * scale) / 2);

                form.roiX = 640 - (ho_rect.x * scale - 10) - (ho_rect.width * scale + 10);
                form.roiY = ho_rect.y * scale - 10;
                form.roiW = ho_rect.width * scale + 10;
                form.roiH = ho_rect.height * scale + 10;
            }
            #endregion

            #region close hand
            if (handOpen.total == 0)
            {
                IntPtr ptrC = cvlib.cvLoad("..\\..\\Training\\handClose.xml");
                cascadeC = (CvHaarClassifierCascade)cvlib.PtrToType(ptrC, typeof(CvHaarClassifierCascade));
                cascadeC.ptr = ptrC;
                handClose = cvlib.cvHaarDetectObjects( small_image,  cascadeC,  storage, 1.2, 2, cvlib.CV_HAAR_DO_CANNY_PRUNING, cvlib.cvSize(0, 0));
                if (handClose.total != 0)
                {
                    for (i = 0; i < handClose.total; i++)
                    {
                        hc_rect = (CvRect)cvlib.PtrToType(cvlib.cvGetSeqElem( handClose, i), typeof(CvRect));
                        cvlib.cvRectangle( image, cvlib.cvPoint(hc_rect.x * scale, hc_rect.y * scale),
                                            cvlib.cvPoint((hc_rect.x + hc_rect.width) * scale, (hc_rect.y + hc_rect.height) * scale),
                                            cvlib.CV_RGB(0, 0, 255), 1, 8, 0);
                    }
                    form.closex = image.width - ((hc_rect.x * scale) + ((hc_rect.width * scale) / 2));
                    form.closey = hc_rect.y * scale + ((hc_rect.height * scale) / 2);
                }
            }
            #endregion

            cvlib.cvReleaseMemStorage( storage);
            cvlib.cvReleaseHaarClassifierCascade( cascadeO);
            if (handOpen.total == 0)
                cvlib.cvReleaseHaarClassifierCascade( cascadeC);
            cvlib.cvReleaseImage( gray);
            cvlib.cvReleaseImage( small_image);
            cvlib.cvReleaseImage( imgSkin);
            cvlib.cvFlip( image);
            return image;
        }
    }
}
