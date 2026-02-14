using MMS.Application.Ports.In.NoSql.HistoricalStat.Dto;

namespace MMS.Adapters.Rest.Controllers
{
    /// <summary>
    /// Controller for managing historical statistics for machines.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HistoricalStatsController(IHistoricalStatsService service) : ControllerBase
    {
        #region Queries

        /// <summary>
        /// Retrieves all historical stats for a given machine.
        /// </summary>
        /// <param name="machineId">The machine ID.</param>
        /// <returns>List of historical stats for the machine.</returns>
        [HttpGet("GetAllByMachine/{machineId}")]
        [SwaggerOperation(Summary = "Get all historical stats by machine ID", Description = "Returns all historical stats for a specific machine.")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<HistoricalStats>>), StatusCodes.Status200OK)]
        [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
        public async Task<IActionResult> GetAllByMachine(Guid machineId)
        {
            var response = await service.GetAllByMachineIdAsync(machineId);
            return StatusCode(response.StatusCode, response);
        }


        /// <summary>
        /// Gets all historical stats.
        /// </summary>
        /// <returns>List of all historical stats.</returns>
        [HttpGet("GetAll")]
        [SwaggerOperation(Summary = "Get all historical stats", Description = "Returns all historical stats from the database.")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<HistoricalStats>>), StatusCodes.Status200OK)]
        //[Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
        public async Task<IActionResult> GetAll([FromQuery] PageParameters pageParameters)
        {
            var response = await service.GetAllAsync(pageParameters);
            return StatusCode(response.StatusCode, response);
        }


        /// <summary>
        /// Retrieves a historical stat entry by its ID.
        /// </summary>
        /// <param name="id">The ID of the historical stat entry.</param>
        /// <returns>The requested historical stat.</returns>
        [HttpGet("GetById/{id}")]
        [SwaggerOperation(Summary = "Get historical stat by ID", Description = "Fetches a historical stat entry using its ID.")]
        [ProducesResponseType(typeof(ApiResponse<HistoricalStats>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
        public async Task<IActionResult> GetById(string id)
        {
            var response = await service.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Creates historical stats records for all machines on a given day.
        /// </summary>
        /// <param name="request">Request containing the date for which to create historical stats.</param>
        /// <returns>List of historical stats records created for each machine on that day.</returns>
        [HttpPost("createHistoricalRecordForDay")]
        [SwaggerOperation(
            Summary = "Create historical stats records for a day",
            Description = "Generates and stores historical stats for all machines on the specified day.")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<HistoricalStats>>), StatusCodes.Status201Created)]
        [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
        public async Task<IActionResult> CreateHistoricalRecordForDay([FromBody] CreateHistoricalRecordForDayDto request)
        {
            var response = await service.CreateHistoricalRecordForDayAsync(request.Date);
            return StatusCode(response.StatusCode, response);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Creates a new historical stat entry.
        /// </summary>
        /// <param name="model">The historical stat data to create.</param>
        /// <returns>The newly created historical stat.</returns>
        [HttpPost("Create")]
        [SwaggerOperation(Summary = "Create historical stat", Description = "Adds a new historical stat record.")]
        [ProducesResponseType(typeof(ApiResponse<HistoricalStats>), StatusCodes.Status201Created)]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] HistoricalStats model)
        {
            var response = await service.CreateAsync(model);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Updates an existing historical stat entry.
        /// </summary>
        /// <param name="model">The historical stat object with updated data.</param>
        [HttpPut("Update")]
        [SwaggerOperation(Summary = "Update historical stat", Description = "Updates an existing historical stat entry.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Policy = AuthorizationPolicies.RequireMMSBridge)]
        public async Task<IActionResult> Update([FromBody] HistoricalStats model)
        {
            var response = await service.UpdateAsync(model);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Deletes a historical stat entry by ID.
        /// </summary>
        /// <param name="id">The ID of the historical stat to delete.</param>
        [HttpDelete("Delete/{id}")]
        [SwaggerOperation(Summary = "Delete historical stat", Description = "Deletes a historical stat entry by its ID.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]
        public async Task<IActionResult> Delete(string id)
        {
            var response = await service.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        #endregion
    }
}
