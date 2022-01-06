using System.Collections.Generic;
using System.Linq;
using TKOM.Node;

namespace TKOM.Interpreter
{
    /// <summary>
    /// Keeps track of the functions so that the collection cannot contain two functions called in the same way.
    /// </summary>
    internal class FunctionsCollection
    {
        private readonly HashSet<Function> functions = new();

        /// <summary>
        /// Adds the <paramref name="function"/> to the collection if its call will not be ambiguous between any other in the collection.
        /// </summary>
        /// <param name="function"></param>
        /// <returns><c>true</c> on success, <c>false</c> else.</returns>
        public bool TryAdd(Function function)
        {
            var funs = functions.ToList();
            for (int i = 0; i < functions.Count; i++)
            {
                var paramTypes = funs[i].Parameters.Select(p => p.Type).ToList();
                for (int j = i + 1; j < functions.Count; j++)
                {
                    if (funs[j].CanBeCalledLike(funs[i].Name, paramTypes))
                        return false;
                }
            }

            functions.Add(function);
            return true;
        }
        /// <summary>
        /// Return <c>true</c> and sets <paramref name="function"/> accordingly if the collection contains
        /// a function with given <paramref name="name"/> and <paramref name="types"/>.<br></br>
        /// Return <c>false</c> and sets <paramref name="function"/> to <c>null</c> if the function was not found.
        /// </summary>
        public bool TryGet(string name, IList<Type> types, out Function function)
        {
            function = null;

            var funs = functions.ToList().FindAll(f => f.CanBeCalledLike(name, types));
            if (!funs.Any())
                return false;
            
            function = funs.Single();
            return true;
        }
    }
}
