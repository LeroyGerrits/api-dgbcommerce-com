﻿namespace DGBCommerce.Domain.Parameters
{
    public class GetFaqCategoriesParameters
    {
        public Guid? Id { get; set; }
        public Guid? ParentId { get; set; }
        public string? Name { get; set; }
    }
}