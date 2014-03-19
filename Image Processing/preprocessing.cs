using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace EMPOWER_TALK
{
    class preprocessing
    {

        private CvMat data;

        private MainForm form;

        #region preprocessing functions

        public void cariX(IplImage imgSrc, ref int min, ref int max)
        {
            bool minTemu = false;

            data = new CvMat();

            CvScalar maxVal = cvlib.cvRealScalar(imgSrc.width * 255);
            CvScalar val = cvlib.cvRealScalar(0);
            
            //For each column sum, if sum <width * 255 then we find min
            //then proceed to the end of me to find max, if sum <width * 255 then found a new max
            for (int i = 0; i < imgSrc.width; i++)
            {
                cvlib.cvGetCol( imgSrc,  data, i); //col
                val = cvlib.cvSum( data);
                if (val.Val < maxVal.Val)
                {
                    max = i;
                    if (!minTemu)
                    {
                        min = i;
                        minTemu = true;
                    }
                }
            }
        }

        public void cariY(IplImage imgSrc, ref int min, ref int max)
        {
            bool minFound = false;

            data = new CvMat();

            CvScalar maxVal = cvlib.cvRealScalar(imgSrc.width * 255);
            CvScalar val = cvlib.cvRealScalar(0);

            //For each row sum, if sum <width * 255 then we find min
            //then proceed to the end of me to find max, if sum <width * 255 then found a new max
            for (int i = 0; i < imgSrc.height; i++)
            {
                cvlib.cvGetRow( imgSrc,  data, i); //row
                val = cvlib.cvSum( data);
                if (val.val1 < maxVal.val1)
                {
                    max = i;
                    if (!minFound)
                    {
                        min = i;
                        minFound = true;
                    }
                }
            }
        }

        public CvRect cariBB(IplImage imgSrc)
        {
            CvRect aux;
            int xmin, xmax, ymin, ymax, height, width;
            xmin = xmax = ymin = ymax = height = width = 0;

            cariX(imgSrc, ref xmin, ref xmax);
            cariY(imgSrc, ref ymin, ref ymax);

            width = xmax - xmin;
            height = ymax - ymin;

            double lebar = width * 1.5;

            height = height >= (width * 1.5) ? (int)lebar : height;

            //form.WriteLine("height = " + height.ToString(), true, true);
            //form.WriteLine("width = " + width.ToString(), true, true);

            aux = cvlib.cvRect(xmin, ymin, width, height);

            return aux;
        }

        public preprocessing(MainForm form)
        {
            this.form = form;
        }

        public IplImage preprocess(IplImage imgSrc, int new_width, int new_height)
        {
            IplImage result;
            IplImage scaledResult;

            // A = aspect ratio maintained
            CvMat data = new CvMat();
            CvMat dataA = new CvMat();
            CvRect bb = new CvRect();
            CvRect bbA = new CvRect();

            bb = cariBB(imgSrc);
            //Search the data bounding box
            cvlib.cvGetSubRect( imgSrc,  data, cvlib.cvRect(bb.x, bb.y, bb.width, bb.height));

            //Create an image with a data width and height (aspect ratio = 1)
            int size = (bb.width > bb.height) ? bb.width : bb.height;
            result = cvlib.cvCreateImage(cvlib.cvSize(size, size), 8, 1);
            cvlib.cvSet( result, cvlib.cvScalar(255, 255, 255));

            int x = (int)Math.Floor((size - bb.width) / 2.0f);
            int y = (int)Math.Floor((size - bb.height) / 2.0f);
            cvlib.cvGetSubRect( result,  dataA, cvlib.cvRect(x, y, bb.width, bb.height));
            cvlib.cvCopy( data,  dataA);

            scaledResult = cvlib.cvCreateImage(cvlib.cvSize(new_width, new_height), 8, 1);
            cvlib.cvResize( result,  scaledResult, cvlib.CV_INTER_NN);

            return scaledResult;
        }

        #endregion 
    }
}
