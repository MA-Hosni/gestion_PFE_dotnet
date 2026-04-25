// ════════════════════════════════════════════════════════════════
//  JwtTokenService.cs — APRÈS refactoring avec Singleton
//  Couche : PfeManagement.Infrastructure
//  Responsable : Khemissi Nour
//
//  AVANT : dépendait de IConfiguration injecté via constructeur
//          → _config.GetSection("JwtSettings") relu à chaque appel
//
//  APRÈS : utilise AppConfigurationManager.Instance
//          → valeurs déjà en mémoire RAM, aucune relecture
// ════════════════════════════════════════════════════════════════

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        // ── Référence vers le Singleton ──────────────────────────
        // Plus besoin de IConfiguration dans le constructeur.
        // L'instance unique est déjà initialisée dans Program.cs.
        private readonly AppConfigurationManager _config;

        public JwtTokenService()
        {
            // Récupère l'instance unique — jamais de "new" ici
            _config = AppConfigurationManager.Instance;
        }

        /// <summary>
        /// Génère un JWT pour l'utilisateur donné.
        /// Les paramètres JWT (secret, expiration) viennent du
        /// Singleton — déjà en RAM depuis le démarrage du serveur.
        /// </summary>
        public string GenerateToken(User user)
        {
            // Valeurs lues depuis le Singleton (RAM) — pas de I/O
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config.JwtSecret));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,
                          user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email,
                          user.Email),
                new Claim("role",
                          user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti,
                          Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(
                                        _config.JwtAccessTokenExpMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

// ════════════════════════════════════════════════════════════════
//  COMPARAISON — CODE ORIGINAL (AVANT)
//  Ce bloc est fourni à titre de documentation pour le rapport.
//  NE PAS inclure dans le projet final.
// ════════════════════════════════════════════════════════════════
//
// public class JwtTokenService : IJwtTokenService
// {
//     private readonly IConfiguration _config;   // couplage fort
//
//     public JwtTokenService(IConfiguration config)
//     {
//         _config = config;
//     }
//
//     public string GenerateToken(User user)
//     {
//         // PROBLÈME : GetSection relu à CHAQUE requête HTTP !
//         var jwtSettings = _config.GetSection("JwtSettings");
//         var key = new SymmetricSecurityKey(
//             Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
//         var credentials = new SigningCredentials(
//             key, SecurityAlgorithms.HmacSha256);
//         var token = new JwtSecurityToken(
//             expires: DateTime.UtcNow.AddMinutes(
//                 double.Parse(jwtSettings["AccessTokenExpirationMinutes"]!)),
//             signingCredentials: credentials);
//         return new JwtSecurityTokenHandler().WriteToken(token);
//     }
// }