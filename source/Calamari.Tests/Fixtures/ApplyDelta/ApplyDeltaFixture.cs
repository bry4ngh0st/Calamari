﻿using System;
using System.IO;
using Calamari.Integration.FileSystem;
using Calamari.Tests.Fixtures.Deployment.Packages;
using Calamari.Tests.Helpers;
using NUnit.Framework;
using Octodiff.Core;

namespace Calamari.Tests.Fixtures.ApplyDelta
{
    [TestFixture]
    public class ApplyDeltaFixture : CalamariFixture
    {
        readonly string downloadPath = Path.Combine(GetPackageDownloadFolder("ApplyDelta"), "Files");
        readonly string newFileName = "Acme.Web.1.0.0.1.nupkg";

        CalamariResult ApplyDelta(string basisFile, string fileHash, string deltaFile, string newFile)
        {
            return Invoke(Calamari()
                .Action("apply-delta")
                .Argument("basisFileName", basisFile)
                .Argument("fileHash", fileHash)
                .Argument("deltaFileName", deltaFile)
                .Argument("newFileName", newFile));
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Environment.SetEnvironmentVariable("TentacleHome", GetPackageDownloadFolder("ApplyDelta"));
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Environment.SetEnvironmentVariable("TentacleHome", null);
        }

        [SetUp]
        public void SetUp()
        {
            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(downloadPath))
                Directory.Delete(downloadPath, true);
        }

        [Test]
        public void ShouldFailWhenNoBasisFileIsSpecified()
        {
            var result = ApplyDelta("", "Hash", "Delta", "New");

            result.AssertNonZero();
            result.AssertErrorOutput("No basis file was specified. Please pass --basisFileName MyPackage.1.0.0.0.nupkg");
        }

        [Test]
        public void ShouldFailWhenNoFileHashIsSpecified()
        {
            var result = ApplyDelta("Basis", "", "Delta", "New");

            result.AssertNonZero();
            result.AssertErrorOutput("No file hash was specified. Please pass --fileHash MyFileHash");
        }
        [Test]
        public void ShouldFailWhenNoDeltaFileIsSpecified()
        {
            var result = ApplyDelta("Basis", "Hash", "", "New");

            result.AssertNonZero();
            result.AssertErrorOutput("No delta file was specified. Please pass --deltaFileName MyPackage.1.0.0.0_to_1.0.0.1.octodelta");
        }

        [Test]
        public void ShouldFailWhenNoNewFileIsSpecified()
        {
            var result = ApplyDelta("Basis", "Hash", "Delta", "");

            result.AssertNonZero();
            result.AssertErrorOutput("No new file name was specified. Please pass --newFileName MyPackage.1.0.0.1.nupkg");
        }

        [Test]
        public void ShouldFailWhenBasisFileCannotBeFound()
        {
            var basisFile = Path.Combine(downloadPath, "MyPackage.1.0.0.0.nupkg");
            var result = ApplyDelta(basisFile, "Hash", "Delta", "New");

            result.AssertNonZero();
            result.AssertErrorOutput("Could not find basis file: " + basisFile);
        }

        [Test]
        public void ShouldFailWhenDeltaFileCannotBeFound()
        {
            using (var basisFile = new TemporaryFile(PackageBuilder.BuildSamplePackage("Acme.Web", "1.0.0.0")))
            {
                var deltaFilePath = Path.Combine(downloadPath,
                    "Acme.Web.1.0.0.0_to_1.0.0.1.octodelta");
                var result = ApplyDelta(basisFile.FilePath, basisFile.Hash, deltaFilePath, "New");

                result.AssertNonZero();
                result.AssertErrorOutput("Could not find delta file: " + deltaFilePath);
            }
        }

        [Test]
        public void ShouldFailWhenBasisFileHashDoesNotMatchSpecifiedFileHash()
        {
            var otherBasisFileHash = "2e9407c9eae20ffa94bf050283f9b4292a48504c";
            using (var basisFile = new TemporaryFile(PackageBuilder.BuildSamplePackage("Acme.Web", "1.0.0.0")))
            {
                var deltaFilePath = Path.Combine(downloadPath,
                    "Acme.Web.1.0.0.0_to_1.0.0.1.octodelta");
                using (var deltaFile = File.CreateText(deltaFilePath))
                {
                    deltaFile.WriteLine("This is a delta file!");
                    deltaFile.Flush();
                }

                var result = ApplyDelta(basisFile.FilePath, otherBasisFileHash, deltaFilePath, newFileName);

                result.AssertNonZero();
                result.AssertErrorOutput("Basis file hash {0} does not match the file hash specified {1}", basisFile.Hash, otherBasisFileHash);
            }
        }

        [Test]
        public void ShouldFailWhenDeltaFileIsInvalid()
        {
            using (var basisFile = new TemporaryFile(PackageBuilder.BuildSamplePackage("Acme.Web", "1.0.0.0")))
            {
                var deltaFilePath = Path.Combine(downloadPath,
                    "Acme.Web.1.0.0.0_to_1.0.0.1.octodelta");
                using (var deltaFile = File.CreateText(deltaFilePath))
                {
                    deltaFile.WriteLine("This is a delta file!");
                    deltaFile.Flush();
                }

                var result = ApplyDelta(basisFile.FilePath, basisFile.Hash, deltaFilePath, newFileName);

                result.AssertNonZero();
                result.AssertOutput("Applying delta to {0} with hash {1} and storing as {2}",
                    basisFile.FilePath,
                    basisFile.Hash,
                    Path.Combine(downloadPath, newFileName));
                result.AssertErrorOutput("The delta file appears to be corrupt.");
            }
        }

        [Test]
        public void ShouldApplyDeltaToPreviousPackageToCreateNewPackage()
        {
            var signatureBuilder = new SignatureBuilder();
            var deltaBuilder = new DeltaBuilder();

            using (var basisFile = new TemporaryFile(PackageBuilder.BuildSamplePackage("Acme.Web", "1.0.0.0")))
            using (var signatureFile = new TemporaryFile(basisFile.FilePath + ".octosig"))
            {
                using (var basisFileStream = File.OpenRead(basisFile.FilePath))
                using (var signatureFileStream = File.OpenWrite(signatureFile.FilePath))
                {
                    signatureBuilder.Build(basisFileStream, new SignatureWriter(signatureFileStream));
                    Assert.That(File.Exists(signatureFile.FilePath));
                }

                using (var newFile = new TemporaryFile(PackageBuilder.BuildSamplePackage("Acme.Web", "1.0.0.1", true)))
                using (var deltaFile = new TemporaryFile(basisFile.FilePath +
                        "_to_" + newFileName + ".octodelta"))
                {
                    using (var newFileStream = File.OpenRead(newFile.FilePath))
                    using (var signatureFileStream = File.OpenRead(signatureFile.FilePath))
                    using (var deltaFileStream = File.Open(deltaFile.FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                    {
                        deltaBuilder.BuildDelta(newFileStream,
                            new SignatureReader(signatureFileStream, deltaBuilder.ProgressReporter),
                            new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaFileStream)));
                    }
                    var result = ApplyDelta(basisFile.FilePath, basisFile.Hash, deltaFile.FilePath, newFileName);
                    result.AssertZero();
                    result.AssertOutput("Applying delta to {0} with hash {1} and storing as {2}", basisFile.FilePath,
                        basisFile.Hash, Path.Combine(downloadPath, newFileName));
                    result.AssertOutput("##octopus[deltaVerification");
                }
            }
        }
    }
}
