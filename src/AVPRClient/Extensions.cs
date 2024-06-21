using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AVPRIndex;
using YamlDotNet.Core.Tokens;
using static AVPRIndex.Domain;
 
namespace AVPRClient
{
    public static class Extensions
    {
        public static bool IdentityEquals(
            this AVPRClient.ValidationPackage package,
            AVPRClient.ValidationPackage other
        )
        {
            return package.Name == other.Name
                && package.MajorVersion == other.MajorVersion
                && package.MinorVersion == other.MinorVersion
                && package.PatchVersion == other.PatchVersion
                && package.PreReleaseVersionSuffix == other.PreReleaseVersionSuffix
                && package.BuildMetadataVersionSuffix == other.BuildMetadataVersionSuffix;
        }

        public static bool IdentityEquals(
            this AVPRClient.ValidationPackage package,
            AVPRIndex.Domain.ValidationPackageIndex indexedPackage
        )
        {
            return package.Name == indexedPackage.Metadata.Name
                && package.MajorVersion == indexedPackage.Metadata.MajorVersion
                && package.MinorVersion == indexedPackage.Metadata.MinorVersion
                && package.PatchVersion == indexedPackage.Metadata.PatchVersion
                && package.PreReleaseVersionSuffix == indexedPackage.Metadata.PreReleaseVersionSuffix
                && package.BuildMetadataVersionSuffix == indexedPackage.Metadata.BuildMetadataVersionSuffix;
        }

        public static bool IdentityEquals(
            this AVPRIndex.Domain.ValidationPackageIndex indexedPackage,
            AVPRClient.ValidationPackage package
        )
        {
            return package.Name == indexedPackage.Metadata.Name
                && package.MajorVersion == indexedPackage.Metadata.MajorVersion
                && package.MinorVersion == indexedPackage.Metadata.MinorVersion
                && package.PatchVersion == indexedPackage.Metadata.PatchVersion
                && package.PreReleaseVersionSuffix == indexedPackage.Metadata.PreReleaseVersionSuffix
                && package.BuildMetadataVersionSuffix == indexedPackage.Metadata.BuildMetadataVersionSuffix;
        }

        public static AVPRClient.ValidationPackage toValidationPackage(
            this AVPRIndex.Domain.ValidationPackageIndex indexedPackage,
            DateTimeOffset releaseDate
        )
        {
            return new AVPRClient.ValidationPackage
            {
                Name = indexedPackage.Metadata.Name,
                Summary = indexedPackage.Metadata.Summary,
                Description = indexedPackage.Metadata.Description,
                MajorVersion = indexedPackage.Metadata.MajorVersion,
                MinorVersion = indexedPackage.Metadata.MinorVersion,
                PatchVersion = indexedPackage.Metadata.PatchVersion,
                PreReleaseVersionSuffix = indexedPackage.Metadata.PreReleaseVersionSuffix,
                BuildMetadataVersionSuffix = indexedPackage.Metadata.BuildMetadataVersionSuffix,
                PackageContent = BinaryContent.fromFile(indexedPackage.RepoPath),
                ReleaseDate = releaseDate,
                Tags =
                    indexedPackage.Metadata.Tags
                    .Select(tag =>
                    {
                        return new AVPRClient.OntologyAnnotation
                        {
                            Name = tag.Name,
                            TermAccessionNumber = tag.TermAccessionNumber,
                            TermSourceREF = tag.TermSourceREF
                        };
                    })
                    .ToList(),
                ReleaseNotes = indexedPackage.Metadata.ReleaseNotes,
                Authors =
                    indexedPackage.Metadata.Authors
                    .Select(author =>
                    {
                        return new AVPRClient.Author
                        {
                            FullName = author.FullName,
                            Email = author.Email,
                            Affiliation = author.Affiliation,
                            AffiliationLink = author.AffiliationLink
                        };
                    })
                    .ToList(),
                CQCHookEndpoint = indexedPackage.Metadata.CQCHookEndpoint
            };
        }

