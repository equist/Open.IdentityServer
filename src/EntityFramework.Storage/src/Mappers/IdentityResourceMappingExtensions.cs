using System.Linq;

namespace IdentityServer4.EntityFramework.Mappers;

/// <summary>
/// Class containing mapping extensions for IdentityResource classes.
/// </summary>
public static class IdentityResourceMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.IdentityResource"/>
    /// </summary>
    /// <param name="identityResourceEntity"></param>
    extension(Entities.IdentityResource identityResourceEntity)
    {
        public Models.IdentityResource ToModel()
        {
            return new Models.IdentityResource
            {
                Enabled = identityResourceEntity.Enabled,
                Name = identityResourceEntity.Name,
                DisplayName = identityResourceEntity.DisplayName,
                Description = identityResourceEntity.Description,
                ShowInDiscoveryDocument = identityResourceEntity.ShowInDiscoveryDocument,
                UserClaims = identityResourceEntity.UserClaims.ToModel(),
                Properties = identityResourceEntity.Properties.ToModelDictionary(),
                Required = identityResourceEntity.Required,
                Emphasize = identityResourceEntity.Emphasize,
            };
        }
    }

    /// <summary>
    /// Mapping extension methods for <see cref="Models.IdentityResource"/>
    /// </summary>
    /// <param name="identityResourceModel"></param>
    extension(Models.IdentityResource identityResourceModel)
    {
        public Entities.IdentityResource ToEntity()
        {
            return new Entities.IdentityResource
            {
                Enabled = identityResourceModel.Enabled,
                Name = identityResourceModel.Name,
                DisplayName = identityResourceModel.DisplayName,
                Description = identityResourceModel.Description,
                ShowInDiscoveryDocument = identityResourceModel.ShowInDiscoveryDocument,
                UserClaims = identityResourceModel.UserClaims.ToEntity<Entities.IdentityResourceClaim>(),
                Properties = identityResourceModel.Properties.ToEntityList<Entities.IdentityResourceProperty>(),
                Required = identityResourceModel.Required,
                Emphasize = identityResourceModel.Emphasize,
            };
        }
    }
}