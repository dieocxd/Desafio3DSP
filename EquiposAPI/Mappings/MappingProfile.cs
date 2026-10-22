using AutoMapper;
using EquiposAPI.DTOs.Auth;
using EquiposAPI.DTOs.Categorias;
using EquiposAPI.DTOs.Equipos;
using EquiposAPI.DTOs.Prestamos;
using EquiposAPI.Models;

namespace EquiposAPI.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Categorias
        CreateMap<Categoria, CategoriaDto>();
        CreateMap<CategoriaCreateDto, Categoria>();
        CreateMap<CategoriaUpdateDto, Categoria>();

        // Equipos
        CreateMap<Equipo, EquipoDto>()
            .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : string.Empty));
        CreateMap<EquipoCreateDto, Equipo>();
        CreateMap<EquipoUpdateDto, Equipo>()
            .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => src.RowVersion));

        // Usuarios
        CreateMap<Usuario, UsuarioDto>();
        CreateMap<RegisterUserDto, Usuario>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        // Prestamos
        CreateMap<Prestamo, PrestamoDto>()
            .ForMember(dest => dest.EquipoCodigo, opt => opt.MapFrom(src => src.Equipo != null ? src.Equipo.CodigoInventario : string.Empty))
            .ForMember(dest => dest.EquipoNombre, opt => opt.MapFrom(src => src.Equipo != null ? src.Equipo.Nombre : string.Empty))
            .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Username : string.Empty));
        CreateMap<PrestamoCreateDto, Prestamo>();
    }
}
