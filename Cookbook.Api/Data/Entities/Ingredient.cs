﻿using System;

namespace Cookbook.Api.Data.Entities
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImgSrc { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}