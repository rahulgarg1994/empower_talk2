#pragma once

namespace EMPOWER_TALK
{
    ref class preprocessing
    {
        private: CvMat data;
        private: MainForm form;
        public: void cariX(IplImage imgSrc, int& min, int& max);
        public: void cariY(IplImage imgSrc, int& min, int& max);
        public: CvRect cariBB(IplImage imgSrc);
        public: preprocessing(MainForm form);
        public: IplImage preprocess(IplImage imgSrc, int new_width, int new_height);
    };
}

