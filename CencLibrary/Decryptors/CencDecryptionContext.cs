namespace CencLibrary
{
    public class CencDecryptionContext
    {
        public IList<string> Messages { get; } = new List<string>();
        
        public bool IsError { get; private set; }


        internal void AddError(string message)
        {
            Messages.Add(message);
            IsError = true;
        }


        internal void AddWarning(string message)
        {
            Messages.Add(message);
        }
    }
}