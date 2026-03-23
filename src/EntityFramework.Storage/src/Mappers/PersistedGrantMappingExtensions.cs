namespace IdentityServer4.EntityFramework.Mappers;

/// <summary>
/// Mapping extension methods for PersistedGrant objects
/// </summary>
public static class PersistedGrantMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.PersistedGrant"/>
    /// </summary>
    /// <param name="persistedGrantEntity">The entity.</param>
    extension(Entities.PersistedGrant persistedGrantEntity)
    {
        /// <summary>
        /// Maps an entity to a model.
        /// </summary>
        /// <returns></returns>
        public Models.PersistedGrant ToModel()
        {
            return new Models.PersistedGrant
            {
                Key = persistedGrantEntity.Key,
                Type = persistedGrantEntity.Type,
                SubjectId = persistedGrantEntity.SubjectId,
                SessionId = persistedGrantEntity.SessionId,
                ClientId = persistedGrantEntity.ClientId,
                Description = persistedGrantEntity.Description,
                CreationTime = persistedGrantEntity.CreationTime,
                Expiration = persistedGrantEntity.Expiration,
                ConsumedTime = persistedGrantEntity.ConsumedTime,
                Data = persistedGrantEntity.Data,
            };
        }
    }
    
    /// <summary>
    /// Mapping extension methods for <see cref="Models.PersistedGrant"/>
    /// </summary>
    /// <param name="persistedGrantModel">The model.</param>
    extension(Models.PersistedGrant persistedGrantModel)
    {
        /// <summary>
        /// Maps a model to an entity.
        /// </summary>
        public Entities.PersistedGrant ToEntity()
        {
            return new Entities.PersistedGrant
            {
                Key = persistedGrantModel.Key,
                Type = persistedGrantModel.Type,
                SubjectId = persistedGrantModel.SubjectId,
                SessionId = persistedGrantModel.SessionId,
                ClientId = persistedGrantModel.ClientId,
                Description = persistedGrantModel.Description,
                CreationTime = persistedGrantModel.CreationTime,
                Expiration = persistedGrantModel.Expiration,
                ConsumedTime = persistedGrantModel.ConsumedTime,
                Data = persistedGrantModel.Data,
            };
        }

        /// <summary>
        /// Updates an entity from a model.
        /// </summary>
        /// <param name="existingEntity">The entity.</param>
        public void UpdateEntity(Entities.PersistedGrant existingEntity)
        {
            existingEntity.Key = persistedGrantModel.Key;
            existingEntity.Type = persistedGrantModel.Type;
            existingEntity.SubjectId = persistedGrantModel.SubjectId;
            existingEntity.SessionId = persistedGrantModel.SessionId;
            existingEntity.Description = persistedGrantModel.Description;
            existingEntity.CreationTime = persistedGrantModel.CreationTime;
            existingEntity.Expiration = persistedGrantModel.Expiration;
            existingEntity.ConsumedTime = persistedGrantModel.ConsumedTime;
            existingEntity.Data = persistedGrantModel.Data;
        }
    }
}