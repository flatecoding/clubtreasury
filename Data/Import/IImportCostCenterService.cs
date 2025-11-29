namespace TTCCashRegister.Data.Import;

public interface IImportCostCenterService
{
    Task<bool> ImportCostCentersAndPositions(Stream fileStream);
}