﻿/*

Updated: 2009/10/28

2009/10/28  Andreas Nägeli        Initial version of this file.


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
using System.IO;
using System.Text.RegularExpressions;

using GenHTTP.Abstraction.Elements;

namespace GenHTTP.Abstraction.Compiling {
  
  /// <summary>
  /// Allows you to compile snippets which can be used to minimize the
  /// effort to generate websites via the document framework.
  /// </summary>
  public class SnippetCompiler {
    private string _ToParse;
    private Exception _Error;
    private DocumentType _Type;
    private Encoding _Encoding;

    #region Constructors

    /// <summary>
    /// Create a new compiler.
    /// </summary>
    /// <param name="type">The type of the related document</param>
    /// <param name="encoding">The encoding of the data</param>
    /// <param name="source">The element to compile</param>
    public SnippetCompiler(DocumentType type, Encoding encoding, Element source) {
      _ToParse = source.Serialize(type);
      _Type = type;
      _Encoding = encoding;
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// The error which occured during compilation.
    /// </summary>
    public Exception Error {
      get { return _Error; }
    }

    #endregion

    #region Compilation

    /// <summary>
    /// Compile the given element.
    /// </summary>
    /// <param name="file">The file to compile to</param>
    /// <param name="usedNamespace">The namespace to use</param>
    /// <param name="snippetName">The name of the snippet</param>
    /// <returns>true, if the operation succeeded</returns>
    public bool Compile(string file, string usedNamespace, string snippetName) {
      if (file == null || file.Length == 0) throw new ArgumentException("You need to specify a file to write to", "file");
      if (usedNamespace == null || usedNamespace.Length == 0) throw new ArgumentException("You need to specify the namespace to use", "usedNamespace");
      if (snippetName == null || snippetName.Length == 0) throw new ArgumentException("You need to specify the name of the snippet", "snippetName");
      try {
        // reset errors and open stream for writing
        _Error = null;
        StreamWriter w = new StreamWriter(file, false, _Encoding);
        try {
          // write header of the file
          w.WriteLine("/*");
          w.WriteLine("");
          w.WriteLine("  Snippet " + snippetName);
          w.WriteLine("");
          w.WriteLine("  Generated by the GenHTTP snippet compiler v0.01");
          w.WriteLine("");
          w.WriteLine("*/");
          w.WriteLine("using System;");
          w.WriteLine("using System.Text;");
          w.WriteLine("using System.Collections.Generic;");
          w.WriteLine("");
          w.WriteLine("using GenHTTP.Abstraction;");
          w.WriteLine("using GenHTTP.Abstraction.Compiling;");
          w.WriteLine("");
          w.WriteLine("namespace " + usedNamespace + " {");
          w.WriteLine("");
          // structures to store data
          List<string> staticContent = new List<string>();
          List<string[]> placeholders = new List<string[]>();
          // analyze snippet
          Regex re = new Regex(Regex.Escape("%GENHTTP PLACEHOLDER TYPE: ") + "([^ ]+)" + Regex.Escape(" NAME: ") + "([^%]+)" + Regex.Escape("%"));
          // retrieve first match
          Match match = re.Match(_ToParse);
          while (match.Success) {
            // add the text before this match (which is static)
            staticContent.Add(_ToParse.Substring(0, match.Index));
            // add the placeholder
            placeholders.Add(new string[] { match.Groups[1].Value, match.Groups[2].Value });
            // shorten the remaining text
            _ToParse = _ToParse.Substring(match.Index + match.Length);
            // get next match
            match = re.Match(_ToParse);
          }
          // add the remaining text without matches
          if (_ToParse.Length > 0) staticContent.Add(_ToParse);
          // write base class
          w.WriteLine("  internal class " + snippetName + "Base : ISnippetBase {");
          // define attributes and initialize them
          w.WriteLine("    private Encoding _Encoding;");
          w.WriteLine("    private DocumentType _Type;");
          w.WriteLine("    private List<byte[]> _Parts;");
          w.WriteLine("");
          w.WriteLine("    public " + snippetName + "Base() {");
          w.WriteLine("      _Encoding = Encoding.GetEncoding(\"" + _Encoding.WebName + "\");");
          w.WriteLine("      _Type = DocumentType." + _Type.ToString() + ";");
          w.WriteLine("      _Parts = new List<byte[]>(" + staticContent.Count + ");");
          foreach (string part in staticContent) {
            w.WriteLine("      _Parts.Add(_Encoding.GetBytes(\"" + part.Replace("\"", "\\\"").Replace("\r\n", "\\r\\n") + "\"));");
          }
          w.WriteLine("    }");
          // write get-/setters
          w.WriteLine("");
          w.WriteLine("    public byte[] this[int nr] {");
          w.WriteLine("      get { return _Parts[nr]; }");
          w.WriteLine("    }");
          w.WriteLine("");
          w.WriteLine("");
          w.WriteLine("    public Encoding Encoding {");
          w.WriteLine("      get { return _Encoding; }");
          w.WriteLine("    }");
          w.WriteLine("");
          w.WriteLine("    public DocumentType Type {");
          w.WriteLine("      get { return _Type; }");
          w.WriteLine("    }");
          w.WriteLine("");
          w.WriteLine();
          w.WriteLine("  }");
          w.WriteLine("");
          w.WriteLine("  internal class " + snippetName + " : ISnippet {");
          w.WriteLine("    private ISnippetBase _Base;");
          w.WriteLine("    private List<byte[]> _Content;");
          w.WriteLine("    private long _ContentLength;");
          // write placeholders
          foreach (string[] placeholder in placeholders.Distinct()) {
            w.WriteLine("    private " + placeholder[0] + " _" + placeholder[1] + ";");
          }
          w.WriteLine("");
          w.WriteLine("    public " + snippetName + "(ISnippetBase snippetBase) {");
          w.WriteLine("      _Content = new List<byte[]>(" + placeholders.Count + ");");
          w.WriteLine("      _Base = snippetBase;");
          w.WriteLine("    }");
          w.WriteLine("");
          w.WriteLine("    public ISnippetBase Base {");
          w.WriteLine("      get { return _Base; }");
          w.WriteLine("    }");
          w.WriteLine("");
          // write get-/setters for every placeholder
          foreach (string[] placeholder in placeholders.Distinct()) {
            w.WriteLine("    public " + placeholder[0] + " " + placeholder[1] + " {");
            w.WriteLine("      get { return _" + placeholder[1] + "; }");
            w.WriteLine("      set { _" + placeholder[1] + " = value; }");
            w.WriteLine("    }");
            w.WriteLine("");
          }
          // calculate content length
          w.WriteLine("    public long ContentLength {");
          w.WriteLine("      get {");
          w.WriteLine("        if (_ContentLength != 0) return _ContentLength;");
          w.WriteLine("        _Content.Clear();");
          foreach (string[] placeholder in placeholders) {
            Type type = Type.GetType(placeholder[0]);
            if (type.IsSubclassOf(typeof(Element))) {
              w.WriteLine("        _Content.Add(_Base.Encoding.GetBytes(_" + placeholder[1] + ".Serialize(_Base.Type)));");
            }
            else if (type == typeof(string)) {
              w.WriteLine("        _Content.Add(_Base.Encoding.GetBytes(_" + placeholder[1] + "));");
            }
            else if (type == typeof(SnippetContainer)) {
              w.WriteLine("        _Content.Add(" + placeholder[1] + ".ToByteArray());");
            }
            else {
              w.WriteLine("        _Content.Add(_Base.Encoding.GetBytes(_" + placeholder[1] + ".ToString()));");
            }
          }
          w.Write("        return _ContentLength = (" + staticContent.Sum((string s) => s.Length));
          int i = 0;
          foreach (string[] placeholder in placeholders) {
            w.Write(" + _Content[" + i++ + "].Length");
          }
          w.WriteLine(");");
          w.WriteLine("      }");
          w.WriteLine("    }");
          w.WriteLine("");
          // write serialization
          w.WriteLine("    public byte[] ToByteArray() {");          
          w.WriteLine("      byte[] ret = new byte[ContentLength];");
          w.WriteLine("      int nextPos = 0;");
          i = 0;
          // serialization main part
          foreach (string[] placeholder in placeholders) {
            w.WriteLine("      // " + placeholder[1]);
            w.WriteLine("      System.Buffer.BlockCopy(_Base[" + i + "], 0, ret, nextPos, " + staticContent[i].Length + ");");
            w.WriteLine("      nextPos += _Base[" + i + "].Length;");
            w.WriteLine("      System.Buffer.BlockCopy(_Content[" + i + "], 0, ret, nextPos, _Content[" + i + "].Length);");
            w.WriteLine("      nextPos += _Content[" + i + "].Length;");
            i++;
          }
          w.WriteLine("      System.Buffer.BlockCopy(_Base[" + i + "], 0, ret, nextPos, _Base[" + i + "].Length);");
          w.WriteLine("      return ret;");
          w.WriteLine("    }");
          w.WriteLine("");
          w.WriteLine("    public string ToString() {");
          w.WriteLine("      return _Base.Encoding.GetString(ToByteArray());");
          w.WriteLine("    }");
          w.WriteLine();
          w.WriteLine("  }");
          // write footer of the file
          w.WriteLine("");
          w.WriteLine("}");
          w.WriteLine("");
        }
        catch (Exception e) {
          // a error occured .. save it
          _Error = e;
        }
        // close file
        w.Close();
      }
      catch (Exception ex) {
        // failed to open the file
        throw new IOException("Failed to write to the given file.", ex);
      }
      // was there any error?
      return _Error != null;
    }

    #endregion

  }

}
