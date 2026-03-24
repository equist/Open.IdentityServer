#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.EntityFramework.Mappers;

/// <summary>
/// Class containing mapping extensions for Secret classes.
/// </summary>
public static class SecretMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.ApiResource"/>
    /// </summary>
    /// <param name="secretList"></param>
    /// <typeparam name="Type"></typeparam>
    extension<Type>(List<Type>? secretList)
        where Type: Entities.Secret
    {
        /// <summary>
        /// Maps List of Type to a collection of <see cref="Models.Secret"/>
        /// </summary>
        /// <returns></returns>
        public ICollection<Models.Secret> ToModel() => secretList?.Select(secret =>
        {
            var entity = new Models.Secret
            {
                Description = secret.Description,
                Value = secret.Value,
                Expiration = secret.Expiration,
            };

            entity.Type = secret.Type ?? entity.Type;

            return entity;
        }).ToList() ?? [];
    }

    /// <summary>
    /// Mapping extension methods for collections of <see cref="Models.Secret"/>
    /// </summary>
    /// <param name="secretsModels"></param>
    extension(ICollection<Models.Secret>? secretsModels)
    {
        /// <summary>
        /// Maps collection to a list of Type
        /// </summary>
        /// <typeparam name="Type">class that implements <see cref="Entities.Secret"/> and has an empty ctor</typeparam>
        /// <returns></returns>
        public List<Type> ToEntity<Type>()
            where Type: Entities.Secret, new() => secretsModels?.Select(secret => new Type
        {
            Description = secret.Description,
            Value = secret.Value,
            Expiration = secret.Expiration,
            Type = secret.Type,
        }).ToList() ?? [];
    }
}