﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fixie.Listeners
{
    public class TeamCityListener : Listener
    {
        public void AssemblyStarted(Assembly assembly)
        {
            Message("testSuiteStarted name='{0}'", assembly.FileName());
        }

        public void CasePassed(PassResult result)
        {
            var @case = result.Case;

            Message("testStarted name='{0}'", @case.Name);
            Output(@case, result.Output);
            Message("testFinished name='{0}' duration='{1}'", @case.Name, DurationInMilliseconds(result.Duration));
        }

        public void CaseFailed(FailResult result)
        {
            var @case = result.Case;
            var exceptions = result.Exceptions;

            Message("testStarted name='{0}'", @case.Name);
            Output(@case, result.Output);
            Message("testFailed name='{0}' message='{1}' details='{2}'", @case.Name, PrimaryMessage(exceptions), CompoundStackTrace(exceptions));
            Message("testFinished name='{0}' duration='{1}'", @case.Name, DurationInMilliseconds(result.Duration));
        }

        public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
            Message("testSuiteFinished name='{0}'", assembly.FileName());
        }

        static void Message(string format, params string[] args)
        {
            var encodedArgs = args.Select(Encode).Cast<object>().ToArray();
            Console.WriteLine("##teamcity["+format+"]", encodedArgs);
        }

        static void Output(Case @case, string output)
        {
            if (!String.IsNullOrEmpty(output))
                Message("testStdOut name='{0}' out='{1}'", @case.Name, output);
        }

        static string Encode(string value)
        {
            var builder = new StringBuilder();
            
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '|': builder.Append("||"); break;
                    case '\'': builder.Append("|'"); break;
                    case '[': builder.Append("|["); break;
                    case ']': builder.Append("|]"); break;
                    case '\n': builder.Append("|n"); break; // Line Feed
                    case '\r': builder.Append("|r"); break; // Carriage Return
                    case '\u0085': builder.Append("|x"); break;  // Next Line
                    case '\u2028': builder.Append("|l"); break;  // Line Separator
                    case '\u2029': builder.Append("|p"); break;  // Paragraph Separator
                    default: builder.Append(ch); break;
                }
            }

            return builder.ToString();
        }

        static string DurationInMilliseconds(TimeSpan duration)
        {
            return ((int)Math.Ceiling(duration.TotalMilliseconds)).ToString();
        }

        static string CompoundStackTrace(IEnumerable<Exception> exceptions)
        {
            using (var writer = new StringWriter())
            {
                writer.WriteCompoundStackTrace(exceptions);
                return writer.ToString();
            }
        }

        static string PrimaryMessage(IEnumerable<Exception> exceptions)
        {
            return exceptions.First().Message;
        }
    }
}