﻿
using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Routing;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Modules
{
    
    /// <summary>
    /// A builder which will provide an <see cref="IRouter"/>.
    /// </summary>
    public interface IRouterBuilder : IBuilder<IRouter>
    {

    }

}
