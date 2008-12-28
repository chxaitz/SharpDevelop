﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using ICSharpCode.PythonBinding;
using NUnit.Framework;

namespace PythonBinding.Tests.Converter
{
	/// <summary>
	/// Converts a C# try-catch-finally to python.
	/// </summary>
	[TestFixture]
	public class TryCatchFinallyConversionTestFixture
	{
		string csharp = "class Loader\r\n" +
						"{\r\n" +
						"\tpublic void load(string xml)\r\n" +
						"\t{\r\n" +
						"\t\ttry {\r\n" +
						"\t\t\tXmlDocument doc = new XmlDocument();\r\n" +
						"\t\t\tdoc.LoadXml(xml);\r\n" +
						"\t\t} catch (XmlException ex) {\r\n" +
						"\t\t\tConsole.WriteLine(ex.ToString());\r\n" +
						"\t\t} finally {\r\n" +
						"\t\t\tConsole.WriteLine(xml);\r\n" +
						"\t\t}\r\n" +
						"\t}\r\n" +
						"}";
		
		/// <summary>
		/// Note that Python seems to need to nest the try-catch
		/// inside a try-finally if the finally exists.
		/// </summary>
		[Test]
		public void ConvertedCode()
		{
			string expectedPython = "class Loader(object):\r\n" +
									"\tdef load(self, xml):\r\n" +
									"\t\ttry:\r\n" +
									"\t\t\tdoc = XmlDocument()\r\n" +
									"\t\t\tdoc.LoadXml(xml)\r\n" +
									"\t\texcept XmlException, ex:\r\n" +
									"\t\t\tConsole.WriteLine(ex.ToString())\r\n" +
									"\t\tfinally:\r\n" +
									"\t\t\tConsole.WriteLine(xml)";
			
			CSharpToPythonConverter converter = new CSharpToPythonConverter();
			string python = converter.Convert(csharp);
		
			Assert.AreEqual(expectedPython, python);
		}
	}
}
