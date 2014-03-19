using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;


namespace EMPOWER_TALK
{
    class AbsDiff
    {
        private MainForm form;
        private IplImage imgLast;
        private IplImage imgDiff;
        private bool sudah_ambil = false;

        int diam = 0, gerak = 0, wave = 0;

        public AbsDiff(MainForm form)
        {
            this.form = form;
        }

        public void countWhitePix(IplImage image)
        {
            int p, white = 0;

            byte pix;

            byte[] data = image.ImageData;

            for (int x = 0; x < image.widthStep; x++)
            {
                for (int y = 0; y < image.height; y++)
                {
                    p = y * image.widthStep + x;

                    pix = data[p];

                    if (pix == 255)
                        white++;
                }
            }

            if (white < 50 && white < 5)
                diam++;
            else
                diam = 0;

            if (white > 100)
            {
                gerak++;
                if (white > 500)
                    wave++;
            }

            if (diam > 10)
            {
                gerak = 0;
                wave = 0;
                diam = 0;
                form.match = true;
            }

            if (wave > 10)
            {
                form.reset = true;
                wave = 0;
                gerak = 0;
                diam = 0;
            }

            cvlib.cvReleaseImage( image);
        }

        public void Absolute(IplImage imgNow)
        {
            imgDiff = cvlib.cvCreateImage(cvlib.cvGetSize( imgNow), imgNow.depth, imgNow.nChannels);

            if (!sudah_ambil)
            {
                imgLast = cvlib.cvCreateImage(cvlib.cvGetSize( imgNow), imgNow.depth, imgNow.nChannels);
                imgLast = cvlib.cvCloneImage( imgNow);
                sudah_ambil = true;
            }
            else
                sudah_ambil = false;

            cvlib.cvAbsDiff( imgNow,  imgLast,  imgDiff);

            cvlib.cvSmooth( imgDiff,  imgDiff);
            cvlib.cvSmooth( imgDiff,  imgDiff);

            if (form.showAbs)
                cvlib.cvShowImage("Motion",  imgDiff);

            countWhitePix(imgDiff);

            if (!sudah_ambil)
                cvlib.cvReleaseImage( imgLast);

            cvlib.cvReleaseImage( imgNow);
            cvlib.cvReleaseImage( imgDiff);
        }
    }
}
