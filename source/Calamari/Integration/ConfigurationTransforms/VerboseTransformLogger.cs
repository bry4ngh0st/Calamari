﻿using System;
using Microsoft.Web.XmlTransform;

namespace Calamari.Integration.ConfigurationTransforms
{
    public class VerboseTransformLogger : IXmlTransformationLogger
    {
        public void LogMessage(string message, params object[] messageArgs)
        {
            Log.VerboseFormat(message, messageArgs);
        }

        public void LogMessage(MessageType type, string message, params object[] messageArgs)
        {
            LogMessage(message, messageArgs);
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            Log.WarnFormat(message, messageArgs);
        }

        public void LogWarning(string file, string message, params object[] messageArgs)
        {
            Log.WarnFormat("File {0}: ", file);
            Log.WarnFormat(message, messageArgs);
        }

        public void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            Log.WarnFormat("File {0}, line {1}, position {2}: ", file, lineNumber, linePosition);
            Log.WarnFormat(message, messageArgs);
        }

        public void LogError(string message, params object[] messageArgs)
        {
            Log.ErrorFormat(message, messageArgs);
        }

        public void LogError(string file, string message, params object[] messageArgs)
        {
            Log.ErrorFormat("File {0}: ", file);
            Log.ErrorFormat(message, messageArgs);
        }

        public void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            Log.ErrorFormat("File {0}, line {1}, position {2}: ", file, lineNumber, linePosition);
            Log.ErrorFormat(message, messageArgs);
        }

        public void LogErrorFromException(Exception ex)
        {
            Log.ErrorFormat(ex.ToString());
        }

        public void LogErrorFromException(Exception ex, string file)
        {
            Log.ErrorFormat("File {0}: ", file);
            Log.ErrorFormat(ex.ToString());
        }

        public void LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition)
        {
            Log.ErrorFormat("File {0}, line {1}, position {2}: ", file, lineNumber, linePosition);
            Log.ErrorFormat(ex.ToString());
        }

        public void StartSection(string message, params object[] messageArgs)
        {
            Log.VerboseFormat(message, messageArgs);
        }

        public void StartSection(MessageType type, string message, params object[] messageArgs)
        {
            StartSection(message, messageArgs);
        }

        public void EndSection(string message, params object[] messageArgs)
        {
            Log.VerboseFormat(message, messageArgs);
        }

        public void EndSection(MessageType type, string message, params object[] messageArgs)
        {
            EndSection(message, messageArgs);
        }
    }
}