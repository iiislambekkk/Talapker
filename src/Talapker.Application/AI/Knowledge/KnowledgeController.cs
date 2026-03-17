using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Assistant;
using Wolverine;

namespace Talapker.Application.AI.Knowledge;

[ApiController]
[Route("institutions/{institutionId:guid}/knowledge")]
public class KnowledgeController(
    TalapkerDbContext db,
    IAmazonS3 s3Client,
    IMessageBus bus,
    IConfiguration configuration
) : ControllerBase
{
    [HttpPost("files")]
    public async Task<ActionResult<UploadKnowledgeFileResponse>> UploadFileAsync(
        Guid institutionId,
        IFormFile file,
        [FromForm] string? name)
    {
        if (file.Length == 0)
            return BadRequest("Файл пустой");

        var institution = await db.Institutions.FindAsync(institutionId);
        if (institution is null)
            return NotFound("Учреждение не найдено");

        var fileId = Guid.NewGuid();
        var ext = Path.GetExtension(file.FileName);
        var displayName = string.IsNullOrWhiteSpace(name) ? file.FileName : name.Trim() + ext;
        var key = $"knowledge/{institutionId}/{Guid.NewGuid()}{ext}";

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        await s3Client.PutObjectAsync(new PutObjectRequest
        {
            InputStream = new MemoryStream(memoryStream.ToArray()),
            Key = key,
            BucketName = configuration["AWS3Settings:Bucket"]!,
            ContentType = file.ContentType,
            UseChunkEncoding = false
        });

        var knowledgeFile = new KnowledgeFile
        {
            Id = fileId,
            InstitutionId = institutionId,
            FileName = displayName,
            StorageKey = key,
            Status = KnowledgeFileStatus.Pending,
            ContentType = file.ContentType
        };

        db.KnowledgeFiles.Add(knowledgeFile);
        await db.SaveChangesAsync();

        await bus.PublishAsync(new ProcessKnowledgeFile(fileId));

        return Ok(new UploadKnowledgeFileResponse(
            FileId: fileId,
            FileName: displayName,
            StorageKey: key,
            Status: KnowledgeFileStatus.Pending.ToString()
        ));
    }

    [HttpGet("files")]
    public async Task<ActionResult<List<KnowledgeFileDto>>> GetFilesAsync(Guid institutionId)
    {
        var files = await db.KnowledgeFiles
            .Where(f => f.InstitutionId == institutionId)
            .OrderByDescending(f => f.UploadedAt)
            .Select(f => new KnowledgeFileDto(
                f.Id,
                f.FileName,
                f.StorageKey,
                f.Status.ToString(),
                f.ErrorMessage,
                f.UploadedAt,
                f.ProcessedAt,
                f.Entries.Count
            ))
            .ToListAsync();

        return Ok(files);
    }

    [HttpDelete("files/{fileId:guid}")]
    public async Task<ActionResult> DeleteFileAsync(Guid institutionId, Guid fileId)
    {
        try
        {
            await bus.InvokeAsync(new DeleteKnowledgeFile(institutionId, fileId));
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("entries")]
    public async Task<ActionResult<PagedResult<KnowledgeEntryDto>>> GetEntriesAsync(
        Guid institutionId,
        [FromQuery] KnowledgeEntriesQuery query)
    {
        var q = db.KnowledgeEntries
            .Include(e => e.SourceFile)
            .Where(e => e.InstitutionId == institutionId);

        if (query.ManualOnly == true)
            q = q.Where(e => e.SourceFileId == null);
        else if (query.FileId.HasValue)
            q = q.Where(e => e.SourceFileId == query.FileId);

        if (!string.IsNullOrWhiteSpace(query.Tag))
            q = q.Where(e => e.Tags.Contains(query.Tag));

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderByDescending(e => e.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new KnowledgeEntryDto(
                e.Id,
                e.SourceFileId,
                e.SourceFile != null ? e.SourceFile.FileName : null,
                e.SourceFile != null ? e.SourceFile.StorageKey : null,
                e.Question,
                e.Answer,
                e.Tags,
                e.CreatedAt,
                e.UpdatedAt
            ))
            .ToListAsync();

        return Ok(new PagedResult<KnowledgeEntryDto>(items, totalCount, query.Page, query.PageSize));
    }

    [HttpPost("entries")]
    public async Task<ActionResult<KnowledgeEntryDto>> CreateEntryAsync(
        Guid institutionId,
        [FromBody] CreateKnowledgeEntryRequest request)
    {
        try
        {
            var result = await bus.InvokeAsync<CreateKnowledgeEntryResult>(
                new CreateKnowledgeEntry(institutionId, request.Question, request.Answer, request.Tags));

            return Ok(new KnowledgeEntryDto(
                result.EntryId, null, null, null,
                result.Question, result.Answer, result.Tags,
                result.CreatedAt, null
            ));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("entries/{entryId:guid}")]
    public async Task<ActionResult<KnowledgeEntryDto>> UpdateEntryAsync(
        Guid institutionId,
        Guid entryId,
        [FromBody] UpdateKnowledgeEntryRequest request)
    {
        try
        {
            var result = await bus.InvokeAsync<UpdateKnowledgeEntryResult>(
                new UpdateKnowledgeEntry(institutionId, entryId, request.Question, request.Answer, request.Tags));

            return Ok(new KnowledgeEntryDto(
                result.EntryId,
                result.SourceFileId,
                result.SourceFileName,
                result.SourceFileKey,
                result.Question,
                result.Answer,
                result.Tags,
                result.CreatedAt,
                result.UpdatedAt
            ));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("entries/{entryId:guid}")]
    public async Task<ActionResult> DeleteEntryAsync(Guid institutionId, Guid entryId)
    {
        try
        {
            await bus.InvokeAsync(new DeleteKnowledgeEntry(institutionId, entryId));
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("ask")]
    public async Task<ActionResult<AskKnowledgeResponse>> AskAsync(
        Guid institutionId,
        [FromBody] AskKnowledgeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Вопрос пустой");

        var result = await bus.InvokeAsync<AskKnowledgeResult>(
            new AskKnowledge(institutionId, request.Question));

        return Ok(new AskKnowledgeResponse(result.Answer, result.Sources));
    }
}