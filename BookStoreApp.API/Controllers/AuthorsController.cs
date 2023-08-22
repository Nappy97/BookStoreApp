using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.API.Data;
using BookStoreApp.API.Models.Author;
using BookStoreApp.API.Static;

namespace BookStoreApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorsController : ControllerBase
{
    private readonly BookStoreDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthorsController> _logger;

    public AuthorsController(BookStoreDbContext context, IMapper mapper, ILogger<AuthorsController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: api/Authors
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorReadOnlyDto>>> GetAuthors()
    {
        _logger.LogInformation($"Request to {nameof(GetAuthors)}");
        try
        {
            var authors = await _context.Authors.ToListAsync();

            var modelAuthors = _mapper.Map<IEnumerable<AuthorReadOnlyDto>>(authors);

            return Ok(modelAuthors);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error GET in {nameof(GetAuthors)}");
            return StatusCode(500, Messages.Error500Message);
        }
    }

    // GET: api/Authors/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorReadOnlyDto>> GetAuthor(int id)
    {
        _logger.LogInformation($"Request to {nameof(GetAuthor)}");
        try
        {
            var author = await _context.Authors.FindAsync(id);

            if (author == null)
            {
                _logger.LogError($"Record Not Found: {nameof(GetAuthor)} - ID: {id}");
                return NotFound();
            }

            var authorDto = _mapper.Map<AuthorReadOnlyDto>(author);

            return Ok(authorDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error GET in {nameof(GetAuthor)}");
            return StatusCode(500, Messages.Error500Message);
        }
    }

    // PUT: api/Authors/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutAuthor(int id, AuthorUpdateDto authorDto)
    {
        if (id != authorDto.Id)
        {
            _logger.LogWarning($"Update Id invalid in {nameof(PutAuthor)} - ID: {id}");
            return BadRequest();
        }

        var author = await _context.Authors.FindAsync(id);

        if (author == null)
        {
            _logger.LogWarning($"{nameof(author)} record not found in {nameof(PutAuthor)} - ID: {id}");
            return NotFound();
        }

        _mapper.Map(authorDto, author);
        _context.Entry(author).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await AuthorExists(id))
            {
                return NotFound();
            }
            else
            {
                _logger.LogError(ex, $"Error GET in {nameof(PutAuthor)}");
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Authors
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<AuthorCreateDto>> PostAuthor(AuthorCreateDto authorDto)
    {
        try
        {
            var author = _mapper.Map<Author>(authorDto);

            await _context.Authors.AddAsync(author);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, author);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error POST in {nameof(PostAuthor)}", authorDto);
            return StatusCode(500, Messages.Error500Message);
        }
    }

    // DELETE: api/Authors/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        try
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                _logger.LogWarning($"{nameof(Author)} record not found in {nameof(DeleteAuthor)} - ID: {id}");
                return NotFound();
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error DELETE in {nameof(DeleteAuthor)}");
            return StatusCode(500, Messages.Error500Message);
        }
    }

    private async Task<bool> AuthorExists(int id)
    {
        return await _context.Authors?.AnyAsync(e => e.Id == id);
    }
}