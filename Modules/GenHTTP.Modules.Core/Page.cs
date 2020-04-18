﻿using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core
{

    public static class Page
    {

        public static PageProviderBuilder From(string title, string content)
        {
            return From(content).Title(title);
        }

        public static PageProviderBuilder From(string content)
        {
            return From(Data.FromString(content));
        }

        public static PageProviderBuilder From(IBuilder<IResourceProvider> content)
        {
            return From(content.Build());
        }

        public static PageProviderBuilder From(IResourceProvider content)
        {
            return new PageProviderBuilder().Content(content);
        }

    }

}
