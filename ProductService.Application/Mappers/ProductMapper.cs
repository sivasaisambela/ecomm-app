using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Mappers
{
    /// <summary>
    /// AutoMapper profile for Product mappings
    /// 
    /// Why AutoMapper?
    /// - Reduces boilerplate code
    /// - Centralized mapping logic
    /// - Easy to maintain
    /// - Type-safe
    /// 
    /// Mappings:
    /// - Product → ProductDto
    /// - CreateProductDto → Product
    /// - UpdateProductDto → Product
    /// </summary>
    public class ProductMapper : Profile
    {
        public ProductMapper()
        {
            // ============================================
            // ENTITY TO DTO
            // ============================================

            // Product → ProductDto
            CreateMap<Product, ProductDto>()
                // Map computed properties
                .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.AvailableQuantity))
                .ForMember(dest => dest.IsStockLow, opt => opt.MapFrom(src => src.IsStockLow));

            // ============================================
            // DTO TO ENTITY (Create)
            // ============================================

            // CreateProductDto → Product
            // Uses factory method to ensure valid state
            CreateMap<CreateProductDto, Product>()
                .ConvertUsing((src, dest, ctx) =>
                    Product.Create(
                        src.Name,
                        src.Description,
                        src.Sku,
                        src.Price,
                        src.Cost,
                        src.StockQuantity,
                        src.MinimumStockLevel,
                        src.Category,
                        "system"  // Default user
                    ));

            // ============================================
            // DTO TO ENTITY (Update)
            // ============================================

            // UpdateProductDto → Product
            // Note: This just maps properties, actual update happens in service
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Cost))
                .ForMember(dest => dest.MinimumStockLevel, opt => opt.MapFrom(src => src.MinimumStockLevel))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
        }
    }
}
