﻿// Copyright (c) Jeff Kluge. All rights reserved.
//
// Licensed under the MIT license.

using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using Shouldly;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace Microsoft.Build.Utilities.ProjectCreation.UnitTests.PackageRepositoryTests
{
    public class LibraryTests : TestBase
    {
        [Fact]
        public void BasicLibrary()
        {
            PackageRepository.Create(TestRootPath)
                .Package("PackageA", "1.0.0", out PackageIdentity packageA)
                    .Library(FrameworkConstants.CommonFrameworks.Net45);

            VerifyAssembly(packageA, FrameworkConstants.CommonFrameworks.Net45);
        }

        [Fact]
        public void LibraryWithVersion()
        {
            const string assemblyVersion = "2.3.4.5";

            PackageRepository.Create(TestRootPath)
                .Package("PackageA", "1.0.0", out PackageIdentity packageA)
                .Library(FrameworkConstants.CommonFrameworks.Net45, assemblyVersion: assemblyVersion);

            VerifyAssembly(packageA, FrameworkConstants.CommonFrameworks.Net45, version: "2.3.4.5");
        }

        [Fact]
        public void MultipleLibrariesMultipleTargetFrameworks()
        {
            PackageRepository.Create(TestRootPath)
                .Package("PackageA", "1.0.0", out PackageIdentity packageA)
                .Library(FrameworkConstants.CommonFrameworks.Net45)
                .Library(FrameworkConstants.CommonFrameworks.NetStandard20);

            VerifyAssembly(packageA, FrameworkConstants.CommonFrameworks.Net45);
            VerifyAssembly(packageA, FrameworkConstants.CommonFrameworks.NetStandard20);
        }

        [Fact]
        public void MultipleLibrariesSameTargetFramework()
        {
            PackageRepository.Create(TestRootPath)
                .Package("PackageA", "1.0.0", out PackageIdentity packageA)
                    .Library(FrameworkConstants.CommonFrameworks.Net45, filename: null)
                    .Library(FrameworkConstants.CommonFrameworks.Net45, filename: "CustomAssembly.dll");

            VerifyAssembly(packageA, FrameworkConstants.CommonFrameworks.Net45);
            VerifyAssembly(packageA, FrameworkConstants.CommonFrameworks.Net45, assemblyFileName: "CustomAssembly.dll");
        }

        private void VerifyAssembly(PackageIdentity packageIdentity, NuGetFramework targetFramework, string assemblyFileName = null, string version = null)
        {
            DirectoryInfo packageDirectory = new DirectoryInfo(((VersionFolderPathResolver)VersionFolderPathResolver).GetInstallPath(packageIdentity.Id, packageIdentity.Version))
                .ShouldExist();

            DirectoryInfo libDirectory = new DirectoryInfo(Path.Combine(packageDirectory.FullName, "lib", targetFramework.GetShortFolderName()))
                .ShouldExist();

            FileInfo classLibrary = new FileInfo(Path.Combine(libDirectory.FullName, assemblyFileName ?? $"{packageIdentity.Id}.dll"))
                .ShouldExist();

            AssemblyName assemblyName = AssemblyName.GetAssemblyName(classLibrary.FullName);

            assemblyName.Version.ShouldBe(version == null ? new Version(1, 0, 0, 0) : Version.Parse(version));
        }
    }
}