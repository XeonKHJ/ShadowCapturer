﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowDriver.DriverCommunicator
{
    internal class ShadowStatus
    {
        internal static void HandleException(Exception exception)
        {
            throw new ShadowFilterException(exception);
        }
    }
}
