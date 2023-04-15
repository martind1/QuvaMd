using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement;
using Quva.Model.Dtos.RootManagement.Plants;
using Quva.Model.Dtos.Shared;
using System.Reflection;

namespace Quva.Services.Mapping
{
    public static class MapsterConfiguration
    {
        const int MAXDEPTH = 3;

        public static void AddMapster(this IServiceCollection services)
        {
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            Assembly applicationAssembly = typeof(BaseDto<,>).Assembly;
            typeAdapterConfig.Scan(applicationAssembly);

            TypeAdapterConfig<Device, DeviceDto>.NewConfig().MaxDepth(MAXDEPTH);
            TypeAdapterConfig<DeviceParameter, DeviceParameterDto>.NewConfig().MaxDepth(MAXDEPTH);
            TypeAdapterConfig<Location, LocationDto>.NewConfig().MaxDepth(MAXDEPTH);
            TypeAdapterConfig<MappingWorkplaceDevice, MappingWorkplaceDeviceDto>.NewConfig().MaxDepth(MAXDEPTH);

            services.AddSingleton(typeAdapterConfig);
        }
    }
}
