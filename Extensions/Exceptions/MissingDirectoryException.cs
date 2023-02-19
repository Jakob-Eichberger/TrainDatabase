using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions.Exceptions
{
    /// <summary>
    /// Exception that is thrown when a directory, that is expected to be here, is not found.
    /// </summary>
    public class MissingDirectoryException : Exception
    {
        /// <summary>
        /// Exception that is thrown when a directory, that is expected to be here, is not found.
        /// </summary>
        public MissingDirectoryException(DirectoryInfo directoryInfo) : base(message: $"Expected to find a directory at the following location: {directoryInfo}")
        {
        }

        /// <summary>
        /// Exception that is thrown when a directory, that is expected to be here, is not found.
        /// </summary>
        public MissingDirectoryException(string directoryInfo) : base(message: $"Expected to find a directory at the following location: {directoryInfo}")
        {
        }
    }
}
