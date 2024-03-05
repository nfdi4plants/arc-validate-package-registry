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
        public static AVPRClient.ValidationPackage toValidationPackage(
            this AVPRIndex.Domain.ValidationPackageIndex indexedPackage,
            DateTimeOffset releaseDate
        )
        {
            return new AVPRClient.ValidationPackage
            {
                Name = indexedPackage.Metadata.Name,
                Description = indexedPackage.Metadata.Description,
                MajorVersion = indexedPackage.Metadata.MajorVersion,
                MinorVersion = indexedPackage.Metadata.MinorVersion,
                PatchVersion = indexedPackage.Metadata.PatchVersion,
                PackageContent = File.ReadAllBytes(indexedPackage.RepoPath),
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
                    .ToList()
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
                MD5 md5 = MD5.Create();
                return new AVPRClient.PackageContentHash
                {
                    Hash = Convert.ToHexString(md5.ComputeHash(File.ReadAllBytes(indexedPackage.RepoPath))),
                    PackageMajorVersion = indexedPackage.Metadata.MajorVersion,
                    PackageMinorVersion = indexedPackage.Metadata.MinorVersion,
                    PackagePatchVersion = indexedPackage.Metadata.PatchVersion
                };
            }
            else
            {
                return new AVPRClient.PackageContentHash
                {
                    Hash = indexedPackage.ContentHash,
                    PackageMajorVersion = indexedPackage.Metadata.MajorVersion,
                    PackageMinorVersion = indexedPackage.Metadata.MinorVersion,
                    PackagePatchVersion = indexedPackage.Metadata.PatchVersion
                };
            }
        }

        public static AVPRIndex.Domain.Author [] AsIndexType (
            this ICollection<Author> authors
        )
        {
            return authors
                .Select(author =>
                    new AVPRIndex.Domain.Author
                    {
                        FullName = author.FullName,
                        Email = author.Email,
                        Affiliation = author.Affiliation,
                        AffiliationLink = author.AffiliationLink
                    })
                .ToArray();
        }

        public static AVPRIndex.Domain.OntologyAnnotation[] AsIndexType(
            this ICollection<OntologyAnnotation> ontologyAnnotations
        )
        {
            return ontologyAnnotations
                .Select(tag =>
                    new AVPRIndex.Domain.OntologyAnnotation
                    {
                        Name = tag.Name,
                        TermSourceREF = tag.TermSourceREF,
                        TermAccessionNumber = tag.TermAccessionNumber
                    })
                .ToArray();
        }

        public static AVPRIndex.Domain.ValidationPackageMetadata toValidationPackageMetadata(
            this AVPRClient.ValidationPackage validationPackage
        )
        {
            return Domain.ValidationPackageMetadata.create(
                validationPackage.Name,
                validationPackage.Summary,
                validationPackage.Description,
                validationPackage.MajorVersion,
                validationPackage.MinorVersion,
                validationPackage.PatchVersion,
                Microsoft.FSharp.Core.FSharpOption<bool>.None,
                validationPackage.Authors.AsIndexType(),
                validationPackage.Tags.AsIndexType(),
                validationPackage.ReleaseNotes
            );
        }
    }
}
