using Impartial;

namespace ImpartialApi
{
    public static class Api
    {
        public static void ConfigureApi(this WebApplication app)
        {
            app.MapGet(pattern: "/Competitors", GetCompetitors);
            app.MapGet(pattern: "/Competitors/{id}", GetCompetitorById);
        }

        private static async Task<IResult> GetCompetitors(IDatabaseProvider db)
        {
            try
            {
                return Results.Ok(await db.GetAllCompetitors());
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> GetCompetitorById(int id, IDatabaseProvider db)
        {
            try
            {
                var results = await db.GetAllCompetitors();
                return Results.Ok(await data.GetUsers());
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }
    }
}
