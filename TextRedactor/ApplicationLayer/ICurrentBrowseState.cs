namespace ApplicationLayer
{
    internal interface ICurrentBrowseState
    {
        string CurrentProject { get; }

        string CurrentFile { get; }

        string WorkDirectory { get;}

        FileSystemQueue Queue { get; }
    }
}
