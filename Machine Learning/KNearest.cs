using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace EMPOWER_TALK
{
    class KNearest
    {
        private MainForm form;
        private preprocessing p;

        #region global vars
        string file_path = "C:\\EMPOWER TALK\\EMPOWER TALK\\Training\\";

        int train_samples = 5;
        int classes = 18;

        public IplImage src_image;
        public IplImage prs_image;
        public IplImage img;
        public IplImage img32;

        private CvMat trainData;
        private CvMat trainClasses;

        int size = 150;

        int K;
        CvKNearest knn;
        #endregion

        public KNearest(MainForm form)
        {
            this.form = form;

            trainData = cvlib.cvCreateMat(train_samples * classes, size * size, cvlib.CV_32FC1);
            trainClasses = cvlib.cvCreateMat(train_samples * classes, 1, cvlib.CV_32FC1);

            K = int.Parse(form.txtK.Text);

            p = new preprocessing(form);
        }

        //For getData method makes use of data and training classes
        //training folder are subfolders according to the class masing2
        //each file name possessed cnn.pbm where c = class {0 .. 99} and n = {image sequence 00 .. 99}
        public void getData()
        {
            CvMat row = new CvMat();
            CvMat data = new CvMat();
            string file;
            int i = 7, j = 0;
            for (i = 0; i < classes; i++)
            {
                for (j = 0; j < train_samples; j++)
                {
                    if (j < 10)
                        file = file_path + i.ToString() + "\\" + i.ToString() + "0" + j.ToString() + ".pbm";
                    else
                        file = file_path + i.ToString() + "\\" + i.ToString() + j.ToString() + ".pbm";

                    //form.WriteLine("Training..." + file,true,true);

                    src_image = cvlib.cvLoadImage(file,cvlib.CV_LOAD_IMAGE_GRAYSCALE);
                   
                    if (src_image == null)
                    {
                        form.WriteLine("Error: Cant load image: " + file + "\n", true, true);
                    }
                  

                    //process file
                    prs_image = p.preprocess(src_image, size, size);

                    //set class label
                    cvlib.cvGetRow( trainClasses,  row, i * train_samples + j);
                    cvlib.cvSet( row, cvlib.cvRealScalar(i));

                    CvMat row_header = new CvMat();
                    CvMat row1 = new CvMat();

                    //set data
                    cvlib.cvGetRow( trainData,  row, i * train_samples + j);
                    img = cvlib.cvCreateImage(cvlib.cvSize(size, size), (int)cvlib.IPL_DEPTH_32F, 1);
                    //convert 8bits image to 32 bits
                    cvlib.cvConvertScale( prs_image,  img, 0.0039215, 0);
                    cvlib.cvGetSubRect( img,  data, cvlib.cvRect(0, 0, size, size));
                    //convert data matrix size x size to vector
                    row1 = cvlib.cvReshape( data,  row_header, 0, 1);
                    cvlib.cvCopy( row1,  row);

                    cvlib.cvReleaseImage( src_image);
                    cvlib.cvReleaseImage( prs_image);
                    cvlib.cvReleaseImage( img);
                }
            }
        }

        //invoke knn class
        public void train()
        {
            knn =  CvKNearest(trainData, trainClasses, false, K);
        }

        private CvKNearest CvKNearest(CvMat trainData, CvMat trainClasses, bool p, int K)
        {
            throw new NotImplementedException();
        }

        //img For classifying method which is based on the nearest class
        public float classify(ref IplImage img, bool showResult)
        {
            CvMat data = new CvMat();
            CvMat results = new CvMat();                                                                    //<<<< check
            CvMat dist = new CvMat();                                                                       //<<<< check
            CvMat nearest = cvlib.cvCreateMat(1, K, cvlib.CV_32FC1);                                     //<<<< check        

            float result;
            //process file
            prs_image = p.preprocess(img, size, size);

            //set data
            img32 = cvlib.cvCreateImage(cvlib.cvSize(size, size), (int)cvlib.IPL_DEPTH_32F, 1);
            cvlib.cvConvertScale( prs_image, img32, 0.0039215, 0);
            cvlib.cvGetSubRect( img32,  data, cvlib.cvRect(0, 0, size, size));             //possible memory leak??

            CvMat row_header = new CvMat();
            CvMat row1 = new CvMat();                                                                       //<<< check

            //convert data matrix size x size to vector
            row1 = cvlib.cvReshape( data,  row_header, 0, 1);                                        //<<< check

            result = knn.find_nearest(row1, K, results, IntPtr.Zero, nearest, dist);

            int accuracy = 0;
            for (int i = 0; i < K; i++)
            {
                if (nearest.fl[i] == result)
                    accuracy++;
            }
            float pre = 100 * ((float)accuracy / (float)K);
            if (showResult == true)
            {
                form.WriteLine("|\tClass\t\t|\tPrecision\t\t|\tAccuracy/K\t|\n", false, false);
                form.WriteLine("|\t" + result.ToString() + "\t\t|\t" + pre.ToString("N2") + "% \t\t|\t" + accuracy.ToString() + "/" + K.ToString() + "\t\t|" + "\n", false, false);
                form.WriteLine(" -------------------------------------------------------------------------------------------------------------------------------------------------\n", false, false);
            }

            cvlib.cvReleaseImage( img);

            return result;
        }

        //For testing the method set image (For test validation using training data)
        public void test()
        {
            string file = null;
            int i, j;
            int error = 0;
            int testCount = 0;
            for (i = 5; i < classes; i++)
            {
                for (j = 90; j < 100; j++)
                {
                    file = file_path + i.ToString() + "\\" + i.ToString() + j.ToString() + ".pbm";
                    src_image = cvlib.cvLoadImage(file, cvlib.CV_LOAD_IMAGE_GRAYSCALE);
                    if (src_image == null)
                    {
                        form.textBox.AppendText("Error: Cant load image: " + file + "\n");
                    }
                    //process file
                    //prs_image = p.preprocess(src_image, size, size);
                    float r = classify(ref src_image, true);
                    if ((int)r != i)
                        error++;

                    testCount++;
                }
            }
            float totalerror = 100 * (float)error / (float)testCount;
            form.textBox.AppendText("System Error : " + totalerror.ToString() + "\n");
        }
        
    }
}
