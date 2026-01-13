using lily.Data;
using lily.Models;
using Microsoft.EntityFrameworkCore;

namespace lily.Repository
{
    public class ToysRepository
    {
        private readonly ToyStoreContext _context;

        public ToysRepository(ToyStoreContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Toy>> GetAllAsync()
        {
            return await _context.Toys.ToListAsync();
        }

        public async Task<Toy?> GetByIdAsync(int id)
        {
            return await _context.Toys.FindAsync(id);
        }

        public async Task<Toy> AddAsync(Toy toy)
        {
            _context.Toys.Add(toy);
            await _context.SaveChangesAsync();
            return toy;
        }

        public async Task UpdateAsync(Toy toy)
        {
            _context.Toys.Update(toy);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var toy = await _context.Toys.FindAsync(id);
            if (toy != null)
            {
                _context.Toys.Remove(toy);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Toy>> SearchAsync(string searchTerm)
        {
            return await _context.Toys
                .Where(t => t.ToyName.Contains(searchTerm) || 
                           (t.Description != null && t.Description.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<IEnumerable<Toy>> GetByCategoryAsync(string category)
        {
            return await _context.Toys
                .Where(t => t.Category == category && t.IsActive == true)
                .ToListAsync();
        }
    }
}
