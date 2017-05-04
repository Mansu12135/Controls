using System.Collections.Generic;

namespace Controls
{
    interface IDictionaryManager
    {
        List<Structure> GetInformation(string word);
        List<string> GetSynonyms(string word);
        List<string> GetAntonyms(string word);
    }
}
