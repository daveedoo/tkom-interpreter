using System;
using System.Collections.Generic;
using System.Linq;
using Type = TKOM.Node.Type;

namespace TKOM.Interpreter
{
    /// <summary>
    /// Collection, that keeps track of the functions so that it cannot contain two functions called in the same way.
    /// </summary>
    internal class FunctionsCollection
    {
        private readonly HashSet<Function> functions = new();

        /// <summary>
        /// Adds the <paramref name="function"/> to the collection if its call will not be ambiguous between any other in the collection.
        /// Throws an exception if the the operation is not possible.
        /// </summary>
        /// <param name="function"></param>
        /// <exception cref="InvalidOperationException">Collection already contains a function ambiguous with <paramref name="function"/>.</exception>
        public void Add(Function function)
        {
            var funs = functions.ToList();
            for (int i = 0; i < functions.Count; i++)
            {
                var paramTypes = funs[i].Parameters.Select(p => p.Type).ToList();
                if (function.CanBeCalledLike(funs[i].Name, paramTypes))
                    throw new InvalidOperationException($"{nameof(FunctionsCollection)} already contains a function ambiguous with {function.Name}");
            }

            functions.Add(function);
        }
        /// <summary>
        /// Adds the <paramref name="function"/> to the collection if its call will not be ambiguous between any other in the collection.
        /// </summary>
        /// <param name="function"></param>
        /// <returns>Information of the <paramref name="function"/> was added successfully.</returns>
        public bool TryAdd(Function function)
        {
            var funs = functions.ToList();
            for (int i = 0; i < functions.Count; i++)
            {
                var paramTypes = funs[i].Parameters.Select(p => p.Type).ToList();
                if (function.CanBeCalledLike(funs[i].Name, paramTypes))
                    return false;
            }

            functions.Add(function);
            return true;
        }
        /// <summary>
        /// Return <c>true</c> and sets <paramref name="function"/> accordingly if the collection contains
        /// a function with given <paramref name="name"/> and parameters of types <paramref name="types"/>.<br></br>
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
