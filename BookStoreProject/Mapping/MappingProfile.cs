using AutoMapper;
using BookStoreProject.Data;
using BookStoreProject.DTOs;
using BookStoreProject.Models;
using BookStoreProject.ViewModels;

namespace BookStoreProject.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // =========================
            // BOOK MAPPINGS
            // =========================

            // Book -> BookDto
            CreateMap<Book, BookDto>()
                .ForMember(
                    dest => dest.GenreName,
                    opt => opt.MapFrom(src => src.Genre.GenreName)
                );


            // Book -> BookDisplayDto
            CreateMap<Book, BookDisplayDto>()
                .ForMember(
                    dest => dest.GenreName,
                    opt => opt.MapFrom(src => src.Genre.GenreName)
                )
                .ForMember(
                    dest => dest.Quantity,
                    opt => opt.Ignore()
                );


            // Book -> CreateBookDto
            CreateMap<Book, CreateBookDto>();


            // CreateBookDto -> Book
            CreateMap<CreateBookDto, Book>();


            // Book -> UpdateBookDto
            CreateMap<Book, UpdateBookDto>();


            // UpdateBookDto -> Book
            CreateMap<UpdateBookDto, Book>();


            // =========================
            // GENRE MAPPINGS
            // =========================

            // Genre -> GenreDto
            CreateMap<Genre, GenreDto>();


            // GenreDto -> Genre
            CreateMap<GenreDto, Genre>();


            // =========================
            // CART MAPPINGS
            // =========================

            // CartDetail -> CartItemDto
            CreateMap<CartDetail, CartItemDto>()
                .ForMember(
                    dest => dest.BookName,
                    opt => opt.MapFrom(src => src.Book.BookName)
                )
                .ForMember(
                    dest => dest.Image,
                    opt => opt.MapFrom(src => src.Book.Image)
                )
                .ForMember(
                    dest => dest.Price,
                    opt => opt.MapFrom(src => src.Book.Price)
                );


            // ShoppingCartVM -> CartItemDto
            CreateMap<ShoppingCartVM, CartItemDto>();


            // =========================
            // ORDER MAPPINGS
            // =========================

            // Order -> OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(
                    dest => dest.StatusName,
                    opt => opt.MapFrom(src => src.OrderStatus.StatusName)
                );


            // OrderDetail -> OrderDetailDto
            CreateMap<OrderDetail, OrderDetailDto>()
                .ForMember(
                    dest => dest.BookName,
                    opt => opt.MapFrom(src => src.Book.BookName)
                );


            // =========================
            // ORDER STATUS
            // =========================

            // OrderStatus -> OrderStatusDto
            CreateMap<OrderStatus, OrderStatusDto>();


            // =========================
            // DASHBOARD
            // =========================

            // AdminDashboardVM -> AdminDashboardDto
            CreateMap<AdminDashboardVM, AdminDashboardDto>();


            // MonthlyRevenueVM -> MonthlyRevenueDto
            CreateMap<MonthlyRevenueVM, MonthlyRevenueDto>();


            // BookSalesVM -> BookSalesDto
            CreateMap<BookSalesVM, BookSalesDto>();

            CreateMap<GenreSalesVM, GenreSalesDto>();


            // =========================
            // USER
            // =========================

            // ApplicationUser -> ApplicationUserDto
            CreateMap<ApplicationUser, ApplicationUserDto>();

            // =========================
            // AI CHAT
            // =========================

            CreateMap<ChatResponse, ChatResponseDto>();
        }
    }
}