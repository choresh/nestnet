#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Mvc;
using NestNet.Infra.BaseClasses;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Dtos;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Services;
using GlobalE.Payments.Manager.Core.Modules.DisputeHistories.Entities;

namespace GlobalE.Payments.Manager.Api.Modules.DisputeHistories.Controllers
{
    [Route("api/dispute-histories")]
    public class DisputeHistoriesController : CrudControllerBase<DisputeHistoryEntity, DisputeHistoryCreateDto, DisputeHistoryUpdateDto, DisputeHistoryResultDto, DisputeHistoryQueryDto>
    {
        public DisputeHistoriesController(IDisputeHistoriesService disputeHistoriesService)
            : base(disputeHistoriesService, "DisputeHistoryId")
        {
        }

        [HttpGet]
        public override async Task<ActionResult<IEnumerable<DisputeHistoryResultDto>>> GetAll()
        {
            return await base.GetAll();
        }

        [HttpGet("{disputeHistoryId}")]
        public override async Task<ActionResult<DisputeHistoryResultDto>> GetById(long disputeHistoryId)
        {
            return await base.GetById(disputeHistoryId);
        }

        [HttpPost]
        public override async Task<ActionResult<DisputeHistoryResultDto>> Create(DisputeHistoryCreateDto disputeHistory)
        {
            return await base.Create(disputeHistory);
        }

        [HttpPut("{disputeHistoryId}")]
        public override async Task<ActionResult<DisputeHistoryResultDto>> Update(long disputeHistoryId, DisputeHistoryUpdateDto disputeHistory, bool ignoreMissingOrNullFields)
        {
            return await base.Update(disputeHistoryId, disputeHistory, ignoreMissingOrNullFields);
        }

        [HttpDelete("{disputeHistoryId}")]
        public override async Task<IActionResult> Delete(long disputeHistoryId)
        {
            return await base.Delete(disputeHistoryId);
        }
    }
}

#pragma warning restore IDE0290 // Use primary constructor