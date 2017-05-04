using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Microsoft.Office.Interop.Word;
using Application = System.Windows.Forms.Application;
using Point = System.Windows.Point;
using Word=Microsoft.Office.Interop.Word;
using System.Windows.Input;

namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для SuperRichTextBox.xaml
    /// </summary>
    public partial class SuperRichTextBox : UserControl
    {
       

        public SuperRichTextBox()
        {
            InitializeComponent();
           // AttachEventHandler();
        }

        //private void AttachEventHandler()
        //{
        //    MainControl.TextChanged += MainControl_TextChanged;
        //}

        //private void MainControl_TextChanged(object sender, EventArgs e)
        //{
        //}

        //private void Label_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        #region OldFunction
        /*    public List<string> GetSynonymsInternet(string word)
        {
            allDone.Reset();
            string url =
                "http://www.dictionaryapi.com/api/v1/references/thesaurus/xml/fast?key=10797f0b-f532-4728-a855-b875b4d54eb4";
            //string url = "http://thesaurus.altervista.org/thesaurus/v1?word="+word+"&language=en_US&output=xml&key=fGNIC0kX12ZXnP4vOEFs"; 
                //"http://watson.kmi.open.ac.uk/API/term/synonyms?term="+word+"";
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = WebRequestMethods.Http.Get;
            webRequest.Accept = "application/json";
            RequestState myRequestState = new RequestState(OperationType.Synonyms);
            myRequestState.request = webRequest;
            webRequest.BeginGetResponse(new AsyncCallback(RespCallback), myRequestState);
            allDone.WaitOne();
            return myRequestState.InformationList;
        }
     

        public List<string> GetDefinition(string word)
        {
            allDone.Reset();
            //http://www.dictionaryapi.com/api/v1/references/thesaurus/xml/fast?key=10797f0b-f532-4728-a855-b875b4d54eb4
            string url = 
                //"http://wordnetweb.princeton.edu/perl/webwn?s="+word+"&sub=Search+WordNet&o2=&o0=&o8=1&o1=1&o7=&o5=&o9=&o6=&o3=&o4=&h=000";//
                "http://api.wordnik.com:80/v4/word.json/" + word + "/definitions?limit=20&includeRelated=false&sourceDictionaries=all&useCanonical=false&includeTags=false&api_key=a2a73e7b926c924fad7001ca3111acd55af2ffabf50eb4ae5";

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = WebRequestMethods.Http.Get;
            webRequest.Accept = "application/json";
            RequestState myRequestState = new RequestState(OperationType.Definitions);
            myRequestState.request = webRequest;
            webRequest.BeginGetResponse(new AsyncCallback(RespCallback), myRequestState);
            allDone.WaitOne();
            return myRequestState.InformationList;
        }
       private static List<string> DeSerialize(string response, OperationType type)
       {
           List<string> text = new List<string>();
           string firstExpression="", secondExpression="";
           switch (type)
           {
               case OperationType.Definitions:
                   {
                       firstExpression = "text\":\"";
                       secondExpression = "\",\"score\"";
                       break;
                   }
               case OperationType.Synonyms:
                   {
                       firstExpression = "<synonyms>";
                       secondExpression = "</synonyms>";
                       break;
                   }
           }
           while (true)
           {
               int count = response.IndexOf(firstExpression);
               if (count == -1) { break; }
               response = response.Remove(0, count + firstExpression.Length);
               text.Add(response.Substring(0, response.IndexOf(secondExpression)));
           }
           return text;
       } 
      */
        #endregion

        //public BitmapSource Draw()
        //{
        //    Rect bounds = VisualTreeHelper.GetDescendantBounds(this);
        //    RenderTargetBitmap rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);
        //    DrawingVisual dv = new DrawingVisual();
        //    using (DrawingContext ctx = dv.RenderOpen())
        //    {
        //        VisualBrush vb = new VisualBrush(this);
        //        ctx.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
        //    }
        //    rtb.Render(dv);
        //    return rtb;
        //}
    }
}