        public static AVPRClient.ValidationPackage toValidationPackage(
            this AVPRIndex.Domain.ValidationPackageIndex indexedPackage
        )
        {
            return indexedPackage.toValidationPackage(DateTimeOffset.Now);
        }

        public static AVPRClient.PackageContentHash toPackageContentHash(
            this AVPRIndex.Domain.ValidationPackageIndex indexedPackage,
            bool HashFileDirectly = false
        )
        {
            if ( HashFileDirectly)
            {
                return new AVPRClient.PackageContentHash
                {
                    PackageName = indexedPackage.Metadata.Name,
                    Hash = Hash.hashFile(indexedPackage.RepoPath),
                    PackageMajorVersion = indexedPackage.Metadata.MajorVersion,
                    PackageMinorVersion = indexedPackage.Metadata.MinorVersion,
                    PackagePatchVersion = indexedPackage.Metadata.PatchVersion,
                    PackagePreReleaseVersionSuffix = indexedPackage.Metadata.PreReleaseVersionSuffix,
                    PackageBuildMetadataVersionSuffix = indexedPackage.Metadata.BuildMetadataVersionSuffix
                };
            }
            else
            {
                return new AVPRClient.PackageContentHash
                {
                    PackageName = indexedPackage.Metadata.Name,
                    Hash = indexedPackage.ContentHash,
                    PackageMajorVersion = indexedPackage.Metadata.MajorVersion,
                    PackageMinorVersion = indexedPackage.Metadata.MinorVersion,
                    PackagePatchVersion = indexedPackage.Metadata.PatchVersion,
                    PackagePreReleaseVersionSuffix = indexedPackage.Metadata.PreReleaseVersionSuffix,
                    PackageBuildMetadataVersionSuffix = indexedPackage.Metadata.BuildMetadataVersionSuffix
                };
            }
        }

        public static AVPRIndex.Domain.Author AsIndexType(
            this Author authors
        )
        {             
            return 
                new AVPRIndex.Domain.Author
                {
                    FullName = authors.FullName,
                    Email = authors.Email,
                    Affiliation = authors.Affiliation,
                    AffiliationLink = authors.AffiliationLink
                };
        }

        public static AVPRIndex.Domain.Author [] AsIndexType (
            this ICollection<Author> authors
        )
        {
            return authors
                .Select(author => author.AsIndexType())
                .ToArray();
        }

        public static AVPRIndex.Domain.OntologyAnnotation AsIndexType(
            this OntologyAnnotation tag
        )
        {
            return new AVPRIndex.Domain.OntologyAnnotation
            {
                Name = tag.Name,
                TermSourceREF = tag.TermSourceREF,
                TermAccessionNumber = tag.TermAccessionNumber
            };
        }

        public static AVPRIndex.Domain.OntologyAnnotation[] AsIndexType(
            this ICollection<OntologyAnnotation> ontologyAnnotations
        )
        {
            return ontologyAnnotations
                .Select(tag => tag.AsIndexType())
                .ToArray();
        }

        public static AVPRIndex.Domain.ValidationPackageMetadata toValidationPackageMetadata(
            this AVPRClient.ValidationPackage validationPackage
        )
        {
            return Domain.ValidationPackageMetadata.create(
                name: validationPackage.Name,
                summary: validationPackage.Summary,
                description: validationPackage.Description,
                majorVersion: validationPackage.MajorVersion,
                minorVersion: validationPackage.MinorVersion,
                patchVersion: validationPackage.PatchVersion,
                PreReleaseVersionSuffix: validationPackage.PreReleaseVersionSuffix,
                BuildMetadataVersionSuffix: validationPackage.BuildMetadataVersionSuffix,
                Publish: Microsoft.FSharp.Core.FSharpOption<bool>.None,
                Authors: validationPackage.Authors.AsIndexType(),
                Tags: validationPackage.Tags.AsIndexType(),
                ReleaseNotes: validationPackage.ReleaseNotes,
                CQCHookEndpoint: validationPackage.CQCHookEndpoint
            );
        }
    }
}
