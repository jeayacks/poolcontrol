//-----------------------------------------------------------------------
// <copyright file="CommandLineParser.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Parse arguments of the command line
    /// </summary>
    public sealed class CommandLineParser
    {
        private bool hasArgs;

        private CommandLineParser()
        {
            this.Commands = new List<string>();
            this.Flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.Args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the list of commands.
        /// </summary>
        public List<string> Commands { get; }

        /// <summary>
        /// Gets the flags.
        /// </summary>
        public HashSet<string> Flags { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public Dictionary<string, string> Args { get; }

        /// <summary>
        /// Parse the arguments.
        /// </summary>
        /// <param name="args">Array of arguments.</param>
        /// <returns>Parser instance.</returns>
        public static CommandLineParser FromArguments(string[] args)
        {
            var parser = new CommandLineParser();
            parser.Parse(args);
            return parser;
        }

        /// <summary>
        /// Test if the first argument is the specified command.
        /// </summary>
        /// <param name="name">Command name.</param>
        /// <returns>True if the first command is the specified command.</returns>
        public bool IsCommand(string name)
        {
            bool result = false;
            if (this.Commands.Count > 0)
            {
                result = string.Equals(name, this.Commands[0], StringComparison.CurrentCultureIgnoreCase);
            }

            return result;
        }

        /// <summary>
        /// Parse arguments.
        /// </summary>
        /// <param name="args">Array of arguments.</param>
        private void Parse(string[] args)
        {

            string argScope = null;
            foreach (string arg in args)
            {
                this.hasArgs = this.hasArgs || arg.StartsWith("--");

                if (string.Equals(arg, "/?", StringComparison.Ordinal))
                {
                    this.Flags.Add("help");
                }
                else if (!this.hasArgs)
                {
                    this.Commands.Add(arg.Trim());
                }
                else
                {
                    // it's either an arg, an arg value or a flag
                    if (arg.StartsWith("--") && arg.Length > 2)
                    {
                        string argVal = arg.Substring(2);

                        // this means two --args in a row which means previous was a flag
                        if (argScope != null)
                        {
                            this.Flags.Add(argScope.Trim());
                        }

                        argScope = argVal;
                    }
                    else if (!arg.StartsWith("-"))
                    {
                        // we found a value - check if we're in scope of an arg
                        if (argScope != null && !this.Args.ContainsKey(argScope = argScope.Trim()))
                        {
                            // ignore duplicates - first wins - below will be val1
                            // --arg1 val1 --arg1 val1
                            this.Args.Add(argScope, arg);
                            argScope = null;
                        }
                    }
                    else
                    {
                        // ignoring the second value for an arg (val2 below) 
                        // --arg val1 val2

                        // ignoring invalid things like empty - and --
                        // --arg val1 -- --flag
                    }
                }
            }


            // handle last arg being a flag
            if (argScope != null)
            {
                this.Flags.Add(argScope);
            }
        }
    }
}
