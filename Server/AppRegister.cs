using Autofac;
using Sunny.Framework.DB.Repository;
using WishServer.Service;

namespace WishServer
{
    public class AppRegister : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .AsClosedTypesOf(typeof(IRepositoryBase<,>))
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.IsAssignableTo<IServiceBase>() || t.IsAssignableTo<IMessageHandler>())
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
