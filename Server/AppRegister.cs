using Autofac;
using Sunny.Framework.DB.Repository;
using WishServer.Service;

namespace WishServer;

public class AppRegister : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(ThisAssembly)
            .AsClosedTypesOf(typeof(IRepositoryBase<,>))
            .AsSelf()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope(); // 避免DbContext线程不安全

        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(t => t.IsAssignableTo<IServiceBase>() || t.IsAssignableTo<IMessageHandler>())
            .AsSelf()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope(); // 避免DbContext线程不安全
    }
}