using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core;

public class YatsdbException : ApplicationException
{
    public YatsdbException()
    {
    }

    public YatsdbException(string? message)
        : base(message)
    {
    }

    public YatsdbException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
