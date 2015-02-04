namespace Fixie.VisualStudio.TestAdapter
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    /// <summary>
    /// A <see cref="ISourceLocationProvider"/> that uses Mono.Cecil to 
    /// read method symbol information. 
    /// </summary>
    public class SourceLocationProvider : ISourceLocationProvider
    {
        const int Hidden = 16707566;

        private readonly ConcurrentDictionary<string, TypeDefinition[]> moduleTypeCache =
            new ConcurrentDictionary<string, TypeDefinition[]>();

        private readonly ConcurrentDictionary<string, TypeDefinition> typeCache =
            new ConcurrentDictionary<string, TypeDefinition>();

        /// <summary>
        /// Gets the <see cref="SourceLocation"/> for the given <paramref name="methodName"/>.
        /// </summary>
        /// <param name="assemblyFileName">The path of the assembly that contains the method.</param>
        /// <param name="className">The name of the class that contains the method.</param>
        /// <param name="methodName">The name of the method for which to return the <see cref="SourceLocation"/>.</param>
        /// <returns>The <see cref="SourceLocation"/> if found, otherwise, null.</returns>
        public SourceLocation GetSourceLocation(string assemblyFileName, string className, string methodName)
        {
            MethodDefinition testMethod = GetMethod(assemblyFileName, className, methodName);
            SequencePoint sequencePoint;
            CustomAttribute asyncStateMachineAttribute;
            if (TryGetAsyncStateMachineAttribute(testMethod, out asyncStateMachineAttribute))
            {
                var stateMachineMoveNextMethod = GetStateMachineMoveNextMethod(asyncStateMachineAttribute);
                sequencePoint = GetMethodSequencePoint(stateMachineMoveNextMethod.Body);
            }
            else
            {
                sequencePoint = GetMethodSequencePoint(testMethod.Body);
            }

            if (sequencePoint == null)
            {
                return null;
            }

            return new SourceLocation
            {
                Path = sequencePoint.Document.Url,
                LineNumber = sequencePoint.StartLine
            };
        }

        private static MethodDefinition GetStateMachineMoveNextMethod(CustomAttribute asyncStateMachineAttribute)
        {
            var stateMachineType = (TypeDefinition)asyncStateMachineAttribute.ConstructorArguments[0].Value;
            var stateMachineMoveNextMethod = stateMachineType.GetMethods().First(m => m.Name == "MoveNext");
            return stateMachineMoveNextMethod;
        }

        private static bool TryGetAsyncStateMachineAttribute(MethodDefinition method, out CustomAttribute attribute)
        {
            attribute =
                method.CustomAttributes.FirstOrDefault(c => c.AttributeType.Name == "AsyncStateMachineAttribute");
            return attribute != null;
        }

        private static TypeDefinition[] GetTypes(string assemblyName)
        {
            ReaderParameters readerParameters = new ReaderParameters { ReadSymbols = true };
            ModuleDefinition module = ModuleDefinition.ReadModule(assemblyName, readerParameters);
            return module.GetTypes().ToArray();
        }

        private static SequencePoint GetMethodSequencePoint(MethodBody body)
        {
            foreach (var instruction in body.Instructions)
            {
                if (instruction.SequencePoint != null && instruction.SequencePoint.StartLine != Hidden)
                {
                    return instruction.SequencePoint;
                }
            }

            return null;
        }

        private MethodDefinition GetMethod(string assemblyFileName, string className, string methodName)
        {
            var type = typeCache.GetOrAdd(className, cn => GetType(assemblyFileName, cn));
            return type.GetMethods().First(m => m.Name == methodName);
        }

        private TypeDefinition GetType(string assemblyFileName, string className)
        {
            TypeDefinition[] types = moduleTypeCache.GetOrAdd(assemblyFileName, GetTypes);
            return types.First(t => t.FullName == className);
        }
    }
}