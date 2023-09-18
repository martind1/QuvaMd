namespace Quva.Services.Loading.Helper;

// Translation Code Constants:
public static class TrCode
{
    public record LoadingService
    {
        //BasetypeService:
        public const string NoSiloMaterial = "LoadingService.NoSiloMaterial";
        public const string WrongMixindex = "LoadingService.WrongMixindex";
        public const string WrongMixindex0 = "LoadingService.WrongMixindex0";
        public const string NoTrueBasicType = "LoadingService.NoTrueBasicType";
        public const string NoContingentSilos = "LoadingService.NoContingentSilos";
        public const string DeliveryNotFound = "LoadingService.DeliveryNotFound";
        public const string DebitorNotFound = "LoadingService.DebitorNotFound";
        public const string LocationNull = "LoadingService.LocationNull";
        public const string MaterialNotFound = "LoadingService.MaterialNotFound";
        public const string ContingentRequired = "LoadingService.ContingentRequired";

        //LoadOrderService:
        public const string NoLoadingPoints = "LoadingService.NoLoadingPoints";
        public const string NoMainPositions = "LoadingService.NoMainPositions";
        public const string LoadorderExists = "LoadingService.LoadorderExists";
        public const string NoSiloDelivery = "LoadingService.NoSiloDelivery";

        //ApplyRule
        public const string LockBigbag = "LoadingService.LockBigbag";
        public const string LockTruck2 = "LoadingService.LockTruck2";
        public const string LockTruck = "LoadingService.LockTruck";
        public const string LockRail = "LoadingService.LockRail";
        public const string LockLaboratory = "LoadingService.LockLaboratory";
        public const string LockProduction = "LoadingService.LockProduction";
        public const string LockForSensitiveCustomer = "LoadingService.LockForSensitiveCustomer";
        public const string CheckSilolevel = "LoadingService.CheckSilolevel";
    }

}
