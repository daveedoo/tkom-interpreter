using System;
using System.Text;

namespace TKOM.Scanner
{
    internal class LimitedStringBuilder
    {
        private StringBuilder builder;
        public int Length => builder.Length;
        public int Capacity { get; }

        /// <summary>
        /// Wrapper around the StringBuilder class with strict MaxCapacity.
        /// </summary>
        /// <param name="capacity">Maximum capacity of the <see cref="LimitedStringBuilder"/> instance.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public LimitedStringBuilder(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("limit");

            builder = new StringBuilder();
            Capacity = capacity;
        }

        /// <summary>
        /// Appends a string representation of character <paramref name="c"/> to this instance.
        /// </summary>
        /// <param name="c"></param>
        /// <returns><c>true</c> if appending successful. <c>false</c> otherwise.</returns>
        public bool Append(char c)
        {
            if (builder.Length >= Capacity)
                return false;
            builder.Append(c);
            return true;
        }

        public void Clear() => builder.Clear();

        public override string ToString() => builder.ToString();
    }
}
