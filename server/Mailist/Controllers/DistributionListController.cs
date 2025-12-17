using ChurchTools;
using ChurchTools.Model;
using Mailist.EmailRelay;
using Mailist.Models.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mailist.Controllers;

[ApiController]
public class DistributionListController : ControllerBase
{
    private readonly DatabaseContext database;
    private readonly IChurchToolsApi churchTools;
    private readonly IMemoryCache memoryCache;

    public DistributionListController(DatabaseContext database, IChurchToolsApi churchTools, IMemoryCache memoryCache)
    {
        this.database = database;
        this.churchTools = churchTools;
        this.memoryCache = memoryCache;
    }

    [Authorize]
    [HttpGet("~/api/distribution-lists")]
    [ProducesResponseType(typeof(DistributionList[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<DistributionList[]>> GetDistributionLists()
    {
        var lists = await database.DistributionLists
            .AsNoTracking()
            .OrderBy(dl => dl.Alias)
            .ToListAsync();

        DistributionList[] response = await Task.WhenAll(lists.Select(AddCachedRecipientCount));

        return response;
    }

    [Authorize]
    [HttpGet("~/api/distribution-lists/{id:long}")]
    [ProducesResponseType(typeof(DistributionList), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DistributionList>> GetDistributionList([FromRoute] long id)
    {
        var distributionList = await database.DistributionLists.AsNoTracking().SingleOrDefaultAsync(dl => dl.Id == id);
        if (distributionList == null)
            return NotFound();

        DistributionList response = await AddCachedRecipientCount(distributionList);

        return response;
    }

    private async Task<DistributionList> AddCachedRecipientCount(EmailRelay.Entities.DistributionList dl)
    {
        JsonElement recipientsQuery = JsonElement.Parse(dl.RecipientsQuery);
        JsonElement sendersQuery = JsonElement.Parse(dl.SendersQuery);
        int recipientCount = 0;
        int senderCount = 0;

        // TODO: Move caching into an own service and probably use separate cache entries for recipient and sender counts
        if (recipientsQuery.ValueKind != JsonValueKind.Null || sendersQuery.ValueKind != JsonValueKind.Null)
        {
            (recipientCount, senderCount) = await memoryCache.GetOrCreateAsync(dl.Id, async cacheEntry =>
            {
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                ChurchQueryRequest<IdNameEmail> recipientsQueryRequest = new(recipientsQuery);
                ChurchQueryRequest<IdNameEmail> sendersQueryRequest = new(sendersQuery);
                var recipients = churchTools.ChurchQuery(recipientsQueryRequest);
                var senders = churchTools.ChurchQuery(sendersQueryRequest);
                return ((await recipients).Count, (await senders).Count);
            });
        }

        return new DistributionList
        {
            Id = dl.Id,
            Alias = dl.Alias,
            Newsletter = dl.Flags.HasFlag(DistributionListFlags.Newsletter),
            RecipientsQuery = recipientsQuery,
            SendersQuery = sendersQuery,
            RecipientCount = recipientCount,
            SenderCount = senderCount,
        };
    }

    [Authorize(Roles = "admin")]
    [HttpPost("~/api/distribution-lists")]
    [ProducesResponseType(typeof(DistributionList), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDistributionList([FromBody] CreateDistributionList request)
    {
        if (string.IsNullOrWhiteSpace(request.Alias))
            return BadRequest("Alias must not be empty");

        int recipientCount = 0;

        if (request.RecipientsQuery.ValueKind != JsonValueKind.Null)
        {
            try // Evaluate query to make sure it's valid
            {
                ChurchQueryRequest<IdNameEmail> query = new(request.RecipientsQuery);
                var recipients = await churchTools.ChurchQuery(query);
                recipientCount = recipients.Count;
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid RecipientsQuery: {ex.Message}");
            }
        }

        var distributionList = new EmailRelay.Entities.DistributionList(
            request.Alias,
            request.RecipientsQuery.GetRawText(),
            request.SendersQuery.GetRawText())
        {
            Flags = request.Newsletter ? DistributionListFlags.Newsletter : DistributionListFlags.None,
        };

        database.DistributionLists.Add(distributionList);
        await database.SaveChangesAsync();

        memoryCache.Set(distributionList.Id, recipientCount, TimeSpan.FromMinutes(5));

        var response = new DistributionList
        {
            Id = distributionList.Id,
            Alias = distributionList.Alias,
            Newsletter = distributionList.Flags.HasFlag(DistributionListFlags.Newsletter),
            RecipientsQuery = JsonElement.Parse(distributionList.RecipientsQuery),
            RecipientCount = recipientCount,
        };

        return Created($"/api/distribution-lists/{distributionList.Id}", response);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("~/api/distribution-lists/{id:long}")]
    [ProducesResponseType(typeof(DistributionList), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ModifyDistributionList([FromRoute] long id, [FromBody] CreateDistributionList request)
    {
        if (string.IsNullOrWhiteSpace(request.Alias))
            return BadRequest("Alias must not be empty");

        var distributionList = await database.DistributionLists.FindAsync(id);
        if (distributionList == null)
            return NotFound();

        distributionList.Alias = request.Alias;
        distributionList.Flags = request.Newsletter ? DistributionListFlags.Newsletter : DistributionListFlags.None;
        distributionList.RecipientsQuery = request.RecipientsQuery.GetRawText();

        int recipientCount = 0;

        if (request.RecipientsQuery.ValueKind != JsonValueKind.Null)
        {
            ChurchQueryRequest<IdNameEmail> query = new(request.RecipientsQuery);
            var recipients = await churchTools.ChurchQuery(query);
            recipientCount = recipients.Count;
        }

        await database.SaveChangesAsync();

        memoryCache.Set(distributionList.Id, recipientCount, TimeSpan.FromMinutes(5));

        var response = new DistributionList
        {
            Id = distributionList.Id,
            Alias = distributionList.Alias,
            Newsletter = distributionList.Flags.HasFlag(DistributionListFlags.Newsletter),
            RecipientsQuery = JsonElement.Parse(distributionList.RecipientsQuery),
            RecipientCount = recipientCount,
        };

        return Ok(response);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("~/api/distribution-lists/{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDistributionList([FromRoute] long id)
    {
        var distributionList = await database.DistributionLists.FindAsync(id);

        if (distributionList == null)
            return NotFound();

        database.DistributionLists.Remove(distributionList);
        await database.SaveChangesAsync();

        return NoContent();
    }
}
