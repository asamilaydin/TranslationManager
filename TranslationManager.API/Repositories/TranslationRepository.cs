using Microsoft.EntityFrameworkCore;
using TranslationManager.API.Data;
using TranslationManager.API.Interfaces;
using TranslationManager.API.Models;

namespace TranslationManager.API.Repositories
{
    public class TranslationRepository : ITranslationRepository
    {
        private readonly TranslationDbContext _context;

        public TranslationRepository(TranslationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Translation>> GetAllAsync()
        {
            return await _context.Translations
                .OrderBy(t => t.ResourceKey)
                .ToListAsync();
        }

        public async Task<IEnumerable<Translation>> GetByPlatformAsync(string platform)
        {
            return await _context.Translations
                .Where(t => t.Platform == platform)
                .OrderBy(t => t.ResourceKey)
                .ToListAsync();
        }

        public async Task<IEnumerable<Translation>> SearchAsync(string searchTerm)
        {
            var searchLower = searchTerm.ToLower();
            return await _context.Translations
                .Where(t => 
                    t.ResourceKey.ToLower().Contains(searchLower) ||
                    t.En.ToLower().Contains(searchLower) ||
                    (t.Tr != null && t.Tr.ToLower().Contains(searchLower)) ||
                    (t.De != null && t.De.ToLower().Contains(searchLower))
                )
                .OrderBy(t => t.ResourceKey)
                .ToListAsync();
        }

        public async Task<IEnumerable<Translation>> GetRecentAsync(int limit = 50)
        {
            return await _context.Translations
                .OrderByDescending(t => t.UpdatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Translation?> GetByIdAsync(int id)
        {
            return await _context.Translations.FindAsync(id);
        }

        public async Task<Translation?> GetByResourceKeyAsync(string resourceKey)
        {
            return await _context.Translations
                .FirstOrDefaultAsync(t => t.ResourceKey == resourceKey);
        }

        public async Task<IEnumerable<Translation>> GetByResourceKeysAsync(IEnumerable<string> resourceKeys)
        {
            return await _context.Translations
                .Where(t => resourceKeys.Contains(t.ResourceKey))
                .ToListAsync();
        }

        public async Task<Translation> CreateAsync(Translation translation)
        {
            translation.CreatedAt = DateTime.UtcNow;
            translation.UpdatedAt = DateTime.UtcNow;
            
            _context.Translations.Add(translation);
            await _context.SaveChangesAsync();
            
            return translation;
        }

        public async Task<IEnumerable<Translation>> CreateRangeAsync(IEnumerable<Translation> translations)
        {
            var translationList = translations.ToList();
            foreach (var translation in translationList)
            {
                translation.CreatedAt = DateTime.UtcNow;
                translation.UpdatedAt = DateTime.UtcNow;
            }
            
            _context.Translations.AddRange(translationList);
            await _context.SaveChangesAsync();
            
            return translationList;
        }

        public async Task<Translation> UpdateAsync(Translation translation)
        {
            translation.UpdatedAt = DateTime.UtcNow;
            
            _context.Entry(translation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return translation;
        }

        public async Task<IEnumerable<Translation>> UpdateRangeAsync(IEnumerable<Translation> translations)
        {
            var translationList = translations.ToList();
            foreach (var translation in translationList)
            {
                translation.UpdatedAt = DateTime.UtcNow;
            }
            
            _context.Translations.UpdateRange(translationList);
            await _context.SaveChangesAsync();
            
            return translationList;
        }

        public async Task DeleteAsync(int id)
        {
            var translation = await _context.Translations.FindAsync(id);
            if (translation != null)
            {
                _context.Translations.Remove(translation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<int> ids)
        {
            var translations = await _context.Translations
                .Where(t => ids.Contains(t.Id))
                .ToListAsync();

            if (translations.Any())
            {
                _context.Translations.RemoveRange(translations);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllAsync()
        {
            var allTranslations = await _context.Translations.ToListAsync();
            _context.Translations.RemoveRange(allTranslations);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Translations.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> ExistsByResourceKeyAsync(string resourceKey)
        {
            return await _context.Translations.AnyAsync(e => e.ResourceKey == resourceKey);
        }
    }
}