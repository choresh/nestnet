#pragma warning disable IDE0290 // Use primary constructor
using Microsoft.AspNetCore.Mvc;
using NestNet.Infra.BaseClasses;
using GlobalE.Payments.Manager.Core.Modules.Disputes.Dtos;
using GlobalE.Payments.Manager.Core.Modules.Disputes.Services;
using GlobalE.Payments.Manager.Core.Modules.Disputes.Entities;

namespace GlobalE.Payments.Manager.Api.Modules.Disputes.Controllers
{
    [Route("api/disputes")]
    public class DisputesController : CrudControllerBase<DisputeEntity, DisputeCreateDto, DisputeUpdateDto, DisputeResultDto, DisputeQueryDto>
    {
        public DisputesController(IDisputesService disputesService)
            : base(disputesService, "DisputeId")
        {
        }

        [HttpGet]
        public override async Task<ActionResult<IEnumerable<DisputeResultDto>>> GetAll()
        {
            return await base.GetAll();
        }

        [HttpGet("{disputeId}")]
        public override async Task<ActionResult<DisputeResultDto>> GetById(long disputeId)
        {
            return await base.GetById(disputeId);
        }

        [HttpPost]
        public override async Task<ActionResult<DisputeResultDto>> Create(DisputeCreateDto dispute)
        {
            return await base.Create(dispute);
        }

        [HttpPut("{disputeId}")]
        public override async Task<ActionResult<DisputeResultDto>> Update(long disputeId, DisputeUpdateDto dispute, bool ignoreMissingOrNullFields)
        {
            return await base.Update(disputeId, dispute, ignoreMissingOrNullFields);
        }

        [HttpDelete("{disputeId}")]
        public override async Task<IActionResult> Delete(long disputeId)
        {
            return await base.Delete(disputeId);
        }
    }
}

#pragma warning restore IDE0290 // Use primary constructor