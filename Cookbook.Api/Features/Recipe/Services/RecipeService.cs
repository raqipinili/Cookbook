﻿using AutoMapper;
using Cookbook.Api.Data;
using Cookbook.Api.Data.Entities;
using Cookbook.Api.Features.Recipe.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cookbook.Api.Features.Recipe.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly CookbookDbContext _context;
        private readonly IMapper _mapper;

        public RecipeService(CookbookDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region Recipe
        public async Task<int> AddRecipe(RecipeDto recipeDto)
        {
            int recipeId = 0;
            if (recipeDto.RecipeId == 0)
            {
                recipeId = await addRecipe(recipeDto);
                await UpdateRecipeIngredients(recipeId, recipeDto.IngredientIds);
            }
            else if (await HasRecipe(recipeDto.RecipeId))
            {
                recipeId = recipeDto.RecipeId;
                await UpdateRecipeIngredients(recipeId, recipeDto.IngredientIds);
            }

            return recipeId;
        }

        public async Task<int> UpdateRecipe(RecipeDto recipeDto)
        {
            int recipeId = await updateRecipe(recipeDto);
            return await UpdateRecipeIngredients(recipeId, recipeDto.IngredientIds);
        }

        public async Task<RecipeDto> GetRecipe(int userId, int recipeId)
        {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.UserId == userId && r.RecipeId == recipeId);
            return _mapper.Map<RecipeDto>(recipe);
        }

        public async Task<IEnumerable<RecipeDto>> GetRecipes(int userId)
        {
            var recipes = await _context.Recipes.Where(r => r.UserId == userId).OrderByDescending(r => r.CreatedDate).ToListAsync();
            return _mapper.Map<IEnumerable<RecipeDto>>(recipes);
        }

        public async Task<bool> HasRecipe(int recipeId)
        {
            return await _context.Recipes.AnyAsync(r => r.RecipeId == recipeId);
        }

        public async Task<bool> HasRecipe(string recipeName)
        {
            return await _context.Recipes.AnyAsync(r => r.Name == recipeName);
        }
        #endregion

        #region Recipe Ingredients
        public async Task<int> UpdateRecipeIngredients(int recipeId, IEnumerable<int> ingredientIds)
        {
            await RemoveAllRecipeIngredients(recipeId);
            return await AddRecipeIngredientIds(recipeId, ingredientIds);
        }

        public async Task<int> AddRecipeIngredientIds(int recipeId, IEnumerable<int> ingredientIds)
        {
            if (ingredientIds == null || ingredientIds.Count() == 0)
                return 0;

            var recipeIngredients = ingredientIds.Select(ingredientId => new RecipeIngredient()
            {
                RecipeId = recipeId,
                IngredientId = ingredientId
            });

            await _context.RecipeIngredients.AddRangeAsync(recipeIngredients);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> RemoveAllRecipeIngredients(int recipeId)
        {
            var remove = _context.RecipeIngredients.Where(ri => ri.RecipeId == recipeId);
            _context.RecipeIngredients.RemoveRange(remove);
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<int>> GetRecipeIngredientIds(int userId, int recipeId)
        {
            var result = from r in _context.Recipes
                         join ri in _context.RecipeIngredients on r.RecipeId equals ri.RecipeId
                         where r.UserId == userId && r.RecipeId == recipeId
                         select ri.IngredientId;

            return await result.ToListAsync();
        }
        #endregion

        #region Private Helper Methods
        private async Task<int> addRecipe(RecipeDto recipeDto)
        {
            if (await _context.Recipes.AnyAsync(r => r.Name == recipeDto.Name))
                throw new System.Exception($"Error: Recipe '{recipeDto.Name}' already exists");

            recipeDto.RecipeId = 0;

            var recipe = _mapper.Map<Data.Entities.Recipe>(recipeDto);

            await _context.Recipes.AddAsync(recipe);
            await _context.SaveChangesAsync();
            return recipe.RecipeId;
        }

        public async Task<int> updateRecipe(RecipeDto recipeDto)
        {
            if (await _context.Recipes.AnyAsync(r => r.RecipeId != recipeDto.RecipeId && r.Name == recipeDto.Name))
                throw new System.Exception($"Error: Recipe '{recipeDto.Name}' already exists");

            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.RecipeId == recipeDto.RecipeId);
            if (recipe == null)
                throw new System.Exception($"Error: Recipe with ID: {recipeDto.RecipeId} not found ");

            recipe.Name = recipeDto.Name;
            recipe.Description = recipeDto.Description;
            recipe.ImgSrc = recipeDto.ImgSrc;

            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();
            return recipe.RecipeId;
        }
        #endregion
    }
}
