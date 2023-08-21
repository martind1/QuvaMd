using Mapster;

namespace Quva.Model.Dtos.Shared
{
    public abstract class BaseDto<TDto, TEntity> : IRegister
    where TDto : class, new()
    where TEntity : class, new()
    {

        public TEntity ToEntity()
        {
            return this.Adapt<TEntity>();
        }

        public TEntity ToEntity(TEntity entity)
        {
            return (this as TDto).Adapt(entity);
        }

        public static TDto FromEntity(TEntity entity)
        {
            return entity.Adapt<TDto>();
        }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private TypeAdapterConfig Config { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public virtual void AddCustomMappings() { }


        protected TypeAdapterSetter<TDto, TEntity> SetCustomMappings()
            => Config.ForType<TDto, TEntity>();

        protected TypeAdapterSetter<TEntity, TDto> SetCustomMappingsInverse()
            => Config.ForType<TEntity, TDto>();

        public void Register(TypeAdapterConfig config)
        {
            Config = config;
            AddCustomMappings();
        }
    }
}
