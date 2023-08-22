using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.API.Data;
using BookStoreApp.API.Models.Book;

namespace BookStoreApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly BookStoreDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<BooksController> _logger;

    public BooksController(BookStoreDbContext context,
        IMapper mapper,
        ILogger<BooksController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: api/Books
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookReadOnlyDto>>> GetBooks()
    {
        var bookDtos = await _context.Books
            .Include(q => q.Author)
            .ProjectTo<BookReadOnlyDto>(_mapper.ConfigurationProvider) // 바로 Automapping
            .ToListAsync();
        // var bookDtos = _mapper.Map<IEnumerable<BookReadOnlyDto>>(books);
        return Ok(bookDtos);
    }

    // GET: api/Books/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDetailsDto>> GetBook(int id)
    {
        var book = await _context.Books
            .Include(q => q.Author)
            .ProjectTo<BookDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (book == null)
        {
            _logger.LogError($"Record Not Found: {nameof(GetBook)} - ID: {id}");
            return NotFound();
        }

        return Ok(book);
    }

    // PUT: api/Books/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutBook(int id, BookUpdateDto bookDto)
    {
        if (id != bookDto.Id)
        {
            return BadRequest();
        }

        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            _logger.LogError($"Record Not Found: {nameof(PutBook)} - ID: {id}");
            return NotFound();
        }

        _mapper.Map(bookDto, book);
        _context.Entry(book).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await BookExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Books
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Book>> PostBook(BookCreateDto bookDto)
    {
        var book = _mapper.Map<Book>(bookDto);
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    // DELETE: api/Books/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> BookExists(int id)
    {
        return await _context.Books.AnyAsync(e => e.Id == id);
    }
}