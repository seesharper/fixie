namespace Fixie.VisualStudio.TestAdapter
{
    /// <summary>
    /// Represents a class that is capable of providing the <see cref="SourceLocation"/>
    /// for a given test method.
    /// </summary>
    public interface ISourceLocationProvider
    {
        /// <summary>
        /// Gets the <see cref="SourceLocation"/> for the given <paramref name="methodName"/>.
        /// </summary>
        /// <param name="assemblyFileName">The path of the assembly that contains the method.</param>
        /// <param name="className">The name of the class that contains the method.</param>
        /// <param name="methodName">The name of the method for which to return the <see cref="SourceLocation"/>.</param>
        /// <returns>The <see cref="SourceLocation"/> if found, otherwise, null.</returns>
        SourceLocation GetSourceLocation(string assemblyFileName, string className, string methodName);
    }
}