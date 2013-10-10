#region Licence...

//-----------------------------------------------------------------------------
// Date:	25/10/10
// Module:	Exceptions.cs
// Classes:	Exceptions
//
//
// This module contains the definition of the CS-Script exceptions
//
// Written by Oleg Shilo (oshilo@gmail.com)
// Copyright (c) 2004-2012. All rights reserved.
//
// Redistribution and use of this code WITHOUT MODIFICATIONS are permitted provided that
// the following conditions are met:
// 1. Redistributions must retain the above copyright notice, this list of conditions
//  and the following disclaimer.
// 2. Neither the name of an author nor the names of the contributors may be used
//	to endorse or promote products derived from this software without specific
//	prior written permission.
//
// Redistribution and use of this code WITH MODIFICATIONS are permitted provided that all
// above conditions are met and software is not used or sold for profit.
//
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//	Caution: Bugs are expected!
//----------------------------------------------

#endregion Licence...

using System;
using System.CodeDom.Compiler;

#if net1
using System.Collections;
#else

#endif

using System.Text;
using System.Runtime.Serialization;

namespace csscript
{
    internal class Surrogate86ProcessRequiredException : ApplicationException
    {
    }

    internal class SurrogateHostProcessRequiredException : ApplicationException
    {
        public SurrogateHostProcessRequiredException(string scriptAssembly, string[] scriptArgs, bool startDebugger)
        {
            ScriptAssembly = scriptAssembly;
            StartDebugger = startDebugger;
            ScriptArgs = scriptArgs;
        }

        private string scriptAssembly;

        public string ScriptAssembly
        {
            get { return scriptAssembly; }
            set { scriptAssembly = value; }
        }

        private bool startDebugger;

        public bool StartDebugger
        {
            get { return startDebugger; }
            set { startDebugger = value; }
        }

        private string[] scriptArgs;

        public string[] ScriptArgs
        {
            get { return scriptArgs; }
            set { scriptArgs = value; }
        }
    }

    /// <summary>
    /// The exception that is thrown when a the script compiler error occurs.
    /// </summary>
    [Serializable]
    public class CompilerException : ApplicationException
    {
        ///// <summary>
        ///// Gets or sets the errors.
        ///// </summary>
        ///// <value>The errors.</value>
        //public CompilerErrorCollection Errors { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerException"/> class.
        /// </summary>
        public CompilerException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        public CompilerException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CompilerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the CompilerException instance from the specified compiler errors errors.
        /// </summary>
        /// <param name="Errors">The compiler errors.</param>
        /// <param name="hideCompilerWarnings">if set to <c>true</c> hide compiler warnings.</param>
        /// <returns></returns>
        public static CompilerException Create(CompilerErrorCollection Errors, bool hideCompilerWarnings)
        {
            StringBuilder compileErr = new StringBuilder();
            foreach (CompilerError err in Errors)
            {
                if (err.IsWarning && hideCompilerWarnings)
                    continue;

                //compileErr.Append(err.ToString());
                compileErr.Append(err.FileName);
                compileErr.Append("(");
                compileErr.Append(err.Line);
                compileErr.Append(",");
                compileErr.Append(err.Column);
                compileErr.Append("): ");
                if (err.IsWarning)
                    compileErr.Append("warning ");
                else
                    compileErr.Append("error ");
                compileErr.Append(err.ErrorNumber);
                compileErr.Append(": ");
                compileErr.Append(err.ErrorText);
                compileErr.Append(Environment.NewLine);
            }
            CompilerException retval = new CompilerException(compileErr.ToString());
            retval.Data.Add("Errors", Errors);
            return retval;
        }
    }
}