﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision$</version>
// </file>

using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.WixBinding;
using NUnit.Framework;
using System;

namespace WixBinding.Tests.Project
{
	/// <summary>
	/// Tests that a non-Wix project is ignored by the WixProjectNodeBuilder.
	/// </summary>
	[TestFixture]
	public class WixBuilderCannotBuildNonWixProjectTestFixture
	{
		WixProjectNodeBuilder wixNodeBuilder;
		MSBuildProject project;
		
		[TestFixtureSetUp]
		public void SetUpFixture()
		{
			wixNodeBuilder = new WixProjectNodeBuilder();
			project = new MSBuildProject();
			project.IdGuid = "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF";
			project.FileName = "test.csproj";
		}
		
		[Test]
		public void CannotBuildNonWixProject()
		{
			Assert.IsFalse(wixNodeBuilder.CanBuildProjectTree(project));
		}
	}
}
