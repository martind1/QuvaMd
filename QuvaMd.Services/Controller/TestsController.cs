using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quva.Services.Interfaces.Loading;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Interfaces.WeighingOperation;
using Quva.Services.Loading.Helper;
using Serilog;
using System.Text.Json;

namespace QuvaMd.Services.Controller;

[Route("api/[controller]")]
[ApiController]
//[Authorize] //ToDo wieder einkommentieren
public class TestsController : ControllerBase
{
    private readonly IOdcAdjustmentQuantityService _odcAdjustmentQuantityService;
    private readonly IDeliveryBasetypeService _deliveryBasetypeService;
    private readonly IDeliveryHeadService _deliveryHeadService;
    private readonly ILoadOrderService _loadOrderService;
    //private readonly ILogger _log;

    public TestsController(IOdcAdjustmentQuantityService odcAdjustmentQuantityService, IDeliveryBasetypeService deliveryBasetypeService, IDeliveryHeadService deliveryHeadService, ILoadOrderService loadOrderService)
    {
        //_log = Log.ForContext(GetType());
        _odcAdjustmentQuantityService = odcAdjustmentQuantityService;
        _deliveryBasetypeService = deliveryBasetypeService;
        _deliveryHeadService = deliveryHeadService;
        _loadOrderService = loadOrderService;
    }

    [HttpGet("basetypesByDelivery")]
    public async Task<IActionResult> GetBasetypesByDelivery(long idDeliveryHead)
    {
        try
        {
            var rows = await _deliveryBasetypeService.GetListByDelivery(idDeliveryHead);
            var list = DeliveryBasetypeView.FromDeliveryBasetype(rows).Items;

            if (list.Count > 0)
            {
                return Ok(list);
            }
            return NotFound("No basetypes found");
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("updateOdcQuantities")]
    public async Task<IActionResult> UpdateOdcQuantities([FromQuery] long IdLocation)
    {
        try
        {
            await _odcAdjustmentQuantityService.UpdateQuantities(IdLocation);
            return Ok("Update Odc OK");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("createDeliveryFromOrderNumber")]
    public async Task<IResult> CreateDeliveryFromOrderNumber([FromQuery] long orderNumber, [FromQuery] long? vehicleId, [FromQuery] string user)
    {
        return await _deliveryHeadService.CreateDeliveryFromOrderNumber(orderNumber, vehicleId, user);
    }

    [HttpPost("refreshLoadorder")]
    public async Task<IActionResult> RefreshLoadorder([FromQuery] long idLoadorder)
    {
        try
        {
            var loadingResult = await _loadOrderService.RefreshLoadorder(idLoadorder);
            return Ok(loadingResult);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}
