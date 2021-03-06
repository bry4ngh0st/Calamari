﻿using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Calamari.Tests.Helpers
{
    public class CalamariResult
    {
        private readonly int exitCode;
        private readonly CaptureCommandOutput captured;

        public CalamariResult(int exitCode, CaptureCommandOutput captured)
        {
            this.exitCode = exitCode;
            this.captured = captured;
        }

        private int ExitCode
        {
            get { return exitCode; }
        }

        public void AssertZero()
        {
            Assert.That(ExitCode, Is.EqualTo(0), "Expected command to return exit code 0");
        }

        public void AssertNonZero()
        {
            Assert.That(ExitCode, Is.Not.EqualTo(0), "Expected a non-zero exit code");
        }

        public void AssertOutput(string expectedOutputFormat, params object[] args)
        {
            AssertOutput(String.Format(expectedOutputFormat, args));
        }

        public void AssertOutputVariable(string name, IResolveConstraint resolveConstraint)
        {
            var variable = captured.OutputVariables.Get(name);
            Assert.That(variable, resolveConstraint);
        }

        public void AssertOutput(string expectedOutput)
        {
            var allOutput = string.Join(Environment.NewLine, captured.Infos);
            Assert.That(allOutput.IndexOf(expectedOutput, StringComparison.OrdinalIgnoreCase) >= 0, string.Format("Expected to find: {0}. Output:\r\n{1}", expectedOutput, allOutput));
        }

        public string GetOutputForLineContaining(string expectedOutput)
        {
            var found = captured.Infos.SingleOrDefault(i => i.IndexOf(expectedOutput, StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.IsNotNull(found);
            return found;
        }

        public void AssertErrorOutput(string expectedOutputFormat, params object[] args)
        {
            AssertErrorOutput(String.Format(expectedOutputFormat, args));
        }

        public void AssertErrorOutput(string expectedOutput)
        {
            var allOutput = string.Join(Environment.NewLine, captured.Errors);
            Assert.That(allOutput.IndexOf(expectedOutput, StringComparison.OrdinalIgnoreCase) >= 0, string.Format("Expected to find: {0}. Output:\r\n{1}", expectedOutput, allOutput));
        }
    }
}