#include "pch.h"
#include "preprocessing.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Linq;
using namespace System::Text;
using namespace System::Threading::Tasks;
using namespace CxCore;
using namespace Cv;
using namespace OtherLibs;
using namespace EMPOWER_TALK;

void preprocessing::cariX(IplImage imgSrc, int& min, int& max)
{
    Platform::bool minTemu = false;
    data = ref new CvMat();
    CvScalar maxVal = cxtypes::cvRealScalar(imgSrc::width * 255);
    CvScalar val = cxtypes::cvRealScalar(0);
    
    for (int i = 0; i < imgSrc::width; i++)
    {
        cxcore::CvGetCol(& imgSrc, & data, i);
        val = cxcore::CvSum(& data);
        
        if (val::val1 < maxVal::val1)
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

void preprocessing::cariY(IplImage imgSrc, int& min, int& max)
{
    Platform::bool minFound = false;
    data = ref new CvMat();
    CvScalar maxVal = cxtypes::cvRealScalar(imgSrc::width * 255);
    CvScalar val = cxtypes::cvRealScalar(0);
    
    for (int i = 0; i < imgSrc::height; i++)
    {
        cxcore::CvGetRow(& imgSrc, & data, i);
        val = cxcore::CvSum(& data);
        
        if (val::val1 < maxVal::val1)
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

CvRect preprocessing::cariBB(IplImage imgSrc)
{
    CvRect aux;
    int xmin, xmax, ymin, ymax, height, width;
    xmin = xmax = ymin = ymax = height = width = 0;
    cariX(imgSrc, & xmin, & xmax);
    cariY(imgSrc, & ymin, & ymax);
    width = xmax - xmin;
    height = ymax - ymin;
    double lebar = width * 1.5;
    height = (height >= (width * 1.5)) ? safe_cast<int>(lebar) : height;
    aux = ref new CvRect(xmin, ymin, width, height);
    return aux;
}

preprocessing::preprocessing(MainForm form)
{
    this->form = form;
}

IplImage preprocessing::preprocess(IplImage imgSrc, int new_width, int new_height)
{
    IplImage result;
    IplImage scaledResult;
    CvMat data = ref new CvMat();
    CvMat dataA = ref new CvMat();
    CvRect bb = ref new CvRect();
    CvRect bbA = ref new CvRect();
    bb = cariBB(imgSrc);
    cxcore::CvGetSubRect(& imgSrc, & data, ref new CvRect(bb::x, bb::y, bb::width, bb::height));
    int size = ((bb::width > bb::height)) ? bb::width : bb::height;
    result = cxcore::CvCreateImage(ref new CvSize(size, size), 8, 1);
    cxcore::CvSet(& result, ref new CvScalar(255, 255, 255));
    int x = safe_cast<int>(Math::Floor((size - bb::width) / 2.0f));
    int y = safe_cast<int>(Math::Floor((size - bb::height) / 2.0f));
    cxcore::CvGetSubRect(& result, & dataA, ref new CvRect(x, y, bb::width, bb::height));
    cxcore::CvCopy(& data, & dataA);
    scaledResult = cxcore::CvCreateImage(ref new CvSize(new_width, new_height), 8, 1);
    cv::CvResize(& result, & scaledResult, cv::CV_INTER_NN);
    return scaledResult;
}


