using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;


namespace EMPOWER_TALK
{
    class MotionTemp
    {
        private MainForm form;

        const double MHI_DURATION = 1;
        const double MAX_TIME_DELTA = 0.5;
        const double MIN_TIME_DELTA = 0.05;

        const int N = 4;
        int last = 0;

        IplImage[] buf = new IplImage[10];
        IplImage mhi;
        IplImage orient;
        IplImage mask;
        IplImage segmask;
        CvMemStorage storage;

        public MotionTemp(MainForm form)
        {
            this.form = form;
        }

        public void update_mhi(IplImage imgMain, ref IplImage imgDst, int diff_threshold)
        {
            double timestamp = (double)DateTime.Now.Second;
            CvSize size = cvlib.cvSize(imgMain.width, imgMain.height);
            int i, idx1 = last, idx2;
            IplImage silh;
            CvSeq seq;
            CvRect comp_rect;
            double count;
            double angle;
            CvPoint center;
            double magnitude;
            CvScalar color;

            //allocate images at the beginning or reallocate them if the frame size is changed
            if (mhi.ptr == null || mhi.width != size.width || mhi.height != size.height)
            {
                for (i = 0; i < N; i++)
                {
                    buf[i] = cvlib.cvCreateImage(size, (int)cvlib.IPL_DEPTH_8U, 1);
                   
                }
                cvlib.cvReleaseImage( mhi);
                cvlib.cvReleaseImage( orient);
                cvlib.cvReleaseImage( segmask);
                cvlib.cvReleaseImage( mask);

                mhi = cvlib.cvCreateImage(size, (int)cvlib.IPL_DEPTH_32F, 1);
              
                orient = cvlib.cvCreateImage(size, (int)cvlib.IPL_DEPTH_32F, 1);
                segmask = cvlib.cvCreateImage(size, (int)cvlib.IPL_DEPTH_32F, 1);
                mask = cvlib.cvCreateImage(size, (int)cvlib.IPL_DEPTH_32F, 1);
            }

            cvlib.cvCvtColor( imgMain,  buf[last], cvlib.CV_BGR2GRAY);

            idx2 = (last + 1) % N;
            last = idx2;

            silh = buf[idx2];
            cvlib.cvAbsDiff( buf[idx1],  buf[idx2],  silh);

            cvlib.cvThreshold( silh,  silh, diff_threshold, 1, cvlib.CV_THRESH_BINARY);
            cvlib.cvUpdateMotionHistory( silh,  mhi, timestamp, MHI_DURATION);

            cvlib.cvConvertScale( mhi,  mask, 255 / MHI_DURATION, (MHI_DURATION - timestamp) * 255 / MHI_DURATION);
            cvlib.cvMerge( mask,  imgDst);
            cvlib.cvCalcMotionGradient( mhi,  mask,  orient, MAX_TIME_DELTA, MIN_TIME_DELTA, 3);
            if (storage.ptr == null)
                storage = cvlib.cvCreateMemStorage();
            else
                cvlib.cvClearMemStorage( storage);
            seq = cvlib.cvSegmentMotion( mhi,  segmask,  storage, timestamp, MAX_TIME_DELTA);
            for (i = -1; i < seq.total; i++)
            {
                if (i < 0)
                {
                    comp_rect = cvlib.cvRect(0, 0, size.width, size.height);
                    color = cvlib.CV_RGB(255, 255, 255);
                    magnitude = 100;
                }
                else
                {
                    IntPtr ptr = cvlib.cvGetSeqElem( seq, i);
                    CvConnectedComp c = (CvConnectedComp)cvlib.PtrToType(ptr, typeof(CvConnectedComp));
                    comp_rect = c.rect;
                    if (comp_rect.width + comp_rect.height < 100)
                        continue;
                    color = cvlib.CV_RGB(255, 0, 0);
                    magnitude = 30;
                }

                //select component ROI
                cvlib.cvSetImageROI( silh, comp_rect);
                cvlib.cvSetImageROI( mhi, comp_rect);
                cvlib.cvSetImageROI( orient, comp_rect);
                cvlib.cvSetImageROI( mask, comp_rect);

                //calculate orientation
                angle = cvlib.cvCalcGlobalOrientation( orient,  mask,  mhi, timestamp, MHI_DURATION);
                angle = 360 - angle;

                count = cvlib.cvNorm( silh); //<<<<<<<<<<<<<<< recheck

                cvlib.cvResetImageROI( mhi);
                cvlib.cvResetImageROI( orient);
                cvlib.cvResetImageROI( mask);
                cvlib.cvResetImageROI( silh);

                //check for the case of little motion
                if (count < comp_rect.width * comp_rect.height * 0.05)
                    continue;

                //draw a clock with arrow indicating the direction
                center = cvlib.cvPoint((comp_rect.x + comp_rect.width / 2), (comp_rect.y + comp_rect.height / 2));

                cvlib.cvCircle( imgDst, center, cvlib.cvRound(magnitude * 1.2), color, 3, cvlib.CV_AA, 0);
                cvlib.cvLine( imgDst, center,
                    cvlib.cvPoint(cvlib.cvRound(center.x + magnitude * Math.Cos(angle * Math.PI / 180)),
                    cvlib.cvRound(center.y - magnitude * Math.Sin(angle * Math.PI / 180))),
                    color, 3, cvlib.CV_AA, 0);
            }
        }
    }
}
