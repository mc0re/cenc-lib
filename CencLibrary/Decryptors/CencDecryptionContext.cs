namespace CencLibrary
{
    public class CencDecryptionContext
    {
        public IList<string> Messages { get; } = new List<string>();

        
        internal int AddError(string message)
        {
            Messages.Add(message);
            return -1;
        }


        internal void AddWarning(string message)
        {
            Messages.Add(message);
        }
    }
}