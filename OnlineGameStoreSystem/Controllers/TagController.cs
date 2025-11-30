using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

[Route("api/[controller]")]
[ApiController]
public class TagController : ControllerBase
{
    private readonly DB _db;

    public TagController(DB db)
    {
        _db = db;
    }

    // GET api/tag
    [HttpGet]
    public async Task<IActionResult> GetAllTags()
    {
        // 从数据库获取所有标签的 Name
        List<string> tags = await _db.Tags
            .AsNoTracking()  // 不跟踪，提高性能
            .Select(t => t.Name)
            .ToListAsync();

        return Ok(tags);
    }
}
