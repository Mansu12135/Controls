using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ApplicationLayer
{
    public class RequestState
    {
        public static readonly int BUFFER_SIZE = 32 * 1024;
        public byte[] Buffer;
        public StringBuilder Data;
        public List<Structure> InformationList;
        public WebRequest Request;
        public WebResponse Response;
        public Stream ResponseStream;
        public RequestState()
        {
            Buffer = new byte[BUFFER_SIZE];
            Request = null;
            Data = new StringBuilder();
            ResponseStream = null;
        }
    }
    public class Structure
    {
        public string Term { get; set; }
        public string PartOfSpeech { get; set; }
        public List<string> Definitions { get; set; }
        public List<string> Antonyms { get; set; }
        public List<string> Synonyms { get; set; }
    }
}
