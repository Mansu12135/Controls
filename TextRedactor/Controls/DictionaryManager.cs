using Microsoft.Office.Interop.Word;
using Spire.Doc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

namespace Controls
{
    internal class DictionaryManager
    {
        private Application appWord;
        private object objLanguage = WdLanguageID.wdEnglishUS;
        private static ManualResetEvent allDone = new ManualResetEvent(false);

        private static void RespCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                RequestState myRequestState = (RequestState)asynchronousResult.AsyncState;
                WebRequest myWebRequest1 = myRequestState.Request;
                myRequestState.Response = myWebRequest1.EndGetResponse(asynchronousResult);
                Stream responseStream = myRequestState.Response.GetResponseStream();
                myRequestState.ResponseStream = responseStream;
                IAsyncResult asynchronousResultRead = responseStream.BeginRead(myRequestState.Buffer, 0, RequestState.BUFFER_SIZE, new AsyncCallback(ReadCallBack), myRequestState);

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception raised!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
        }

        private static void ReadCallBack(IAsyncResult asyncResult)
        {
            try
            {
                RequestState myRequestState = (RequestState)asyncResult.AsyncState;
                Stream responseStream = myRequestState.ResponseStream;
                int read = responseStream.EndRead(asyncResult);
                if (read > 0)
                {
                    myRequestState.Data.Append(Encoding.ASCII.GetString(myRequestState.Buffer, 0, read));
                    IAsyncResult asynchronousResult = responseStream.BeginRead(myRequestState.Buffer, 0, RequestState.BUFFER_SIZE, new AsyncCallback(ReadCallBack), myRequestState);
                }
                else
                {
                    if (myRequestState.Data.Length > 1)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(myRequestState.Data.ToString());
                        myRequestState.InformationList = GetExpandoFromXml(doc);
                    }
                    responseStream.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception raised!");
                Console.WriteLine("Source : {0}", e.Source);
                Console.WriteLine("Message : {0}", e.Message);
            }
            allDone.Set();

        }

        private static List<Structure> GetExpandoFromXml(XmlDocument document)
        {
            var list = new List<Structure>();
            foreach (XmlNode element in document.GetElementsByTagName("entry_list")[0].ChildNodes)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<" + element.Name + ">" + element.InnerXml + "</" + element.Name + ">");
                list.Add(ParseChildNode(doc));
            }
            return list;
        }

        private static Structure ParseChildNode(XmlDocument document)
        {
            Structure st = new Structure();
            st.Term = document.GetElementsByTagName("hw")[0].InnerText;
            st.PartOfSpeech = document.GetElementsByTagName("fl")[0].InnerText;
            st.Definitions = GetList(document.GetElementsByTagName("mc"));
            st.Antonyms = GetList(document.GetElementsByTagName("ant"));
            st.Synonyms = GetList(document.GetElementsByTagName("syn"));
            return st;
        }

        private static List<string> GetList(XmlNodeList list)
        {
            List<string> temp = new List<string>();
            foreach (XmlNode node in list)
            {
                temp.Add(node.InnerText);
            }
            return temp;
        }
        internal List<Structure> GetInformation(string word)
        {
            allDone.Reset();
            string url = "http://www.dictionaryapi.com/api/v1/references/thesaurus/xml/" + word + "?key=10797f0b-f532-4728-a855-b875b4d54eb4";
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = WebRequestMethods.Http.Get;
            RequestState myRequestState = new RequestState();
            myRequestState.Request = webRequest;
            webRequest.BeginGetResponse(new AsyncCallback(RespCallback), myRequestState);
            allDone.WaitOne();
            return myRequestState.InformationList;
        }
        internal List<string> GetSynonyms(string word)
        {
            appWord = new Microsoft.Office.Interop.Word.Application();
            List<string> synonymsList = new List<string>();
            object objFalse = false;
            SynonymInfo si =
            appWord.get_SynonymInfo(word, ref objLanguage);
            var MeaningList = si.MeaningList as Array;
            if (MeaningList == null) { return null; }
            foreach (string strMeaning in MeaningList)
            {
                object objMeaning = strMeaning;
                List<string> SynonymList = (si.get_SynonymList(ref objMeaning) as Array).Cast<string>().ToList();
                if (SynonymList == null) { continue; }
                synonymsList.AddRange(SynonymList);
            }
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(si);
            appWord.Quit(WdSaveOptions.wdDoNotSaveChanges);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(appWord);
            return synonymsList;
        }
        internal List<string> GetAntonyms(string word)
        {
            appWord = new Microsoft.Office.Interop.Word.Application();
            SynonymInfo si =
            appWord.get_SynonymInfo(word, ref objLanguage);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(si);
            appWord.Quit(WdSaveOptions.wdDoNotSaveChanges);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(appWord);
            return (si.AntonymList as Array).Cast<string>().ToList();
        }
    }
}
