using System.Collections.Generic;

namespace ApplicationLayer
{
    public interface IDictionaryManager
    {
        List<Structure> GetInformation(string word);
        List<string> GetSynonyms(string word);
        List<string> GetAntonyms(string word);
    }
}
