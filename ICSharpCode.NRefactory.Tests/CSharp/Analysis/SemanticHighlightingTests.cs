//
// SemanticHighlightingTests.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Editor;

namespace ICSharpCode.NRefactory.CSharp.Analysis
{
	[TestFixture]
	public class SemanticHighlightingTests : SemanticHighlightingVisitor<FieldInfo>
	{
		static void SetupColors(object o)
		{
			var fields = typeof(SemanticHighlightingVisitor<FieldInfo>).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var field in fields) {
				if (field.FieldType == typeof(FieldInfo))
					field.SetValue(o, field);
			}
		}
		protected override void Colorize(TextLocation start, TextLocation end, FieldInfo color)
		{
			throw new NotImplementedException ();
		}

		[SetUp]
		public void Setup ()
		{
			SetupColors (this);
		}

		class TestSemanticHighlightingVisitor : SemanticHighlightingVisitor<FieldInfo>
		{

			public TestSemanticHighlightingVisitor(CSharpAstResolver resolver)
			{
				base.resolver = resolver;
				this.regionStart = new TextLocation (1, 1);
				this.regionEnd = new TextLocation (int.MaxValue, int.MaxValue);
				SetupColors(this);
			}

			List<Tuple<DomRegion, string>> colors = new List<Tuple<DomRegion, string>> ();

			protected override void Colorize(TextLocation start, TextLocation end, FieldInfo color)
			{
				colors.Add (Tuple.Create (new DomRegion (start, end), color != null ? color.Name : null));
			}

			public string GetColor(TextLocation loc)
			{
				foreach (var color in colors) {
					if (color.Item1.IsInside (loc))
						return color.Item2;
				}
				return null;
			}
		}

		static TestSemanticHighlightingVisitor CreateHighighting (string text)
		{
			var syntaxTree = SyntaxTree.Parse (text, "a.cs");
			if (syntaxTree.Errors.Count > 0) {
				Console.WriteLine (text);
				Assert.Fail ("parse error.");
			}
			var project = new CSharpProjectContent().AddAssemblyReferences(new [] { CecilLoaderTests.Mscorlib, CecilLoaderTests.SystemCore });
			var file = syntaxTree.ToTypeSystem();
			project = project.AddOrUpdateFiles(file);

			var resolver = new CSharpAstResolver(project.CreateCompilation(), syntaxTree, file);
			var result = new TestSemanticHighlightingVisitor (resolver);
			syntaxTree.AcceptVisitor (result);
			return result;
		}

		void TestColor(string text, FieldInfo keywordColor)
		{
			var sb = new StringBuilder ();
			var offsets = new List<int> ();
			foreach (var ch in text) {
				if (ch == '$') {
					offsets.Add (sb.Length);
					continue;
				}
				sb.Append (ch);
			}
			var visitor = CreateHighighting (sb.ToString ());
			var doc = new ReadOnlyDocument (sb.ToString ());

			foreach (var offset in offsets) {
				var loc = doc.GetLocation (offset);
				var color = visitor.GetColor (loc);
				Assert.AreEqual (keywordColor.Name, color, "Color at " + loc + " is wrong:" + color);
			}
		}

		[Test]
		public void TestClassDeclaration()
		{
			TestColor (@"class $Class { }", referenceTypeColor);
		}

		[Test]
		public void TestStructDeclaration()
		{
			TestColor (@"struct $Class { }", valueTypeColor);
		}
		
		[Test]
		public void TestInterfaceDeclaration()
		{
			TestColor (@"interface $Class { }", interfaceTypeColor);
		}
		
		[Test]
		public void TestEnumDeclaration()
		{
			TestColor (@"enum $Class { }", enumerationTypeColor);
		}

		[Test]
		public void TestDelegateDeclaration()
		{
			TestColor (@"delegate void $Class();", delegateTypeColor);
		}

		[Test]
		public void TestTypeParameter()
		{
			TestColor (@"class Class<$T> where $T : class { void Foo<$Tx> () where $Tx : $T {} } ", typeParameterTypeColor);
		}

