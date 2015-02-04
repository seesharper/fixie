namespace Fixie.VisualStudio.TestAdapter
{
    /// <summary>
    /// Represents the source location of a test method.
    /// </summary>
    public class SourceLocation
    {
        /// <summary>
        /// Gets the path of the source file that contains the test method.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Gets the line number of the test method.
        /// </summary>
        public int LineNumber { get; internal set; }
    }
}