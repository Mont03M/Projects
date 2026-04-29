using Microsoft.Extensions.DependencyInjection;
using server.apps.contexts;
using server.apps.todoApp.models;
using server.apps.todoApp.services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<CounterService>(sp =>
        {
            var context = sp.GetRequiredService<MongoContext>();
            return new CounterService(context.Counters);
        });

        services.AddScoped<IGenericService<Todo>>(sp =>
        {
            var context = sp.GetRequiredService<MongoContext>();
            return new GenericService<Todo>(context.Todos);
        });

        services.AddScoped<TodoService>();

        return services;
    }
}