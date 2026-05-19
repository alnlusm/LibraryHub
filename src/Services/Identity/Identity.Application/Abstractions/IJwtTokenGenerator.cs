using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string Generate(AppUser user);
}
