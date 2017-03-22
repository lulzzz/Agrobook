﻿using System;

namespace Agrobook.Infrastructure.Log
{
    public interface ILogLite
    {
        void Error(string message);
        void Error(Exception ex, string message);
        void Fatal(string message);
        void Fatal(Exception ex, string message);
        void Info(string message);
        void Verbose(string message);
    }
}
