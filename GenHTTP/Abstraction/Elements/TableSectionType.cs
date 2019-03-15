﻿/*

Updated: 2009/10/22

2009/10/22  Andreas Nägeli        Initial version of this file.


LICENSE: This file is part of the GenHTTP webserver.

GenHTTP is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements {

  /// <summary>
  /// Specifies the type of a table section.
  /// </summary>
  public enum TableSectionType {
    /// <summary>
    /// Header of the table.
    /// </summary>
    Head,
    /// <summary>
    /// Body of the table.
    /// </summary>
    Body,
    /// <summary>
    /// Footer of the table.
    /// </summary>
    Foot
  }

}