		[Test]
		public void TestMethodDeclaration()
		{
			TestColor (@"class Class { void $Foo () {} }", methodDeclarationColor);
		}
		
		[Test]
		public void TestMethodCall()
		{
			TestColor (@"class Class { void Foo () { $Foo (); } }", methodCallColor);
		}

		[Test]
		public void TestFieldDeclaration()
		{
			TestColor (@"class Class { int $foo; }", fieldDeclarationColor);
		}

		[Test]
		public void TestFieldAccess()
		{
			TestColor (@"using System; class Class {  int Bar; void Foo () { Console.WriteLine ($Bar); } }", fieldAccessColor);
		}

		[Test]
		public void TestPropertyDeclaration()
		{
			TestColor (@"class Class { int $Foo { get; set; } }", propertyDeclarationColor);
		}

		[Test]
		public void TestPropertyAccess()
		{
			TestColor (@"using System; class Class { int Bar {get; set; } void Foo () { Console.WriteLine ($Bar); } }", propertyAccessColor);
		}

		[Test]
		public void TestEventDeclaration()
		{
			TestColor (@"class Class { event System.EventHandler $Foo, $Bar; }", eventDeclarationColor);
		}

		[Test]
		public void TestCustomEventDeclaration()
		{
			TestColor (@"class Class { event System.EventHandler $Foo { add {} remove {} } }", eventDeclarationColor);
		}

		
		[Test]
		public void TestEventAccess()
		{
			TestColor (@"class Class {  event System.EventHandler Bar; void Foo () { $Bar += (o, s) => {}; } }", eventAccessColor);
		}

		[Test]
		public void TestMethodParameterDeclaration()
		{
			TestColor (@"class Class { void Foo (int $a, string $b) {} }", parameterDeclarationColor);
		}
		[Test]
		public void TestMethodParameterAccess()
		{
			TestColor (@"class Class { void Foo (int a, string b) { int c = $a + $b;} }", parameterAccessColor);
		}

		[Test]
		public void TestIndexerParameterDeclaration()
		{
			TestColor (@"class Class { int this[int $a, string $b] { get { } } }", parameterDeclarationColor);
		}

		[Test]
		public void TestIndexerParameterAccess()
		{
			TestColor (@"class Class { int this[int a, string b] { get { return $a + $b; } } }", parameterAccessColor);
		}

		[Test]
		public void TestVariableDeclaration()
		{
			TestColor (@"class Class { void Test () { int $bar, $foo; } }", variableDeclarationColor);
		}

		[Test]
		public void TestVariableAccess()
		{
			TestColor (@"class Class { void Test () { int bar, foo; int c = $foo + $bar; } }", variableAccessColor);
		}

		[Test]
		public void TestValueKeywordInPropertySetter()
		{
			TestColor (@"class Class { int Property { get {} set { test = $value; } } }", valueKeywordColor);
		}

		[Test]
		public void TestValueKeywordInPropertyGetter()
		{
			TestColor (@"class Class { int value; int Property { get { return $value; } set { } } }", fieldAccessColor);
		}

		[Test]
		public void TestValueKeywordInCustomEvent()
		{
			TestColor (@"using System;
class Class {
	public event EventHandler Property { 
		add { Console.WriteLine ($value); } 
		remove { Console.WriteLine ($value); }
	}
}", valueKeywordColor);
		}
	
		[Test]
		public void TestExternAliasKeyword()
		{
			TestColor (@"extern $alias FooBar;", externAliasKeywordColor);
		}

		/// <summary>
		/// Bug 9539 - Semantic highlighting does not detect invalid type
		/// </summary>
		[Test]
		public void TestBug9539()
		{
			string code =@"class C<T>
		{
			public class N<U>
			{
			}
		}
		
		class A
		{
			public static int Main ()
			{
				var a = typeof (C<>.$N<,>); // The type is not red even if this linedoes not compile
				return 0;
			}
		}
";
			TestColor (code, syntaxErrorColor);
		}

		[Test]
		public void TestNullTypeError()
		{
			string code =@"
		class A
		{
			public static void Main ()
			{
				$var a = null;
			}
		}
";
			TestColor (code, syntaxErrorColor);
		}




	}
}

