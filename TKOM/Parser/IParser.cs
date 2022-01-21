using TKOM.Node;

namespace TKOM.Parser
{
    public interface IParser
    {
        /// <summary>
        /// </summary>
        /// <param name="program">
        ///     Root of the program abstract syntax tree.
        /// </param>
        /// <returns>
        ///     <c>true</c> if parsing finished successfully,<br></br>
        ///     <c>false</c> in case of any syntax error.
        /// </returns>
        public bool TryParse(out Program program);
    }
}
