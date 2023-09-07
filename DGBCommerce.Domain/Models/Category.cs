﻿using System.Text.Json.Serialization;

namespace DGBCommerce.Domain.Models
{
    public class Category
    {
        public Guid? Id { get; set; }
        public required Shop Shop { get; set; }
        [JsonIgnore] public Category? Parent { get; set; }
        public required string Name { get; set; }
        public int? SortOrder { get; set; }
        public List<Category>? Children { get; set; }
    }
}