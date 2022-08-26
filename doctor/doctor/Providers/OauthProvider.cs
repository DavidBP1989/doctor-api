using Dapper;
using doctor.Models.Auth;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace doctor.Providers
{
    public class OauthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string connection = "";
        public OauthProvider()
        {
            connection = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
        }

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            await Task.Run(() => context.Validated());
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);

            using (IDbConnection db = new SqlConnection(connection))
            {
                db.Open();

                var query = db.Query<AuthModel>($@"
                        select r.Emeci, m.Idmedico as DoctorId from
                        registro as r
                        inner join medico as m on r.idRegistro = m.IdRegistro
                        where
                        r.emails like '%{context.UserName}%' and
                        r.clave = '{context.Password}' and
                        r.status = 'V' and
                        r.tipo = 'M'").FirstOrDefault();

                if (query != null)
                {
                    context.Response.Headers.Add("Access-Control-Expose-Headers", new[] { "dId" });
                    context.Response.Headers.Add("dId", new[] { query.DoctorId.ToString() });

                    identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
                    identity.AddClaim(new Claim(ClaimTypes.Name, query.Emeci));
                    identity.AddClaim(new Claim("LoggedOn", DateTime.Now.ToString()));

                    await Task.Run(() => context.Validated(identity));
                }
                else context.SetError("Credenciales incorrectas", "El usuario o contraseña es incorrecto");
                return;
            }
        }
    }
}